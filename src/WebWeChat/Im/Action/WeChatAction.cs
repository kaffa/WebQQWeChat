﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Utility.Extensions;
using Utility.HttpAction.Action;
using Utility.HttpAction.Event;
using Utility.HttpAction.Service;
using WebWeChat.Im.Core;
using WebWeChat.Im.Event;
using WebWeChat.Im.Module.Impl;
using WebWeChat.Im.Module.Interface;
using WebWeChat.Im.Service.Interface;

namespace WebWeChat.Im.Action
{
    public abstract class WeChatAction : HttpAction
    {
        // 为了防止通知层级混乱，其他action不应该直接操作Context，本action也只是在报告错误时用到了。
        // 其他通知应该先通知到调用action的模块，由模块决定是否需要进一步通知
        private IWeChatContext Context { get; set; }
        protected ILogger Logger { get; set; }
        protected SessionModule Session { get; set; }
        protected StoreModule Store { get; set; }
        protected AccountModule Account { get; set; }

        protected long Timestamp => DateTime.Now.ToTimestamp();
        protected string ActionName => GetType().Name;

        protected WeChatAction(IWeChatContext context, ActionEventListener listener = null) :
            base(context.GetSerivce<IHttpService>())
        {
            SetContext(context);
            OnActionEvent += listener;
        }

        public void SetContext(IWeChatContext context)
        {            
            if (context == Context) return;
            Context = context;
            HttpService = context.GetSerivce<IHttpService>();
            Logger = context.GetSerivce<ILogger>();
            Session = context.GetModule<SessionModule>();
            Store = context.GetModule<StoreModule>();
            Account = context.GetModule<AccountModule>();
        }

        public override Task<ActionEvent> HandleExceptionAsync(Exception ex)
        {
            var exception = ex as WeChatException ?? new WeChatException(ex);
            return base.HandleExceptionAsync(exception);
        }

        protected Task<ActionEvent> NotifyErrorEventAsync(WeChatException ex)
        {
            return NotifyActionEventAsync(ActionEvent.CreateEvent(ActionEventType.EvtError, ex));
        }

        protected Task<ActionEvent> NotifyErrorEventAsync(WeChatErrorCode code)
        {
            return NotifyErrorEventAsync(WeChatException.CreateException(code));
        }

        public override async Task<ActionEvent> ExecuteAsync(CancellationToken token)
        {
            Logger.LogTrace($"[Action={ActionName} Begin]");
            var result = await base.ExecuteAsync(token).ConfigureAwait(false);
            Logger.LogTrace($"[Action={ActionName} End]");
            return result;
        }

        protected override Task<ActionEvent> NotifyActionEventAsync(ActionEvent actionEvent)
        {
            var type = actionEvent.Type;
            var target = actionEvent.Target;

            switch (type)
            {
                case ActionEventType.EvtError:
                    {
                        var ex = (WeChatException)target;
                        Logger.LogError($"[Action={ActionName}, Type={type}, {ex}");
                        Context.FireNotify(new WeChatNotifyEvent(WeChatNotifyEventType.Error, ex));
                        break;
                    }
                case ActionEventType.EvtRetry:
                    {
                        var ex = (WeChatException)target;
                        Logger.LogWarning($"[Action={ActionName}, Type={type}, RetryTimes={RetryTimes}][{ex.ToSimpleString()}]");
                        break;
                    }
                case ActionEventType.EvtCanceled:
                    Logger.LogInformation($"[Action={ActionName}, Type={type}, Target={target}]");
                    break;

                default:
                    Logger.LogDebug($"[Action={ActionName}, Type={type}]");
                    break;
            }
            return base.NotifyActionEventAsync(actionEvent);
        }
    }
}
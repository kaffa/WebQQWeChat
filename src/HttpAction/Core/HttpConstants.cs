﻿namespace HttpAction.Core
{
    public static class HttpConstants
    {
        public const string Host = "Host";
        public const string Location = "Location";
        public const string UserAgent = "User-Agent";
        public const string Referrer = "Referer";
        public const string ContentType = "Content-Type";
        public const string ContentLength = "Content-Length";
        public const string SetCookie = "Set-Cookie";
        public const string Origin = "Origin";
        public const string Cookie = "Cookie";
        public const string DefaultGetContentType = "text/html";
        public const string DefaultPostContentType = FormContentType;
        public const string DefaultUserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";
        public const string JsonContentType = "application/json";
        public const string FormContentType = "application/x-www-form-urlencoded";
    }
}

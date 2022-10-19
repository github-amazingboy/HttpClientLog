using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Amazing.HttpClientLog
{
    public static class DelegatingHandlerBuilderExtension
    {
        public static DelegatingHandlerBuilder IfMethod(this DelegatingHandlerBuilder delegatingHandlerBuilder, HttpMethod httpMethod)
        {
            delegatingHandlerBuilder.RequestMatcher = new Func<HttpRequestMessage, Task<bool>>((message) => Task.FromResult(message.Method == httpMethod));
            return delegatingHandlerBuilder;
        }

        public static DelegatingHandlerBuilder IfHost(this DelegatingHandlerBuilder delegatingHandlerBuilder, string host)
        {
            if (host == null)
            {
                return delegatingHandlerBuilder;
            }
            delegatingHandlerBuilder.RequestMatcher = new Func<HttpRequestMessage, Task<bool>>((message) => Task.FromResult(message.RequestUri.Host.ToLower() == host.ToLower().Trim()));
            return delegatingHandlerBuilder;
        }
        public static DelegatingHandlerBuilder IfUriContain(this DelegatingHandlerBuilder delegatingHandlerBuilder, string uripart)
        {
            delegatingHandlerBuilder.RequestMatcher = uripart == null ? null : new Func<HttpRequestMessage, Task<bool>>((message) => Task.FromResult(message.RequestUri.AbsoluteUri.Contains(uripart)));
            return delegatingHandlerBuilder;
        }
        public static DelegatingHandlerBuilder IfUriRegex(this DelegatingHandlerBuilder delegatingHandlerBuilder, string regexPattern)
        {
            if (regexPattern == null)
            {
                return delegatingHandlerBuilder;
            }
            delegatingHandlerBuilder.RequestMatcher = new Func<HttpRequestMessage, Task<bool>>((message) => 
            Task.FromResult(System.Text.RegularExpressions.Regex.IsMatch( message.RequestUri.AbsoluteUri,regexPattern)));
            return delegatingHandlerBuilder;
        }


    }
}

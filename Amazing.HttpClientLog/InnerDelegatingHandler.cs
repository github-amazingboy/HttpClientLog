using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Amazing.HttpClientLog
{
    public class InnerDelegatingHandler : DelegatingHandler
    {
        internal bool EnableLog { get; set; }

        internal long LogMaxRequestContentLength { get; set; }
        internal long LogMaxResponseContentLength { get; set; }

        internal Action<IServiceProvider, HttpLog> LogAction { get;  set; }

        internal IServiceProvider SP;
        internal IList<Action<IServiceProvider, HttpRequestMessage, CancellationToken>> beforeActions = new List<Action<IServiceProvider, HttpRequestMessage, CancellationToken>>();

        internal IList<Action<IServiceProvider, HttpRequestMessage, HttpResponseMessage>> afterActions = new List<Action<IServiceProvider, HttpRequestMessage, HttpResponseMessage>>();

        internal Func<HttpRequestMessage, Task<bool>> _requestMatcher;
        internal Func<HttpRequestMessage, string> SetRequestIDAction { get; set; }
        private ILogger<InnerDelegatingHandler> logger;
        public InnerDelegatingHandler()
        {

        }
        public InnerDelegatingHandler(IServiceProvider SP)
        {
            this.SP = SP;
            if(this.SP != null)
                this.logger = this.SP.GetService<ILogger<InnerDelegatingHandler>>();
        }
        public InnerDelegatingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {

        }

        public InnerDelegatingHandler(IServiceProvider SP, HttpMessageHandler innerHandler,ILogger<InnerDelegatingHandler> logger) : base(innerHandler)
        {
            this.SP = SP;
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpLog httpLog = null;


            bool match = true;
            if (_requestMatcher != null)
            {
                if (await _requestMatcher(request))
                {
                    foreach (var item in beforeActions)
                    {
                        item?.Invoke(this.SP, request, cancellationToken);
                    }
                }
                else
                {
                    match = false;
                }
            }
            else
            {
                foreach (var item in beforeActions)
                {
                    item?.Invoke(this.SP, request, cancellationToken);
                }
            }
            if (this.EnableLog)
            {
                httpLog = new HttpLog();
                string body = null;
                if(request.Content!= null && this.LogMaxRequestContentLength > request.Content.Headers.ContentLength)
                {
                    body = await request.Content.ReadAsStringAsync();
                }
                else
                {
                    if (request.Content != null && this.LogMaxRequestContentLength < request.Content.Headers.ContentLength)
                    {
                        body = $"request content length greater than the setting [{this.LogMaxRequestContentLength}] value";
                    }
                }
                httpLog.Url = request.RequestUri.AbsoluteUri;
                httpLog.HeaderJson = JsonSerializer.Serialize(request.Headers.ToDictionary(h => h.Key, h => h.Value));
                httpLog.RequestBody = body;
                httpLog.Method = request.Method.Method;
                httpLog.RequestTime = DateTime.Now;
                if(this.SetRequestIDAction != null)
                {
                    httpLog.RequestID = this.SetRequestIDAction?.Invoke(request);
                }
            }
            var response = await base.SendAsync(request, cancellationToken);
            if (this.EnableLog)
            {
                string responseContent = null;
                if (request.Content != null && this.LogMaxResponseContentLength > response.Content.Headers.ContentLength)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    if (request.Content != null && this.LogMaxResponseContentLength < response.Content.Headers.ContentLength)
                    {
                        responseContent = $"response content length greater than the setting [{this.LogMaxRequestContentLength}] value";
                    }
                }
                httpLog.FinishedTime = DateTime.Now;
                httpLog.ResponseStatusCode = response.StatusCode.ToString();
                httpLog.DurationTime = httpLog.FinishedTime - httpLog.RequestTime;
                httpLog.ReasonPhrase = response.ReasonPhrase;
                httpLog.ResponseBody = responseContent;

                if(this.logger != null)
                {
                    this.logger.LogTrace(JsonSerializer.Serialize(httpLog));
                }
                if(this.LogAction != null)
                {
                    this.LogAction?.Invoke(this.SP, httpLog);
                }
            }
            if (match)
            {
                foreach (var item in afterActions)
                {
                    item?.Invoke(this.SP, request, response);
                }
            }
            return response;
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Collections.Generic;

namespace Amazing.HttpClientLog
{
    public class DelegatingHandlerBuilder
    {
        private IServiceProvider SP;

        internal Func<HttpRequestMessage, Task<bool>> RequestMatcher { get; set; }
        private Func<HttpRequestMessage,string> SetRequestIDAction { get; set; }

        private IList<Action<IServiceProvider, HttpRequestMessage, CancellationToken>> beforeActions = new List<Action<IServiceProvider, HttpRequestMessage, CancellationToken>>();

        private IList<Action<IServiceProvider, HttpRequestMessage, HttpResponseMessage>> afterActions = new List<Action<IServiceProvider, HttpRequestMessage, HttpResponseMessage>>();

        private bool EnableLogFlag;
        private Action<IServiceProvider, HttpLog> LogAction;
        private long MaxRequestByteLength = 10 * 1024; //10KB
        private long MaxResponseByteLength = 10 * 1024;//10KB

        public DelegatingHandlerBuilder()
        {

        }

        public DelegatingHandlerBuilder(IServiceProvider sp)
        {
            this.SP = sp;
        }

        public DelegatingHandlerBuilder If(Predicate<HttpRequestMessage> predicate)
        {
            RequestMatcher = predicate == null ? null : new Func<HttpRequestMessage, Task<bool>>((message) => Task.FromResult(predicate(message)));
            return this;
        }

        public DelegatingHandlerBuilder BeforeSend(Action<HttpRequestMessage, CancellationToken> changeMessage)
        {
            Action<IServiceProvider, HttpRequestMessage, CancellationToken> tt = (sp, req, cancel) => changeMessage?.Invoke(req, cancel);
            beforeActions.Add(tt);
            return this;
        }

        public DelegatingHandlerBuilder BeforeSend(Action<HttpRequestMessage> changeMessage)
        {
            Action<IServiceProvider, HttpRequestMessage, CancellationToken> tt = (sp, req, cancel) => changeMessage?.Invoke(req);
            this.BeforeSend(tt);
            return this;
        }

        public DelegatingHandlerBuilder BeforeSend(Action<IServiceProvider, HttpRequestMessage, CancellationToken> changeMessage)
        {
            beforeActions.Add(changeMessage);
            return this;
        }


        public DelegatingHandlerBuilder AfterSent(Action<IServiceProvider, HttpRequestMessage, HttpResponseMessage> changeMessage)
        {
            afterActions.Add(changeMessage);
            return this;
        }

        public DelegatingHandlerBuilder AfterSent(Action<HttpRequestMessage, HttpResponseMessage> changeMessage)
        {
            Action<IServiceProvider, HttpRequestMessage, HttpResponseMessage> tt = ( sp,  req, res) => { changeMessage?.Invoke(req, res); };
            afterActions.Add(tt);
            return this;
        }

        public DelegatingHandlerBuilder EnableLog()
        {
            this.EnableLogFlag = true;
            return this;
        }

        public DelegatingHandlerBuilder Log(Action<IServiceProvider ,HttpLog> action)
        {
            this.LogAction = action;
            return this;
        }

        public  DelegatingHandlerBuilder LogRequestID(Func<HttpRequestMessage, string> setRequestIDfunc)
        {
            this.SetRequestIDAction = setRequestIDfunc;
            return this;
        }

        public DelegatingHandlerBuilder LogMaxContentLength(long maxRequestContentLength = 0,long maxResponseByteLength = 0)
        {
            this.MaxRequestByteLength = maxRequestContentLength;
            this.MaxResponseByteLength = maxResponseByteLength;
            return this;
        }

        public DelegatingHandler Build()
        {
            return Build(this.SP);
        }

        public DelegatingHandler Build(IServiceProvider SP)
        {
            this.SP = SP;
            var idh = new InnerDelegatingHandler(this.SP);
            idh.beforeActions = this.beforeActions;
            idh.afterActions = this.afterActions;
            idh._requestMatcher = this.RequestMatcher;
            idh.EnableLog = this.EnableLogFlag;
            idh.LogAction = this.LogAction;
            idh.SetRequestIDAction = this.SetRequestIDAction;
            idh.LogMaxRequestContentLength = this.MaxRequestByteLength;
            idh.LogMaxResponseContentLength = this.MaxResponseByteLength;
            return idh;
        }

    }

}

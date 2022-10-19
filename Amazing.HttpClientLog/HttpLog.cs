using System;
using System.Collections.Generic;
using System.Text;

namespace Amazing.HttpClientLog
{
    public class HttpLog
    {
        public DateTime RequestTime { get; set; }

        public DateTime FinishedTime { get; set; }

        public TimeSpan DurationTime { get; set; }

        public string RequestID { get; set; }

        public string Url { get; set; }

        public string HeaderJson { get; set; }

        public string Method { get; set; }

        public string RequestBody { get; set; }

        public string ResponseStatusCode { get; set; }

        public string ReasonPhrase { get; set; }

        public string ResponseBody { get; set; }
    }
}

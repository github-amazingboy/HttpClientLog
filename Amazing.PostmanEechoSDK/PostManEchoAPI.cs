using System.Net.Http;
using System.Net.Http.Json;

namespace Amazing.PostmanEechoSDK
{
    public class PostManEchoAPI
    {
        private readonly HttpClient _client;

        public PostManEchoAPI(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient();
        }

        public async Task<string> GetUsers()
        {
            var reqmessage = new HttpRequestMessage(HttpMethod.Get, new Uri("https://postman-echo.com/get?foo1=bar1&foo2=bar2"));
            HttpResponseMessage httpResponseMessage = await _client.SendAsync(reqmessage);
            return await httpResponseMessage.Content.ReadAsStringAsync();
        }

        public async Task<string> Post()
        {
            HttpResponseMessage httpResponseMessage = await _client.PostAsJsonAsync("https://postman-echo.com/post?foo1=bar1&foo2=bar2", new {hello="day"});
            return await httpResponseMessage.Content.ReadAsStringAsync();
        }

    }
}
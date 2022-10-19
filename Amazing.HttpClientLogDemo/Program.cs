using Amazing.HttpClientLog;
using Amazing.PostmanEechoSDK;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

DelegatingHandlerBuilder dhb = new DelegatingHandlerBuilder();
dhb.IfUriContain("postman-echo.com").BeforeSend((sp, req, cancel) =>
{
    req.Headers.Add("foolish", "bar");
}).BeforeSend((sp, req, cancel) =>
{
    req.Headers.Add("foolish2", "bar2");
}).AfterSent(async (sp, req, res) =>
{
    var obj = await res.Content.ReadAsStringAsync();
    _ = Task.Run(() =>
    {
        System.Diagnostics.Debug.WriteLine("================postman-echo.com=================");
        System.Diagnostics.Debug.WriteLine(obj);
    });
});

DelegatingHandlerBuilder dhb2 = new DelegatingHandlerBuilder();
dhb2.IfUriContain("get").BeforeSend((sp, req, cancel) =>
{
    req.Headers.Add("yayago", "bar");
}).BeforeSend( async (request) =>
{
    request.Headers.Add("yayago2", "bar2");

    var body = request.Content == null ? null : await request.Content.ReadAsStringAsync();
    
}).AfterSent(async (sp, req, res) =>
{
    var obj = await res.Content.ReadAsStringAsync();
    _ = Task.Run(() =>
    {
        System.Diagnostics.Debug.WriteLine("================get=================");
        System.Diagnostics.Debug.WriteLine(obj);
    });
}).AfterSent(async (sp, req, res) =>
{
    var obj = await res.Content.ReadAsStringAsync();
    _ = Task.Run(() =>
    {
        System.Diagnostics.Debug.WriteLine("======********************======");
        System.Diagnostics.Debug.WriteLine(obj);
    });
});

DelegatingHandlerBuilder dhb3 = new DelegatingHandlerBuilder();
dhb3.AfterSent(async (sp, req, res) =>
{
    var obj = await res.Content.ReadAsStringAsync();
    _ = Task.Run(() =>
    {
        System.Diagnostics.Debug.WriteLine("++++++++++++++no condition++++++++++++++++");
        System.Diagnostics.Debug.WriteLine(obj);
    });
});


DelegatingHandlerBuilder dhb4 = new DelegatingHandlerBuilder();
dhb4.IfUriContain("post").BeforeSend(req =>
{
    //System.Diagnostics.Debug.WriteLine($"contentLength {req.Headers.ToDictionary(h => h.Key, h => h.Value)["Content-Length"]}");
    req.Headers.Add("X-RequestID", Guid.NewGuid().ToString());
})
    .LogRequestID(req =>
{
    req.Headers.TryGetValues("X-RequestID",out IEnumerable<string>? values);
    return values?.FirstOrDefault();
}).EnableLog()
.LogMaxContentLength(5,50).Log((sp, httplog) =>
{
    System.Diagnostics.Debug.WriteLine("===================httplog start==================");
    System.Diagnostics.Debug.WriteLine($"httplog { System.Text.Json.JsonSerializer.Serialize(httplog) }");
    System.Diagnostics.Debug.WriteLine("===================httplog end==================");
}).BeforeSend(async req =>
{
    var reqobj1 = await req.Content?.ReadAsStringAsync();
    System.Diagnostics.Debug.WriteLine("reqobj at before: " + reqobj1);

}).AfterSent(async (sp, req, res) =>
{
    var reqobj1 = await req.Content?.ReadAsStringAsync();
    var obj = await res.Content.ReadAsStringAsync();
    _ = Task.Run(() =>
    {
        System.Diagnostics.Debug.WriteLine("======****** POST *******======");
        System.Diagnostics.Debug.WriteLine("reqobj: " + reqobj1);
        System.Diagnostics.Debug.WriteLine("resobj: " + obj);
    });
});

builder.Services.AddHttpClient(Options.DefaultName)
    .AddHttpMessageHandler(() => dhb.Build())
    .AddHttpMessageHandler(sp => dhb2.Build(sp))
    .AddHttpMessageHandler(sp => dhb3.Build(sp))
    .AddHttpMessageHandler(sp => dhb4.Build(sp)); 


builder.Services.AddTransient<PostManEchoAPI>();

var app = builder.Build();

app.MapGet("/", async (PostManEchoAPI api) =>
{
    //string content = await api.GetUsers();

    string content = await api.Post();

    return content;
}
);

app.Run();



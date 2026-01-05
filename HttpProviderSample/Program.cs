using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 複数のポートを listen
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
    options.ListenLocalhost(5001);
    options.ListenLocalhost(5002);
});

var app = builder.Build();

app.MapPost("/context", async (HttpContext context) =>
{
    var localPort = context.Connection.LocalPort;

    string requestBody;
    using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
        requestBody = await reader.ReadToEndAsync();
    }

    var contextItems = new List<ContextItem>
    {
        new ContextItem
        {
            Name = $"ポート {localPort} で受信",
            Description = $"ポート {localPort} のリクエスト",
            Content = requestBody
        }
    };

    var response = new ContextResponse(contextItems);

    // 応答する ContextItem をトレース出力
    var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    var responseJson = JsonSerializer.Serialize(response, jsonOptions);
    Console.WriteLine($"[Port {localPort}] 応答: {responseJson}");

    await context.Response.WriteAsJsonAsync(response);
});

await app.RunAsync();

record ContextItem
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Content { get; set; }
}

record ContextResponse(List<ContextItem> Items);

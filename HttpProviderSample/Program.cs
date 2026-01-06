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

    // 1. 要求データをトレース出力
    var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    Console.WriteLine($"[Port {localPort}] 要求データ:");
    Console.WriteLine(requestBody);

    // リクエストをパースして詳細を表示
    try
    {
        var requestData = JsonSerializer.Deserialize<RequestData>(requestBody);
        if (requestData != null)
        {
            Console.WriteLine($"  - query: {requestData.query}");
            Console.WriteLine($"  - fullInput: {requestData.fullInput}");
            Console.WriteLine($"  - workspacePath: {requestData.workspacePath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [警告] リクエストのパースに失敗: {ex.Message}");
    }

    // 2. 応答データは2つのコンテキストアイテムをMarkdown形式で返す
    var contextItems = new List<ContextItem>
    {
        new ContextItem
        {
            name = $"HTTP Provider Response (Port {localPort})",
            description = $"受信したリクエスト情報 - ポート {localPort}",
            content = $"""
                # リクエスト情報

                ## 接続情報
                - **ポート番号**: {localPort}
                - **リモートアドレス**: {context.Connection.RemoteIpAddress}

                ## リクエストボディ
                ```json
                {requestBody}
                ```
                """
        },
        new ContextItem
        {
            name = $"Server Info (Port {localPort})",
            description = $"サーバー情報 - ポート {localPort}",
            content = $"""
                # サーバー情報

                ## 環境
                - **ホスト名**: {System.Environment.MachineName}
                - **OS**: {System.Environment.OSVersion}
                - **プロセスID**: {System.Environment.ProcessId}

                ## タイムスタンプ
                - **受信時刻**: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}

                ## ステータス
                ✅ HTTP Context Provider は正常に動作しています
                """
        }
    };

    // 応答する ContextItem をトレース出力
    var responseJson = JsonSerializer.Serialize(contextItems, jsonOptions);
    Console.WriteLine($"[Port {localPort}] 応答データ:");
    Console.WriteLine(responseJson);

    await context.Response.WriteAsJsonAsync(contextItems);
});

await app.RunAsync();

// Continue.dev HTTP Context Provider API に準拠したデータモデル
record RequestData
{
    public string? query { get; set; }
    public string? fullInput { get; set; }
    public object? options { get; set; }
    public string? workspacePath { get; set; }
}

record ContextItem
{
    public string? name { get; set; }
    public string? description { get; set; }
    public string? content { get; set; }
}

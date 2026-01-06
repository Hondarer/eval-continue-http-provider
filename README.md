# eval-continue-http-provider

Continue.dev の HTTP Context Provider API を検証するためのサンプル実装です。  
ASP.NET Core を使用して、複数のポートでリクエストを受け付け、Markdown 形式でコンテキスト情報を返すシンプルな HTTP サーバーを提供します。

## 概要

このプロジェクトは、Continue.dev が提供する HTTP Context Provider の仕様に準拠したサンプルサーバーです。  
Continue.dev は AI コーディングアシスタントであり、HTTP Context Provider を通じて外部からコンテキスト情報を取得できます。

主な目的は、HTTP Context Provider の動作を理解し、カスタムプロバイダーの実装方法を検証することです。

## 主な機能

複数ポートでの同時リッスン機能を備えており、ポート 5000、5001、5002 で同時に待ち受けます。  
これにより、異なる設定やテストケースを並行して試すことができます。

リクエストとレスポンスのトレース出力機能があり、受信したリクエストデータと返送するレスポンスデータをコンソールに JSON 形式で出力します。  
これにより、Continue.dev との通信内容を詳細に確認できます。

Markdown 形式のレスポンスを生成し、リクエスト情報とサーバー情報の 2 つのコンテキストアイテムを返します。  
各アイテムは見出し、リスト、コードブロックなどの Markdown 記法を使用して整形されています。

## 環境要件

このプロジェクトを実行するには .NET 10.0 SDK が必要です。

## ビルドと実行

プロジェクトのビルドは、プロジェクトディレクトリ (HttpProviderSample) で以下のコマンドを実行します。

```bash
dotnet build
```

サーバーの起動は、ビルド後に以下のコマンドで実行します。

```bash
dotnet run
```

サーバーが起動すると、`http://localhost:5000`、`http://localhost:5001`、`http://localhost:5002` の 3 つのポートでリクエストを待ち受けます。

## API 仕様

### エンドポイント

`POST /context` エンドポイントでコンテキスト情報を提供します。

### リクエスト形式

Continue.dev の HTTP Context Provider 仕様に準拠した JSON データを受け付けます。

```json
{
  "query": "検索クエリ",
  "fullInput": "完全な入力テキスト",
  "options": {},
  "workspacePath": "/path/to/workspace"
}
```

### レスポンス形式

2 つのコンテキストアイテムを含む JSON 配列を返します。各アイテムは以下の構造を持ちます。

```json
[
  {
    "name": "HTTP Provider Response (Port 5000)",
    "description": "受信したリクエスト情報 - ポート 5000",
    "content": "Markdown 形式のコンテンツ"
  },
  {
    "name": "Server Info (Port 5000)",
    "description": "サーバー情報 - ポート 5000",
    "content": "Markdown 形式のサーバー情報"
  }
]
```

`content` フィールドには、リクエスト情報 (ポート番号、リモートアドレス、リクエストボディ) とサーバー情報 (ホスト名、OS、プロセス ID、タイムスタンプ) が Markdown 形式で含まれます。

## プロジェクト構成

```text
eval-continue-http-provider/
|-- HttpProviderSample/
|   |-- Program.cs                    (メインプログラム)
|   +-- HttpProviderSample.csproj
|-- docs-src/
|   |-- screenshots.md
|   +-- images/
|       +-- HttpProviderSample_3.png
|-- eval-continue-http-provider.sln
|-- LICENSE
+-- README.md
```

## スクリーンショット

実際の動作例については [docs-src/screenshots.md](docs-src/screenshots.md) を参照してください。

## ライセンス

[LICENSE](LICENSE) ファイルを参照してください。

## 関連リンク

- [Continue.dev](https://continue.dev/) - AI コーディングアシスタント
- [Continue.dev Context Providers Documentation](https://docs.continue.dev/features/context-providers) - Context Provider の公式ドキュメント

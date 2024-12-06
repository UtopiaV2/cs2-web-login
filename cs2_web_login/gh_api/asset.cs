using System.Text.Json.Serialization;

namespace cs2_web_login;

public record Asset(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("node_id")] string NodeId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("uploader")] Uploader Uploader,
    [property: JsonPropertyName("content_type")] string ContentType,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("download_count")] int DownloadCount,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
    [property: JsonPropertyName("browser_download_url")] string BrowserDownloadUrl
);

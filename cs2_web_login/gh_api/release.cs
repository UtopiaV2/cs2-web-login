using System.Text.Json.Serialization;

namespace cs2_web_login;

public record Release(
  [property: JsonPropertyName("url")] string Url,
  [property: JsonPropertyName("assets_url")] string AssetsUrl,
  [property: JsonPropertyName("upload_url")] string UploadUrl,
  [property: JsonPropertyName("html_url")] string HtmlUrl,
  [property: JsonPropertyName("id")] int Id,
  [property: JsonPropertyName("author")] Author Author,
  [property: JsonPropertyName("node_id")] string NodeId,
  [property: JsonPropertyName("tag_name")] string TagName,
  [property: JsonPropertyName("target_commitish")] string TargetCommitish,
  [property: JsonPropertyName("name")] string Name,
  [property: JsonPropertyName("draft")] bool Draft,
  [property: JsonPropertyName("prerelease")] bool Prerelease,
  [property: JsonPropertyName("created_at")] DateTime CreatedAt,
  [property: JsonPropertyName("published_at")] DateTime PublishedAt,
  [property: JsonPropertyName("assets")] IReadOnlyList<Asset> Assets,
  [property: JsonPropertyName("tarball_url")] string TarballUrl,
  [property: JsonPropertyName("zipball_url")] string ZipballUrl,
  [property: JsonPropertyName("body")] string Body
);
/*Vim: set expandtab tabstop=2 shiftwidth=2:*/

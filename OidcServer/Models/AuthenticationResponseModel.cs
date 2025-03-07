using System.Text.Json.Serialization;

namespace OidcServer.Models;

public class AuthenticationResponseModel : TokenResponseModel
{
    [JsonPropertyName("id_token")] public required string IdToken { get; set; }
}
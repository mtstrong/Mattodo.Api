using Microsoft.AspNetCore.Authentication;

namespace Mattodo.Api.Auth;

public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = "41b711d305714e0eb298a50eeb569e83";
}

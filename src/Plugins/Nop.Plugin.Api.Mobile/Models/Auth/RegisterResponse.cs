namespace Nop.Plugin.Api.Mobile.Models.Auth;

public class RegisterResponse
{
    public bool Registered { get; set; } = true;

    public string AccessToken { get; set; }

    public string TokenType { get; set; }

    public int ExpiresIn { get; set; }

    public bool RequiresEmailValidation { get; set; }

    public bool RequiresApproval { get; set; }
}

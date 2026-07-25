using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Mobile.Models.Auth;

public class LoginRequest
{
    [Required]
    public string UsernameOrEmail { get; set; }

    [Required]
    public string Password { get; set; }
}

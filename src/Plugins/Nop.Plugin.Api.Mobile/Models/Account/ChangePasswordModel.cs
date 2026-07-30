using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Mobile.Models.Account;

public class ChangePasswordModel
{
    [Required]
    public string OldPassword { get; set; }

    [Required]
    public string NewPassword { get; set; }
}

namespace Nop.Plugin.Api.Mobile.Models.Account;

public class UpdateProfileRequest
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Company { get; set; }

    public string Phone { get; set; }

    public string Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }
}

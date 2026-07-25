namespace Nop.Plugin.Api.Mobile.Models.Customers;

public class CustomerProfileModel
{
    public int Id { get; set; }

    public string Email { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName { get; set; }

    public string Company { get; set; }

    public string Phone { get; set; }

    public string Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime CreatedOnUtc { get; set; }
}

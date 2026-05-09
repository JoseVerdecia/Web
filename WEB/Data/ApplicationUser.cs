using Microsoft.AspNetCore.Identity;

namespace WEB.Data;


public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
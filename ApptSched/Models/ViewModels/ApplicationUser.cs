using Microsoft.AspNetCore.Identity;

namespace ApptSched.Models.ViewModels
{
    public class ApplicationUser: IdentityUser
    {
        public string Name { get; set; }

    }
}

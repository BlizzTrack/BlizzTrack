using Microsoft.AspNetCore.Identity;

namespace Core.Models
{
    public class User : IdentityUser
    {
        public string AccessToken { get; set; }

        public string Avatar { get; set; }

        public string BattleTag { get; set; }
    }
}
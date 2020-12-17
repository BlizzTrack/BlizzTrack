using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class User : IdentityUser
    {
        public string AccessToken { get; set; }

        public string Avatar { get; set; }

        public string BattleTag { get; set; }
    }
}

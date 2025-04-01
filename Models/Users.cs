using Microsoft.AspNetCore.Identity;

namespace projekt_.NET_schronisko.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}

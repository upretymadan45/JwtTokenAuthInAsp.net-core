using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenAuthDemo.Models
{
    public class UserRefreshToken
    {
        public string Username { get; set; }
        public string RefreshToken { get; set; }
    }
}

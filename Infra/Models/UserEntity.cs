using Infra.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Models
{
    public class UserEntity
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string RoleType {get;set;}
        public string Password { get; set; }    
    }
}

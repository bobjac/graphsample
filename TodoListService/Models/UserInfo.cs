using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListService.Models
{
    public class UserInfo
    {
        public string Surname { get; set; }
        public string GivenName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string MobilePhone { get; set; }
        public string Gender { get; set; }
    }
}

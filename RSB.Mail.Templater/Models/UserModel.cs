using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSB.Mail.Templater.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsPremiumUser { get; set; }
    }
}

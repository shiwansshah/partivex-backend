using System;
using System.Collections.Generic;
using System.Text;

namespace Partivex.Domain.Entities
{
    public class ApplicationUser:IdentityUser
    {
        public string FullName { get; set; }
    }
}

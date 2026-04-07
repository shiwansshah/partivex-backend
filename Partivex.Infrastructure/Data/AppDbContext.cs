using System;
using System.Collections.Generic;
using System.Text;

namespace Partivex.Infrastructure.Data
{
    public class AppDbContext:IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
        {
        }
    }
}

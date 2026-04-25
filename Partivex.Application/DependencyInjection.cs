using System;
using System.Collections.Generic;
using System.Text;

namespace Partivex.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            return services;
        }
    }
}

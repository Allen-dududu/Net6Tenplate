using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Util
{
    public static class GlobalContext
    {

        /// <summary>
        /// All registered service and class instance container. Which are used for dependency injection.
        /// </summary>
        public static IServiceCollection Services { get; set; }

        /// <summary>
        /// Configured service provider.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; set; }

        public static IConfiguration Configuration { get; set; }

        public static IServiceCollection AddGlobalContext(this IServiceCollection services,IServiceProvider serviceProvider, IConfiguration configuration)
        {
            Services = services;
            ServiceProvider = serviceProvider;
            Configuration = configuration;  

            return services;
        }

    }
}

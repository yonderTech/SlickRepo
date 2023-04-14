using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SlickRepo
{
    public class SlickRepoConfig
    {
        /// <summary>
        /// The property used to look up by unique key in the DbSet
        /// </summary>
        public string DbIdProperty { get; set; }
        /// <summary>
        /// The property on the DTO that allows to retrieve the unique key to look up for in DbSet
        /// </summary>
        public string DtoIdProperty { get; set; }
    }

    public static class SlickRepoServiceCollectionExtensions
    {
        /// <summary>
        /// Configure SlickRepo. Used to specified what are the unique keys for both models provided in generic constraints
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <param name="serviceLifetime"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureSlickRepo(this IServiceCollection services,
            Action<SlickRepoConfig> setupAction)
        {

            SlickRepoConfig config = new SlickRepoConfig();
            setupAction.Invoke(config);
            services.AddSingleton(config);
            return services;
        }

    }
}

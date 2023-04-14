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
        public string DbIdProperty { get; set; }
        public string DtoIdProperty { get; set; }
    }

    public static class SlickRepoServiceCollectionExtensions
    {
        public static IServiceCollection AddSlickRepo<TDbModel, TDtoModel>(this IServiceCollection services,
            Action<SlickRepoConfig> setupAction, ServiceLifetime serviceLifetime) where TDbModel : class 
            where TDtoModel : class
        {

            SlickRepoConfig config = new SlickRepoConfig();
            setupAction.Invoke(config);
            services.AddSingleton(config);
            
            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(CreateBaseRepo<TDbModel, TDtoModel>(services, config));
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient(sp =>
                    {
                        var config = sp.GetService<SlickRepoConfig>();
                        var ctx = sp.GetService<DbContext>();
                        return new SlickRepo<TDbModel, TDtoModel>(ctx, config);
                    });
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(sp =>
                    {
                        Debugger.Break();
                        var config = sp.GetService<SlickRepoConfig>();
                        var ctx = sp.GetService<DbContext>();
                        return new SlickRepo<TDbModel, TDtoModel>(ctx, config);
                    });
                    break;
            }

            return services;
        }


        private static SlickRepo<TDbModel, TDtoModel> CreateBaseRepo<TDbModel, TDtoModel>(IServiceCollection services, SlickRepoConfig config) where TDbModel : class 
            where TDtoModel : class
        {
            var serviceProvider = services.BuildServiceProvider();
            DbContext ctx = serviceProvider.GetService<DbContext>();
            SlickRepo<TDbModel, TDtoModel> baseRepo = new SlickRepo<TDbModel, TDtoModel>(ctx, config);
            
            return baseRepo;
        }

    }
}

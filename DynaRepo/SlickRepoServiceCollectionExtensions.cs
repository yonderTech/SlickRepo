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
    public class SlickRepoConfig<TDbModel, TDtoModel> where TDbModel: class where TDtoModel: class
    {
        public Expression<Func<TDbModel, object>>? DbIdProperty { get; set; }
        public Expression<Func<TDtoModel, object>>? DtoIdProperty { get; set; }
    }

    public static class SlickRepoServiceCollectionExtensions
    {
        public static IServiceCollection AddDynaRepo<TDbModel, TDtoModel>(this IServiceCollection services,
            Action<SlickRepoConfig<TDbModel, TDtoModel>> setupAction, ServiceLifetime serviceLifetime) where TDbModel : class 
            where TDtoModel : class
        {

            SlickRepoConfig<TDbModel, TDtoModel> config = new SlickRepoConfig<TDbModel, TDtoModel>();
            setupAction.Invoke(config);
            services.AddSingleton(config);
            
            switch (serviceLifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(CreateBaseRepo<TDbModel, TDtoModel>(services, config));
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient((Func<IServiceProvider, SlickRepo.SlickRepo<TDbModel, TDtoModel>>)(sp =>
                    {
                        var config = sp.GetService<SlickRepoConfig<TDbModel, TDtoModel>>();
                        var ctx = sp.GetService<DbContext>();
                        return (SlickRepo.SlickRepo<TDbModel,TDtoModel>)new SlickRepo<TDbModel, TDtoModel>(ctx, config);
                    }));
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped((Func<IServiceProvider, SlickRepo.SlickRepo<TDbModel, TDtoModel>>)(sp =>
                    {
                        Debugger.Break();
                        var config = sp.GetService<SlickRepoConfig<TDbModel, TDtoModel>>();
                        var ctx = sp.GetService<DbContext>();
                        return (SlickRepo.SlickRepo<TDbModel,TDtoModel>)new SlickRepo<TDbModel, TDtoModel>(ctx, config);
                    }));
                    break;
            }

            return services;
        }


        private static SlickRepo<TDbModel, TDtoModel> CreateBaseRepo<TDbModel, TDtoModel>(IServiceCollection services, SlickRepoConfig<TDbModel, TDtoModel> config) where TDbModel : class 
            where TDtoModel : class
        {
            var serviceProvider = services.BuildServiceProvider();
            DbContext ctx = serviceProvider.GetService<DbContext>();
            SlickRepo<TDbModel, TDtoModel> baseRepo = new SlickRepo<TDbModel, TDtoModel>(ctx, config);
            
            return baseRepo;
        }

    }
}

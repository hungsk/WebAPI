using EFCoreSecondLevelCacheInterceptor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;

namespace WebAPI.Repository
{
    public static class MsSqlServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguredMsSqlDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextPool<APIDbContext>((serviceProvider, optionsBuilder) =>
                    optionsBuilder
                        .UseSqlServer(
                            connectionString,
                            sqlServerOptionsBuilder =>
                            {
                                sqlServerOptionsBuilder
                                    .CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds)
                                    .EnableRetryOnFailure()
                                    .MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);
                            })
                        .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));
            return services;
        }
    }
}

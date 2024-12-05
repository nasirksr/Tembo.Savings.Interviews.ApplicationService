using Microsoft.Extensions.DependencyInjection;
using Services.Applications.Strategies;
using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;

namespace Services.Applications.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationProcessing(this IServiceCollection services)
        {
            services.AddSingleton<IProductProcessingStrategy, ProductOneProcessingStrategy>();
            services.AddSingleton<IProductProcessingStrategy, ProductTwoProcessingStrategy>();

            services.AddSingleton<IApplicationProcessor>(provider =>
            {
                return new ApplicationProcessor(new Dictionary<ProductCode, IProductProcessingStrategy>
                {
                    { ProductCode.ProductOne, provider.GetRequiredService<ProductOneProcessingStrategy>() },
                    { ProductCode.ProductTwo, provider.GetRequiredService<ProductTwoProcessingStrategy>() }
                });
            });

            return services;
        }
    }
}

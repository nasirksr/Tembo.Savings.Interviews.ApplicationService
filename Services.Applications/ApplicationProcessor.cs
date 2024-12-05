using Services.Applications.Strategies;
using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;

namespace Services.Applications;

public class ApplicationProcessor : IApplicationProcessor
{
    private readonly IDictionary<ProductCode, IProductProcessingStrategy> _strategies;

    public ApplicationProcessor(IDictionary<ProductCode, IProductProcessingStrategy> strategies)
    {
        _strategies = strategies;
    }

    public async Task Process(Application application)
    {
        if (!_strategies.TryGetValue(application.ProductCode, out var strategy))
        {
            throw new InvalidOperationException($"Unsupported product code: {application.ProductCode}");
        }

        await strategy.Process(application);
    }
}
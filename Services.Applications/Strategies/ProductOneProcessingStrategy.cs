using System;
using System.Threading.Tasks;

using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;

namespace Services.Applications.Strategies
{
    public class ProductOneProcessingStrategy : IProductProcessingStrategy
    {
        private readonly IServiceAdministrator _adminOne;
        private readonly IKycService _kycService;
        private readonly IBus _bus;

        public ProductOneProcessingStrategy(IServiceAdministrator adminOne, IKycService kycService, IBus bus)
        {
            _adminOne = adminOne;
            _kycService = kycService;
            _bus = bus;
        }

        public async Task Process(Application application)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            if (application.Applicant == null) throw new ArgumentNullException(nameof(application.Applicant));

            var kycResult = await _kycService.GetKycReportAsync(application.Applicant);
            if (kycResult == null || !kycResult.IsSuccess || kycResult.Value == null || !kycResult.Value.IsVerified)
            {
                await _bus.PublishAsync(new KycFailed(application.Applicant.Id, kycResult?.Value?.Id ?? Guid.Empty));
                return;
            }

            var investorResult = await _adminOne.CreateInvestorAsync(application.Applicant);
            if (!investorResult.IsSuccess)
            {
                throw new InvalidOperationException("Failed to create investor.");
            }

            await _bus.PublishAsync(new InvestorCreated(application.Applicant.Id, investorResult.Value.ToString()));
        }

    }
}

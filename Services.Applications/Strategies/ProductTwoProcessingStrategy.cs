using System;
using System.Threading.Tasks;

using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;

namespace Services.Applications.Strategies
{
    public class ProductTwoProcessingStrategy : IProductProcessingStrategy
    {
        private readonly IServiceAdministrator _adminTwo;
        private readonly IKycService _kycService;
        private readonly IBus _bus;

        public ProductTwoProcessingStrategy(IServiceAdministrator adminTwo, IKycService kycService, IBus bus)
        {
            _adminTwo = adminTwo;
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

            var investorResult = await _adminTwo.CreateInvestorAsync(application.Applicant);
            if (!investorResult.IsSuccess)
            {
                throw new InvalidOperationException("Failed to create investor.");
            }

            var accountResult = await _adminTwo.CreateAccountAsync(investorResult.Value, application.ProductCode);
            if (accountResult == null || !accountResult.IsSuccess)
            {
                throw new InvalidOperationException("Failed to create account.");
            }

            var paymentResult = await _adminTwo.ProcessPaymentAsync(accountResult.Value, application.Payment);
            if (paymentResult == null || !paymentResult.IsSuccess)
            {
                throw new InvalidOperationException("Failed to process payment.");
            }

            await _bus.PublishAsync(new InvestorCreated(application.Applicant.Id, investorResult.Value.ToString()));
            await _bus.PublishAsync(new AccountCreated(investorResult.Value.ToString(), application.ProductCode, accountResult.Value.ToString()));
            await _bus.PublishAsync(new ApplicationCompleted(application.Id));
        }


    }
}

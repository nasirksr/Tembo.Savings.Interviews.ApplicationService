using Services.Common.Abstractions.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Common.Abstractions.Abstractions
{
    public interface IServiceAdministrator
    {
        Task<Result<Guid>> CreateInvestorAsync(User user);
        Task<Result<Guid>> CreateAccountAsync(Guid investorId, ProductCode productCode);
        Task<Result<Guid>> ProcessPaymentAsync(Guid accountId, Payment payment);
    }
}

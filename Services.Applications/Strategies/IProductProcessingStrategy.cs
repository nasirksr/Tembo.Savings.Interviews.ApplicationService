using System.Threading.Tasks;
using Services.Common.Abstractions.Model;

namespace Services.Applications.Strategies
{
    public interface IProductProcessingStrategy
    {
        Task Process(Application application);
    }
}

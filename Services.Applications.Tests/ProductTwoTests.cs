using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services.Applications.Strategies;
using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;

namespace Services.Applications.Tests
{
    [TestClass]
    public class ProductTwoTests
    {
        private Mock<IServiceAdministrator> _adminTwoMock;
        private Mock<IKycService> _kycServiceMock;
        private Mock<IBus> _busMock;
        private ProductTwoProcessingStrategy _strategy;

        [TestInitialize]
        public void Setup()
        {
            _adminTwoMock = new Mock<IServiceAdministrator>();
            _kycServiceMock = new Mock<IKycService>();
            _busMock = new Mock<IBus>();

            _strategy = new ProductTwoProcessingStrategy(_adminTwoMock.Object, _kycServiceMock.Object, _busMock.Object);
        }

        [TestMethod]
        public async Task Application_for_ProductTwo_Creates_Investor_And_Account_Successfully()
        {           
            var application = new Application
            {
                Id = Guid.NewGuid(),
                Applicant = new User
                {
                    Id = Guid.NewGuid(),
                    Forename = "Jane",
                    Surname = "Smith",
                    DateOfBirth = new DateOnly(1985, 5, 20)
                },
                Payment = new Payment(
                    new BankAccount
                    {
                        SortCode = "12-34-56",
                        AccountNumber = "12345678"
                    },
                    new Money("GBP", 100)
                )
            };

            _kycServiceMock
                .Setup(s => s.GetKycReportAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success(new KycReport(Guid.NewGuid(), true)));

            _adminTwoMock
                .Setup(a => a.CreateInvestorAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success(Guid.NewGuid()));

            _adminTwoMock
                .Setup(a => a.CreateAccountAsync(It.IsAny<Guid>(), It.IsAny<ProductCode>()))
                .ReturnsAsync(Result.Success(Guid.NewGuid()));

            _adminTwoMock
                .Setup(a => a.ProcessPaymentAsync(It.IsAny<Guid>(), It.IsAny<Payment>()))
                .ReturnsAsync(Result.Success(Guid.NewGuid()));
           
            await _strategy.Process(application);
         
            _adminTwoMock.Verify(a => a.CreateInvestorAsync(application.Applicant), Times.Once);
            _adminTwoMock.Verify(a => a.CreateAccountAsync(It.IsAny<Guid>(), application.ProductCode), Times.Once);
            _adminTwoMock.Verify(a => a.ProcessPaymentAsync(It.IsAny<Guid>(), application.Payment), Times.Once);
            _busMock.Verify(b => b.PublishAsync(It.IsAny<InvestorCreated>()), Times.Once);
            _busMock.Verify(b => b.PublishAsync(It.IsAny<AccountCreated>()), Times.Once);
            _busMock.Verify(b => b.PublishAsync(It.IsAny<ApplicationCompleted>()), Times.Once);
        }


        [TestMethod]
        public async Task Application_for_ProductTwo_Creates_Investor_Successfully()
        {          
            var application = new Application
            {
                Id = Guid.NewGuid(),
                Applicant = new User
                {
                    Id = Guid.NewGuid(),
                    Forename = "Jane",
                    Surname = "Smith",
                    DateOfBirth = new DateOnly(1985, 5, 20)
                },
                Payment = new Payment(
                    new BankAccount
                    {
                        SortCode = "12-34-56",
                        AccountNumber = "12345678"
                    },
                    new Money("GBP", 100)
                )
            };

            _kycServiceMock
                .Setup(s => s.GetKycReportAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success(new KycReport(Guid.NewGuid(), true)));

            _adminTwoMock
                .Setup(a => a.CreateInvestorAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success(Guid.NewGuid()));

            _adminTwoMock
                .Setup(a => a.CreateAccountAsync(It.IsAny<Guid>(), It.IsAny<ProductCode>()))
                .ReturnsAsync(Result.Success(Guid.NewGuid()));

            _adminTwoMock
                .Setup(a => a.ProcessPaymentAsync(It.IsAny<Guid>(), It.IsAny<Payment>()))
                .ReturnsAsync(Result.Success(Guid.NewGuid()));
           
            await _strategy.Process(application);
            
            _adminTwoMock.Verify(a => a.CreateInvestorAsync(application.Applicant), Times.Once);
            _adminTwoMock.Verify(a => a.CreateAccountAsync(It.IsAny<Guid>(), application.ProductCode), Times.Once);
            _adminTwoMock.Verify(a => a.ProcessPaymentAsync(It.IsAny<Guid>(), application.Payment), Times.Once);
            _busMock.Verify(b => b.PublishAsync(It.IsAny<InvestorCreated>()), Times.Once);
            _busMock.Verify(b => b.PublishAsync(It.IsAny<AccountCreated>()), Times.Once);
            _busMock.Verify(b => b.PublishAsync(It.IsAny<ApplicationCompleted>()), Times.Once);
        }


        [TestMethod]
        public async Task Application_for_ProductTwo_Fails_KYC()
        {
           
            var application = new Application
            {
                Id = Guid.NewGuid(),
                Applicant = new User
                {
                    Id = Guid.NewGuid(),
                    Forename = "Jane",
                    Surname = "Smith",
                    DateOfBirth = new DateOnly(1985, 5, 20)
                }
            };

            _kycServiceMock
                .Setup(s => s.GetKycReportAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Failure<KycReport>(new Error("KYC", "FAILED", "KYC failed")));

          
            await _strategy.Process(application);
          
            _busMock.Verify(b => b.PublishAsync(It.Is<KycFailed>(k => k.UserId == application.Applicant.Id)), Times.Once);
            _adminTwoMock.Verify(a => a.CreateInvestorAsync(It.IsAny<User>()), Times.Never);
        }
    }

}

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Services.Applications.Strategies;
using Services.Common.Abstractions.Abstractions;
using Services.Common.Abstractions.Model;

using System;
using System.Threading.Tasks;

namespace Services.Applications.Tests
{
    [TestClass]
    public class ProductOneTests
    {
        private Mock<IServiceAdministrator> _adminOneMock;
        private Mock<IKycService> _kycServiceMock;
        private Mock<IBus> _busMock;
        private ProductOneProcessingStrategy _strategy;

        [TestInitialize]
        public void Setup()
        {
            _adminOneMock = new Mock<IServiceAdministrator>();
            _kycServiceMock = new Mock<IKycService>();
            _busMock = new Mock<IBus>();

            _strategy = new ProductOneProcessingStrategy(_adminOneMock.Object, _kycServiceMock.Object, _busMock.Object);
        }
        [TestMethod]
        public async Task Application_for_ProductOne_creates_Investor_in_AdministratorOne()
        {           
            var user = new User
            {
                Id = Guid.NewGuid(),
                Forename = "John",
                Surname = "Doe",
                DateOfBirth = new DateOnly(1990, 1, 1),
                Nino = "AB123456C",
                Addresses = new[]
                {
                    new Address
                    {
                        Addressline1 = "123 Main Street",
                        PostCode = "AB1 2CD"
                    }
                },
                BankAccounts = new[]
                {
                    new BankAccount
                    {
                        SortCode = "12-34-56",
                        AccountNumber = "12345678"
                    }
                }
            };

            var application = new Application
            {
                Id = Guid.NewGuid(),
                Applicant = user,
                ProductCode = ProductCode.ProductOne
            };

            _kycServiceMock
                .Setup(s => s.GetKycReportAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success(new KycReport(Guid.NewGuid(), true)));

            _adminOneMock
                .Setup(a => a.CreateInvestorAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success(Guid.NewGuid()));
          
            await _strategy.Process(application);
            
            _adminOneMock.Verify(a => a.CreateInvestorAsync(It.Is<User>(u => u.Id == user.Id)), Times.Once);
            _busMock.Verify(b => b.PublishAsync(It.Is<InvestorCreated>(e => e.UserId == user.Id)), Times.Once);
        }


        [TestMethod]
        public async Task Application_for_ProductOne_Creates_Investor_Successfully()
        {
            // Arrange
            var application = new Application
            {
                Id = Guid.NewGuid(),
                Applicant = new User { Id = Guid.NewGuid() }
            };

            _kycServiceMock
                .Setup(s => s.GetKycReportAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success(new KycReport(Guid.NewGuid(), true)));

            _adminOneMock
                .Setup(a => a.CreateInvestorAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Success(Guid.NewGuid()));

            // Act
            await _strategy.Process(application);

            // Assert
            _adminOneMock.Verify(a => a.CreateInvestorAsync(application.Applicant), Times.Once);
            _busMock.Verify(b => b.PublishAsync(It.IsAny<InvestorCreated>()), Times.Once);
        }

        [TestMethod]
        public async Task Application_for_ProductOne_Fails_KYC()
        {           
            var application = new Application
            {
                Id = Guid.NewGuid(),
                Applicant = new User
                {
                    Id = Guid.NewGuid(),
                    Forename = "John",
                    Surname = "Doe",
                    DateOfBirth = new DateOnly(1990, 1, 1),
                    Nino = "AB123456C",
                    Addresses = new[]
                    {
                        new Address
                        {
                            Addressline1 = "123 Main Street",
                            PostCode = "AB1 2CD"
                        }
                    },
                    BankAccounts = new[]
                    {
                        new BankAccount
                        {
                            SortCode = "12-34-56",
                            AccountNumber = "12345678"
                        }
                    }
                }
            };

            _kycServiceMock
                .Setup(s => s.GetKycReportAsync(It.IsAny<User>()))
                .ReturnsAsync(Result.Failure<KycReport>(new Error("KYC", "FAILED", "KYC failed")));
                       
            await _strategy.Process(application);
            
            _busMock.Verify(b => b.PublishAsync(It.Is<KycFailed>(k => k.UserId == application.Applicant.Id)), Times.Once);
            _adminOneMock.Verify(a => a.CreateInvestorAsync(It.IsAny<User>()), Times.Never);
        }
    }
}

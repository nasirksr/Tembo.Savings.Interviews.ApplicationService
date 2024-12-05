using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Services.Applications;
using Services.Applications.Strategies;
using Services.Common.Abstractions.Model;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Applications.Tests
{
    [TestClass]
    public class CommonTests
    {
        private Mock<IProductProcessingStrategy> _mockStrategy;
        private ApplicationProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _mockStrategy = new Mock<IProductProcessingStrategy>();
            var strategies = new Dictionary<ProductCode, IProductProcessingStrategy>
            {
                { ProductCode.ProductOne, _mockStrategy.Object }
            };

            _processor = new ApplicationProcessor(strategies);
        }

        [TestMethod]
        public async Task ApplicationProcessor_ThrowsException_For_UnsupportedProductCode()
        {           
            var application = new Application
            {
                Id = Guid.NewGuid(),
                ProductCode = (ProductCode)999 
            };            
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _processor.Process(application)
            );
        }

        [TestMethod]
        public async Task ApplicationProcessor_Processes_ValidProduct()
        {          
            var application = new Application
            {
                Id = Guid.NewGuid(),
                ProductCode = ProductCode.ProductOne
            };

            _mockStrategy
                .Setup(s => s.Process(It.IsAny<Application>()))
                .Returns(Task.CompletedTask);
            
            await _processor.Process(application);
            
            _mockStrategy.Verify(s => s.Process(application), Times.Once);
        }
    }
}

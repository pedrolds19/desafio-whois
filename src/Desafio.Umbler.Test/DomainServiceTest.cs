using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Desafio.Umbler.Models;
using Desafio.Umbler.Services;
using Desafio.Umbler.Interfaces;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainServiceTest
    {
        private DatabaseContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new DatabaseContext(options);
        }

        [TestMethod]
        public async Task GetResultAsync_ShouldReturnNull_WhenDomainIsInvalid()
        {
            // ARRANGE
            var whoisMock = new Mock<IWhoisClient>();
            using var context = GetInMemoryContext();
            var service = new DomainService(context, whoisMock.Object);

            // ACT
            var resultEmpty = await service.GetResultAsync("");
            var resultNoDot = await service.GetResultAsync("invalidtext");

            // ASSERT
            Assert.IsNull(resultEmpty, "Deveria retornar null para string vazia");
            Assert.IsNull(resultNoDot, "Deveria retornar null para domínio sem ponto");
        }

        [TestMethod]
        public async Task GetResultAsync_ShouldUseCache_WhenTtlIsValid()
        {

            // ARRANGE
            var domainName = "cached-domain.com";
            var whoisMock = new Mock<IWhoisClient>();
            using var context = GetInMemoryContext();

            context.Domains.Add(new Domain
            {
                Name = domainName,
                Ttl = 600, 
                UpdatedAt = DateTime.Now,
                WhoIs = "Old Data",
                HostedAt = "Old Host",
                Ip = "1.1.1.1"
            });
            context.SaveChanges();

            var service = new DomainService(context, whoisMock.Object);

            // ACT
            var result = await service.GetResultAsync(domainName);

            // ASSERT
            Assert.IsNotNull(result);
            Assert.AreEqual("Old Host", result.HostedAt); 

            whoisMock.Verify(x => x.QueryAsync(It.IsAny<string>()), Times.Never);
            whoisMock.Verify(x => x.GetDnsInformation(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GetResultAsync_ShouldRefreshCache_WhenTtlIsExpired()
        {

            // ARRANGE
            var domainName = "expired-domain.com";
            var whoisMock = new Mock<IWhoisClient>();

            whoisMock.Setup(x => x.QueryAsync(It.IsAny<string>())).ReturnsAsync("Name Server: new.ns.com");
            whoisMock.Setup(x => x.GetDnsInformation(It.IsAny<string>())).ReturnsAsync("2.2.2.2");

            using var context = GetInMemoryContext();

            context.Domains.Add(new Domain
            {
                Name = domainName,
                Ttl = 600, 
                UpdatedAt = DateTime.Now.AddMinutes(-20), 
                WhoIs = "Old Data",
                Ip = "1.1.1.1"
            });
            context.SaveChanges();

            var service = new DomainService(context, whoisMock.Object);

            // ACT
            var result = await service.GetResultAsync(domainName);

            // ASSERT
            whoisMock.Verify(x => x.QueryAsync(domainName), Times.Once);

            Assert.IsTrue(result.NameServers.Contains("new.ns.com"));
        }
    }
}
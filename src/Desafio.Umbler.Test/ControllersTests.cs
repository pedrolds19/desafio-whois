using Desafio.Umbler.Controllers;
using Desafio.Umbler.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Desafio.Umbler.Interfaces;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod]
        public void Home_Index_returns_View()
        {
            var controller = new HomeController();
            var response = controller.Index();
            var result = response as ViewResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Home_Error_returns_View_With_Model()
        {
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var response = controller.Error();
            var result = response as ViewResult;
            var model = result.Model as ErrorViewModel;

            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void Domain_In_Database()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var whoisClient = new Mock<IWhoisClient>();
            var domain = new Domain { Id = 1, Ip = "192.168.0.1", Name = "test.com", UpdatedAt = DateTime.Now, HostedAt = "umbler.corp", Ttl = 600, WhoIs = "Ns.umbler.com" };

            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(domain);
                db.SaveChanges();
            }

            using (var db = new DatabaseContext(options))
            {
                var service = new Desafio.Umbler.Services.DomainService(db, whoisClient.Object);
                var controller = new Desafio.Umbler.Controllers.DomainController(service);

                var response = controller.Get("test.com").Result;
                var result = response as OkObjectResult;

                dynamic obj = result?.Value;

                Assert.IsNotNull(result, "O Controller não retornou Ok (200)");
                Assert.IsNotNull(obj, "O objeto retornado é nulo");

                Assert.AreEqual(domain.Name, obj.Domain);
            }
        }

        [TestMethod]
        public void Domain_Not_In_Database()
        {
            var whoisClient = new Mock<IWhoisClient>();
    
            whoisClient.Setup(x => x.QueryAsync(It.IsAny<string>()))
                       .ReturnsAsync("Domain Name: test.com\nRegistrar: Umbler");

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var db = new DatabaseContext(options))
            {
                var service = new Desafio.Umbler.Services.DomainService(db, whoisClient.Object);
                var controller = new Desafio.Umbler.Controllers.DomainController(service);

                var response = controller.Get("test.com").Result;
                var result = response as OkObjectResult;
                dynamic obj = result?.Value;

                Assert.IsNotNull(result, "Deveria retornar OkObjectResult");
                Assert.IsNotNull(obj, "Deveria retornar um objeto válido");
                Assert.AreEqual("test.com", obj.Domain);
            }
        }

        [TestMethod]
        public void Domain_Moking_WhoisClient()
        {
            var whoisClient = new Mock<IWhoisClient>();
            var domainName = "test.com";

            whoisClient.Setup(l => l.QueryAsync(domainName)).ReturnsAsync("Domain Name: " + domainName);

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var db = new DatabaseContext(options))
            {
                var service = new Desafio.Umbler.Services.DomainService(db, whoisClient.Object);
                var controller = new Desafio.Umbler.Controllers.DomainController(service);

                var response = controller.Get("test.com").Result;
                var result = response as OkObjectResult;
                dynamic obj = result?.Value;

                Assert.IsNotNull(obj);
                Assert.AreEqual(domainName, obj.Domain);
            }
        }
       
    }
}
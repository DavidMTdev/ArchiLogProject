using Archi.Api.Controllers;
using Archi.Api.Models;
using Archi.Library.Models;
using Archi.Test.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Archi.Test
{
    public class CustomerControllerTests
    {
        private CustomersController _controller;
        private MockDbContext _context;

        [SetUp]
        public void Setup()
        {
            _context = MockDbContext.GetDbContext();
            _controller = new CustomersController(_context);
        }

        [Test]
        public Task TestGetAll()
        {
            var mockHandler = new Mock<HttpMessageHandler>();

            var test = mockHandler.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            Assert.AreEqual(HttpStatusCode.OK, test);
        }

        [Test]
        public async Task TestPost()
        {
            var post = await _controller.PostItem(new Customer { Active = true, Lastname = "Elisabette", Firstname = "II", Email = "rien" });
            var result = post.Result as ObjectResult;
            Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
        }

        [Test]
        public async Task TestGetID()
        {
            var post = await _controller.GetByID(1);
            var result = post.Result as ObjectResult;
            // Console.WriteLine(result;
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
        }

        /*[Test]
        public async Task TestDelete()
        {
            var post = await _controller.DeleteCustomer(1);
            var result = post.Result as ObjectResult;
            Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
        }*/


    }
}

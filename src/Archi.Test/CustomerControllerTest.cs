using Archi.Api.Controllers;
using Archi.Api.Models;
using Archi.Library.Models;
using Archi.Test.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        public async Task TestGetAll()
        {
            var actionResult = await _controller.GetAll(new Params(), "https://locaost:8080/api/v1/customers");
            //var result = actionResult.Result as ObjectResult;
            var values = actionResult.Value as IEnumerable<Customer>;

            //Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(values);
            Assert.AreEqual(_context.Customers.Count(), values.Count());
        }

        [Test]
        public async Task TestPost()
        {
            var post = await _controller.PostItem(new Customer { Active = true, Lastname = "Elisabette", Firstname = "II", Email = "rien" });
            var result = post.Result as ObjectResult;
            Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
        }

        /*[Test]
        public async Task TestGetID()
        {
            var post = await _controller.GetByID(1);
            var result = post.Result as ObjectResult;
            Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
        }*/

        /*[Test]
        public async Task TestDelete()
        {
            var post = await _controller.DeleteCustomer(1);
            var result = post.Result as ObjectResult;
            Assert.AreEqual((int)HttpStatusCode.Created, result.StatusCode);
        }*/


    }
}

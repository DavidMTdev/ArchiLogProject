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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

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
            var config = new HttpConfiguration();
            //configure web api
            config.MapHttpAttributeRoutes();

            using (var client = new HttpClient())
            {
                string url = "https://localhost:44316/api/v1/customers/";
                var response = await client.GetAsync(url);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                var result = await response.Content.ReadAsStringAsync();
            }
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
            var config = new HttpConfiguration();
            //configure web api
            config.MapHttpAttributeRoutes();

            using (var client = new HttpClient())
            {
                string url = "https://localhost:44316/api/v1/customers/5";
                var response = await client.GetAsync(url);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                var result = await response.Content.ReadAsStringAsync();
                var jsonData = json
            }
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

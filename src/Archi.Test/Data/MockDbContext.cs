using Archi.Api.Data;
using Archi.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Archi.Test.Data
{
    class MockDbContext : ArchiDBContext
    {
        public MockDbContext(DbContextOptions options):base(options)
        {

        }

        public static MockDbContext GetDbContext(bool withdata = true)
        {
            var options = new DbContextOptionsBuilder().UseInMemoryDatabase("DbTest").Options;
            var db = new MockDbContext(options);

            if (withdata)
            {
                db.Customers.Add(new Customer { Active = true, Lastname = "Bob", Firstname = "Patrick", Email = "rien" });
                db.SaveChanges();
            }

            return db;
        }
    }
}

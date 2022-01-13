using Archi.library.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archi.Api.Data
{
    public class ArchiDBContext : BaseDbContext
    {
        public ArchiDBContext(DbContextOptions options):base(options)
        {
        }

    }
}

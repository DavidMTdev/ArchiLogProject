using Archi.Api.Data;
using Archi.Api.Models;
using Archi.library.Controllers;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Archi.Api.Controllers
{
    public class CustomersController : BaseController<ArchiDBContext, Customer>
    {
        readonly IDiagnosticContext _diagnosticContext;
        public CustomersController(IDiagnosticContext diagnosticContext, ArchiDBContext context) : base(context)
        {
            _diagnosticContext = diagnosticContext ??
                throw new ArgumentNullException(nameof(diagnosticContext));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Archi.library.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Archi.Library.Models;
using Archi.Library.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Archi.library.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class BaseController<TContext, TModel> : ControllerBase where TContext : BaseDbContext where TModel : BaseModel
    {
        protected readonly TContext _context;

        public BaseController(TContext context)
        {
            _context = context;
        }


        // GET:/[controller]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAll([FromQuery] Params param)
        {
            try
            {
                var result2 = _context.Set<TModel>().Where(x => x.Active == true);

                dynamic defaultValues = QueryExtensions.DefineValues(this.Request.Scheme, this.Request.Host.Value, this.Request.Path.Value, this.Request.QueryString.Value);

                string URL = defaultValues.Url;
                string QueryString = defaultValues.QueryString;
                string order = defaultValues.Order;
                string Range = param.Range;

                var resultOrd = result2.QuerySort(param, order);
                var resultFilter = resultOrd.QueryFilter(param, this.Request.Query);
                var resultPagi = resultFilter.QueryPaging(param, URL, QueryString, Response);
                var result = resultPagi.QueryFields(param);

                return await result.ToListAsync();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // GET:/[controller]/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetBySearch([FromQuery] Params param)
        {
            try
            {
                var result2 = _context.Set<TModel>().Where(x => x.Active == true);

                dynamic defaultValues = QueryExtensions.DefineValues(this.Request.Scheme, this.Request.Host.Value, this.Request.Path.Value, this.Request.QueryString.Value);

                string URL = defaultValues.Url;
                string QueryString = defaultValues.QueryString;
                string order = defaultValues.Order;
                string Range = param.Range;

                var resultOrd = result2.QuerySort(param, order);
                var resultSearch = resultOrd.QuerySearch(param, this.Request.Query);
                var resultPagi = resultSearch.QueryPaging(param, URL, QueryString, Response);
                var result = resultPagi.QueryFields(param);

                return await result.ToListAsync();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // GET:/[controller]/id
        [HttpGet("{id}")]
        public async Task<ActionResult<TModel>> GetByID(int id)
        {

            var item = await _context.Set<TModel>().SingleOrDefaultAsync(x => x.Active == true && x.ID == id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<ActionResult<TModel>> PutItem(int id, TModel model)
        {
            if (id != model.ID)
            {
                return BadRequest();
            }

            if (!ItemExists(id))
            {
                return NotFound();
            }

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateConcurrencyException)
            {

                throw;

            }

            return NoContent();
        }

        private bool ItemExists(int id)
        {
            return _context.Set<TModel>().Any(e => e.ID == id && e.Active);
        }


        // POST: api/Customers
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<TModel>> PostItem(TModel model)
        {
            _context.Set<TModel>().Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetByID", new { id = model.ID }, model);
        }


        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TModel>> DeleteCustomer(int id)
        {
            var item = await _context.Set<TModel>().FindAsync(id);
            if (item == null || !item.Active)
            {
                return NotFound();
            }

            _context.Set<TModel>().Remove(item);
            await _context.SaveChangesAsync();

            return item;
        }

    }
}

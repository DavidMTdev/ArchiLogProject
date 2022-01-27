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

namespace Archi.library.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class BaseController<TContext, TModel> : ControllerBase where TContext : BaseDbContext where TModel : BaseModel
    {
        protected readonly TContext _context;

        public BaseController(TContext context)
        {
            _context = context;
        }


        // GET:/[controller]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAll([FromQuery] Params param, string urlDefault = "")
        {
            try
            {
                var result2 = _context.Set<TModel>().Where(x => x.Active == true);

                var URL = urlDefault != "" ? urlDefault : this.Request.Scheme + "://" + this.Request.Host.Value + this.Request.Path.Value;
                var QueryString = urlDefault != "" ? urlDefault.Split("?")[1] : this.Request.QueryString.Value;
                var test = this.Request.ToString();

                var order = "none";
                if (QueryString.ToLower().Contains("asc") && QueryString.ToLower().Contains("desc"))
                {
                    order = (QueryString.ToLower().IndexOf("asc", 0) < QueryString.ToLower().IndexOf("desc", 0)) ? "ascToDesc" : "descToAsc";
                }
                else if (QueryString.ToLower().Contains("asc"))
                {
                    order = "asc";
                }
                else
                {
                    order = "desc";
                }

                var resultOrd = result2.Sort(param, order);

                string Range = param.Range;
                if (Range != null)
                {
                    string[] RangeSplit = Range.Split('-');
                    if (int.Parse(RangeSplit[0]) > int.Parse(RangeSplit[1]) || int.Parse(RangeSplit[1]) - int.Parse(RangeSplit[0]) + 1 > 50)
                    {
                        return BadRequest();
                    }
                }

                var resultFilter = resultOrd.Filter(param, this.Request.Query);

                var resultPagi = resultFilter.Pagination(param, URL, QueryString, Response);

                var result = resultPagi.SelectFields(param);

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
            var result2 = _context.Set<TModel>().Where(x => x.Active == true);

            var order = "none";
            if (this.Request.QueryString.Value.ToLower().Contains("asc") && this.Request.QueryString.Value.ToLower().Contains("desc"))
            {
                order = (this.Request.QueryString.Value.ToLower().IndexOf("asc", 0) < this.Request.QueryString.Value.ToLower().IndexOf("desc", 0)) ? "ascToDesc" : "descToAsc";
            }
            else if (this.Request.QueryString.Value.ToLower().Contains("asc"))
            {
                order = "asc";
            }
            else
            {
                order = "desc";
            }

            var resultOrd = result2.Sort(param, order);

            var resultSearch = resultOrd.QuerySearch(param, this.Request.Query);

            var result = resultSearch.SelectFields(param);

            return await result.ToListAsync();
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

            /*var item = await _context.Set<TModel>().FindAsync(id);

            if (item == null || item.Active == false)
            {
                return NotFound();
            }
            */
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

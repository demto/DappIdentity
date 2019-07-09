using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly DataContext _context;

        public ValuesController(DataContext context)
        {
            _context = context;    
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> GetValues()
        { 
            var result = await _context.Values.ToListAsync();

            return Ok(result);
        }

        // GET api/values/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSingleValue(int id)
        {
            var value = await _context.Values.SingleOrDefaultAsync(v => v.Id == id);

            if(value == null){
                return NotFound("Could not find value.");
            }

            return Ok(value);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

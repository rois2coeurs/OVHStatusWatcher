using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OVHStatusWatcher.Models;

namespace OVHStatusWatcher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackerController(IConfiguration config, MyDbContext context) : ControllerBase
    {
        private readonly MyDbContext _context = context;

        // GET: api/e
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tracker>>> GetTrackers()
        {
            return await _context.Trackers.ToListAsync();
        }

        // GET: api/e/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tracker>> GetTracker(long id)
        {
            var tracker = await _context.Trackers.FindAsync(id);

            if (tracker == null)
            {
                return NotFound();
            }

            return tracker;
        }

        // PUT: api/e/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTracker(long id, Tracker tracker)
        {
            if (id != tracker.Id)
            {
                return BadRequest();
            }

            _context.Entry(tracker).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrackerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/e
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tracker>> PostTracker(Tracker tracker)
        {
            _context.Trackers.Add(tracker);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTracker", new { id = tracker.Id }, tracker);
        }

        // DELETE: api/e/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTracker(long id)
        {
            var tracker = await _context.Trackers.FindAsync(id);
            if (tracker == null)
            {
                return NotFound();
            }

            _context.Trackers.Remove(tracker);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TrackerExists(long id)
        {
            return _context.Trackers.Any(e => e.Id == id);
        }
    }
}

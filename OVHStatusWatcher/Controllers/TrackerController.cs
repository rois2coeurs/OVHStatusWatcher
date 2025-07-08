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
    public class TrackerController(MyDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tracker>>> GetTrackers()
        {
            return await context.Trackers.ToListAsync();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<Tracker>> GetTracker(long id)
        {
            var tracker = await context.Trackers.FindAsync(id);

            if (tracker == null)
            {
                return NotFound();
            }

            return tracker;
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> PutTracker(long id, Tracker tracker)
        {
            if (id != tracker.Id)
            {
                return BadRequest();
            }

            context.Entry(tracker).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrackerExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Tracker>> PostTracker(Tracker tracker)
        {
            context.Trackers.Add(tracker);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetTracker", new { id = tracker.Id }, tracker);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DeleteTracker(long id)
        {
            var tracker = await context.Trackers.FindAsync(id);
            if (tracker == null)
            {
                return NotFound();
            }

            context.Trackers.Remove(tracker);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool TrackerExists(long id)
        {
            return context.Trackers.Any(e => e.Id == id);
        }
    }
}

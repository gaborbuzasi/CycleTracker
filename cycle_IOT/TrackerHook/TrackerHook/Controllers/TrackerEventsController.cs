using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TrackerHook.Controllers
{
    [Produces("application/json")]
    [Route("api/TrackerEvents")]
    public class TrackerEventsController : Controller
    {
        private readonly TrackerContext _context;

        public TrackerEventsController(TrackerContext context)
        {
            _context = context;
        }

        // GET: api/TrackerEvents
        [HttpGet]
        public IEnumerable<TrackerEvent> GetTrackerEvents()
        {
            return _context.TrackerEvents;
        }

        // GET: api/TrackerEvents/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrackerEvent([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trackerEvent = await _context.TrackerEvents.SingleOrDefaultAsync(m => m.Id == id);

            if (trackerEvent == null)
            {
                return NotFound();
            }

            return Ok(trackerEvent);
        }

        // PUT: api/TrackerEvents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTrackerEvent([FromRoute] int id, [FromBody] TrackerEvent trackerEvent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != trackerEvent.Id)
            {
                return BadRequest();
            }

            _context.Entry(trackerEvent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrackerEventExists(id))
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

        // POST: api/TrackerEvents
        [HttpPost]
        public async Task<IActionResult> PostTrackerEvent([FromBody] TrackerEvent trackerEvent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.TrackerEvents.Add(trackerEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTrackerEvent", new { id = trackerEvent.Id }, trackerEvent);
        }

        // DELETE: api/TrackerEvents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrackerEvent([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trackerEvent = await _context.TrackerEvents.SingleOrDefaultAsync(m => m.Id == id);
            if (trackerEvent == null)
            {
                return NotFound();
            }

            _context.TrackerEvents.Remove(trackerEvent);
            await _context.SaveChangesAsync();

            return Ok(trackerEvent);
        }

        private bool TrackerEventExists(int id)
        {
            return _context.TrackerEvents.Any(e => e.Id == id);
        }
    }
}
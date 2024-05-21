using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoM.Context;
using PhotoM.Entities;

namespace PhotoM.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CollectionController : Controller
{
   
    private readonly AppDbContext _context;

    public CollectionController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet("getCollection")]
    public async Task<ActionResult<IEnumerable<collection>>> getCollection(string? name)
    {
        var query = _context.collections.AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(c => c.collection_name.Contains(name));
        }

        return await query.ToListAsync();
    }
    [HttpGet("getCollection/{id}")]
    public async Task<ActionResult<collection>> GetCollectionById(int id)
    {
        var collection = await _context.collections.FindAsync(id);

        if (collection == null)
        {
            return NotFound();
        }

        return collection;
    }
    
    [HttpPost("addCollection")]
    public async Task<ActionResult<collection>> AddCollection(collection collection)
    {
        _context.collections.Add(collection);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(getCollection), new { id = collection.collection_id }, collection);
    }

    [HttpPut("updateCollection/{id}")]
    public async Task<IActionResult> UpdateCollection(int id, collection collection)
    {
        if (id != collection.collection_id)
        {
            return BadRequest();
        }

        _context.Entry(collection).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CollectionExists(id))
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

    [HttpDelete("deleteCollection/{id}")]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            var collection = await _context.collections.FindAsync(id);
            if (collection == null)
            {
                return NotFound();
            }
    
            _context.collections.Remove(collection);
            await _context.SaveChangesAsync();
    
            return NoContent();
        }
        
    [HttpGet("getCollectionName/{id}")]
    public async Task<ActionResult<string>> GetCollectionNameById(int id)
    {
        var collection = await _context.collections.FindAsync(id);

        if (collection == null)
        {
            return NotFound();
        }

        return collection.collection_name;
    }

    private bool CollectionExists(int id)
    {
        return _context.collections.Any(e => e.collection_id == id);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoM.Context;
using PhotoM.Entities;

namespace PhotoM.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PhotoController : Controller
{
    private readonly AppDbContext _context;

    public PhotoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("getPhoto")]
    public async Task<ActionResult<IEnumerable<photo>>> GetPhoto(string? title)
    {
        var query = _context.photos.AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(p => p.title.Contains(title));
        }

        return await query.ToListAsync();
    }

    [HttpGet("getPhotosByDirectoryId/{id}")]
    public async Task<ActionResult<IEnumerable<photo>>> GetPhotosByDirectoryId(int id)
    {
        var photos = await _context.photos.Where(p => p.collection_id == id).ToListAsync();

        if (photos == null || !photos.Any())
        {
            return NotFound();
        }

        return photos;
    }

    [HttpGet("getPhoto/{id}")]
    public async Task<IActionResult> GetPhotoById(int id)
    {
        var photo = await _context.photos.FindAsync(id);

        if (photo == null || photo.photo1 == null)
        {
            return NotFound();
        }

        // Get the MIME type of the photo based on its format
        var format = photo.format.ToLower();
        string mimeType;
        switch (format)
        {
            case "jpg":
            case "jpeg":
                mimeType = "image/jpeg";
                break;
            case "png":
                mimeType = "image/png";
                break;
            case "gif":
                mimeType = "image/gif";
                break;
            case "bmp":
                mimeType = "image/bmp";
                break;
            case "tiff":
            case "tif":
                mimeType = "image/tiff";
                break;
            default:
                return BadRequest("Unsupported image format.");
        }

        return File(photo.photo1, mimeType);
    }
    

    [HttpPost("addPhoto")]
    public async Task<ActionResult<photo>> Upload([FromForm] IFormFile file, int collection_id)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is not selected");
        }

        var fileName = Path.GetFileName(file.FileName);

        // Check if the file is an image
        var contentType = file.ContentType;
        var allowedContentTypes = new List<string> { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        if (!allowedContentTypes.Contains(contentType))
        {
            return BadRequest("Invalid file type. Only JPEG, JPG, PNG and GIF types are allowed.");
        }

        byte[] fileData;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileData = ms.ToArray();
        }

        var collection = await _context.collections.FindAsync(collection_id);
        if (collection == null)
        {
            return NotFound("Collection not found");
        }

        var photo = new photo
        {
            title = fileName,
            photo1 = fileData,
            added_date = DateTime.Now,
            format = file.ContentType,
            collection = collection
        };

        _context.photos.Add(photo);
        await _context.SaveChangesAsync();

        // Create a URL to the photo
        var photoUrl = Url.Action("GetPhotoById", "Photo", new { id = photo.photo_id }, Request.Scheme);

        // Return the photo and its URL
        var response = new photo
        {
            photo1 = photo.photo1,
            title = photoUrl
        };

        return CreatedAtAction(nameof(GetPhotoById), new { id = photo.photo_id }, response);
    }


    [HttpPut("updatePhoto/{id}")]
    public async Task<IActionResult> UpdatePhoto(int id, photo photo)
    {
        if (id != photo.photo_id)
        {
            return BadRequest();
        }

        _context.Entry(photo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PhotoExists(id))
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

    [HttpDelete("deletePhoto/{id}")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _context.photos.FindAsync(id);
        if (photo == null)
        {
            return NotFound();
        }

        _context.photos.Remove(photo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<photo>>> SearchPhotos(string? title, DateTime? added_date, string? format, int? collection_id)
    {
        var query = _context.photos.AsQueryable();

        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(p => p.title.Contains(title));
        }

        if (added_date.HasValue)
        {
            var date = added_date.Value.Date;
            query = query.Where(p => p.added_date.HasValue && p.added_date.Value.Date == date);
        }

        if (!string.IsNullOrEmpty(format))
        {
            query = query.Where(p => p.format == format);
        }

        if (collection_id.HasValue)
        {
            query = query.Where(p => p.collection_id == collection_id);
        }

        var photos = await query.ToListAsync();

        return Ok(photos);
    }
    
    [HttpGet("getAllCollections")]
    public async Task<ActionResult<IEnumerable<string>>> GetAllCollections()
    {
        var collections = await _context.collections.Select(c => c.collection_id).Distinct().ToListAsync();
        return Ok(collections);
    }
    
    
    [HttpGet("getAllFormats")]
    public async Task<ActionResult<IEnumerable<string>>> GetAllFormats()
    {
        var formats = await _context.photos.Select(p => p.format).Distinct().ToListAsync();
        return Ok(formats);
    }

    [HttpGet("getAllDates")]
    public async Task<ActionResult<IEnumerable<DateTime>>> GetAllDates()
    {
        var dates = await _context.photos.Select(p => p.added_date.Value).Distinct().ToListAsync();
        return Ok(dates);
    }
    
    [HttpGet("formats")]
    public async Task<ActionResult<IEnumerable<string>>> GetFormats()
    {
        var formats = await _context.photos.Select(p => p.format).Distinct().ToListAsync();
        return Ok(formats);
    }

    [HttpGet("collectionNames")]
    public async Task<ActionResult<IEnumerable<photo>>> GetCollectionNames()
    {
        var photos = await _context.photos.Include(p => p.collection).ToListAsync();
        return Ok(photos);
    }


    private bool PhotoExists(int id)
    {
        return _context.photos.Any(e => e.photo_id == id);
    }
}
using LinksService.Data;
using LinksService.Models;
using LinksService.Models.DTOs;
using LinksService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinksService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LinksController : ControllerBase
{
    private readonly LinksContext _context;
    private readonly IImageUploadService _imageUploadService;

    public LinksController(LinksContext context, IImageUploadService imageUploadService)
    {
        _context = context;
        _imageUploadService = imageUploadService;
    }

    // GET: api/links
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LinkDto>>> GetLinks(
        [FromQuery] int? categoryId = null,
        [FromQuery] string? tag = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Links.Include(l => l.Category).AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(l => l.CategoryId == categoryId);
        }

        if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(l => l.Tags.Contains(tag));
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(l =>
                l.Title.ToLower().Contains(searchLower) ||
                (l.Description != null && l.Description.ToLower().Contains(searchLower)));
        }

        var links = await query
            .OrderBy(l => l.Order)
            .ThenByDescending(l => l.CreatedAt)
            .Select(l => new LinkDto
            {
                Id = l.Id,
                Title = l.Title,
                Url = l.Url,
                Description = l.Description,
                CategoryId = l.CategoryId,
                CategoryName = l.Category != null ? l.Category.Name : null,
                Tags = l.Tags,
                ImageUrl = l.ImageUrl,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                Order = l.Order
            })
            .ToListAsync();

        return Ok(links);
    }

    // GET: api/links/5
    [HttpGet("{id}")]
    public async Task<ActionResult<LinkDto>> GetLink(int id)
    {
        var link = await _context.Links
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (link == null)
        {
            return NotFound();
        }

        return Ok(new LinkDto
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            Description = link.Description,
            CategoryId = link.CategoryId,
            CategoryName = link.Category?.Name,
            Tags = link.Tags,
            ImageUrl = link.ImageUrl,
            CreatedAt = link.CreatedAt,
            UpdatedAt = link.UpdatedAt,
            Order = link.Order
        });
    }

    // POST: api/links
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<LinkDto>> CreateLink([FromBody] CreateLinkDto dto)
    {
        var link = new Link
        {
            Title = dto.Title,
            Url = dto.Url,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Tags = dto.Tags ?? new List<string>(),
            ImageUrl = dto.ImageUrl,
            Order = dto.Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Links.Add(link);
        await _context.SaveChangesAsync();

        // Load category for response
        await _context.Entry(link).Reference(l => l.Category).LoadAsync();

        return CreatedAtAction(nameof(GetLink), new { id = link.Id }, new LinkDto
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            Description = link.Description,
            CategoryId = link.CategoryId,
            CategoryName = link.Category?.Name,
            Tags = link.Tags,
            ImageUrl = link.ImageUrl,
            CreatedAt = link.CreatedAt,
            UpdatedAt = link.UpdatedAt,
            Order = link.Order
        });
    }

    // POST: api/links/upload
    [HttpPost("upload")]
    [Authorize]
    public async Task<ActionResult<LinkDto>> CreateLinkWithUpload([FromForm] CreateLinkDto dto, IFormFile? image)
    {
        string? imageUrl = dto.ImageUrl;

        if (image != null)
        {
            try
            {
                imageUrl = await _imageUploadService.UploadImageAsync(image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        var link = new Link
        {
            Title = dto.Title,
            Url = dto.Url,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Tags = dto.Tags ?? new List<string>(),
            ImageUrl = imageUrl,
            Order = dto.Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Links.Add(link);
        await _context.SaveChangesAsync();

        // Load category for response
        await _context.Entry(link).Reference(l => l.Category).LoadAsync();

        return CreatedAtAction(nameof(GetLink), new { id = link.Id }, new LinkDto
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            Description = link.Description,
            CategoryId = link.CategoryId,
            CategoryName = link.Category?.Name,
            Tags = link.Tags,
            ImageUrl = link.ImageUrl,
            CreatedAt = link.CreatedAt,
            UpdatedAt = link.UpdatedAt,
            Order = link.Order
        });
    }

    // PUT: api/links/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<LinkDto>> UpdateLink(int id, [FromBody] UpdateLinkDto dto)
    {
        var link = await _context.Links.FindAsync(id);

        if (link == null)
        {
            return NotFound();
        }

        if (dto.Title != null) link.Title = dto.Title;
        if (dto.Url != null) link.Url = dto.Url;
        if (dto.Description != null) link.Description = dto.Description;
        if (dto.CategoryId.HasValue) link.CategoryId = dto.CategoryId.Value == 0 ? null : dto.CategoryId;
        if (dto.Tags != null) link.Tags = dto.Tags;
        if (dto.ImageUrl != null) link.ImageUrl = dto.ImageUrl;
        if (dto.Order.HasValue) link.Order = dto.Order.Value;

        link.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Load category for response
        await _context.Entry(link).Reference(l => l.Category).LoadAsync();

        return Ok(new LinkDto
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            Description = link.Description,
            CategoryId = link.CategoryId,
            CategoryName = link.Category?.Name,
            Tags = link.Tags,
            ImageUrl = link.ImageUrl,
            CreatedAt = link.CreatedAt,
            UpdatedAt = link.UpdatedAt,
            Order = link.Order
        });
    }

    // PUT: api/links/5/upload
    [HttpPut("{id}/upload")]
    [Authorize]
    public async Task<ActionResult<LinkDto>> UpdateLinkWithUpload(int id, [FromForm] UpdateLinkDto dto, IFormFile? image)
    {
        var link = await _context.Links.FindAsync(id);

        if (link == null)
        {
            return NotFound();
        }

        if (image != null)
        {
            try
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(link.ImageUrl))
                {
                    await _imageUploadService.DeleteImageAsync(link.ImageUrl);
                }

                link.ImageUrl = await _imageUploadService.UploadImageAsync(image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        if (dto.Title != null) link.Title = dto.Title;
        if (dto.Url != null) link.Url = dto.Url;
        if (dto.Description != null) link.Description = dto.Description;
        if (dto.CategoryId.HasValue) link.CategoryId = dto.CategoryId.Value == 0 ? null : dto.CategoryId;
        if (dto.Tags != null) link.Tags = dto.Tags;
        if (dto.ImageUrl != null && image == null) link.ImageUrl = dto.ImageUrl;
        if (dto.Order.HasValue) link.Order = dto.Order.Value;

        link.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Load category for response
        await _context.Entry(link).Reference(l => l.Category).LoadAsync();

        return Ok(new LinkDto
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            Description = link.Description,
            CategoryId = link.CategoryId,
            CategoryName = link.Category?.Name,
            Tags = link.Tags,
            ImageUrl = link.ImageUrl,
            CreatedAt = link.CreatedAt,
            UpdatedAt = link.UpdatedAt,
            Order = link.Order
        });
    }

    // DELETE: api/links/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteLink(int id)
    {
        var link = await _context.Links.FindAsync(id);

        if (link == null)
        {
            return NotFound();
        }

        // Delete image from S3 if exists
        if (!string.IsNullOrEmpty(link.ImageUrl))
        {
            await _imageUploadService.DeleteImageAsync(link.ImageUrl);
        }

        _context.Links.Remove(link);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

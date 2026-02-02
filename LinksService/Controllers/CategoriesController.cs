using LinksService.Data;
using LinksService.Models;
using LinksService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinksService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly LinksContext _context;

    public CategoriesController(LinksContext context)
    {
        _context = context;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Order)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Order = c.Order,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/categories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Order = category.Order,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        });
    }

    // POST: api/categories
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        // Check if category with same name already exists
        if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
        {
            return Conflict(new { error = "A category with this name already exists" });
        }

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Order = category.Order,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        });
    }

    // PUT: api/categories/5
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        // Check if new name conflicts with existing category
        if (dto.Name != null && dto.Name != category.Name)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == dto.Name))
            {
                return Conflict(new { error = "A category with this name already exists" });
            }
            category.Name = dto.Name;
        }

        if (dto.Description != null) category.Description = dto.Description;
        if (dto.Order.HasValue) category.Order = dto.Order.Value;

        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Order = category.Order,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        });
    }

    // DELETE: api/categories/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        // Set CategoryId to null on all links that reference this category
        var linksWithCategory = await _context.Links
            .Where(l => l.CategoryId == id)
            .ToListAsync();

        foreach (var link in linksWithCategory)
        {
            link.CategoryId = null;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/categories/tags
    [HttpGet("tags")]
    public async Task<ActionResult<IEnumerable<string>>> GetTags()
    {
        var links = await _context.Links
            .Select(l => l.Tags)
            .ToListAsync();

        var tags = links
            .SelectMany(t => t)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return Ok(tags);
    }
}

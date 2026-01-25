using LinksService.Data;
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
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _context.Links
            .Where(l => l.Category != null && l.Category != "")
            .Select(l => l.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
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

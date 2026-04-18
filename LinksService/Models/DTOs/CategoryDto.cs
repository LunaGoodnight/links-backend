using System.ComponentModel.DataAnnotations;

namespace LinksService.Models.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Order { get; set; }
}

public class UpdateCategoryDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? Order { get; set; }
}

public class BatchUpdateCategoryOrderDto
{
    [Required]
    public List<CategoryOrderItem> Items { get; set; } = new();
}

public class CategoryOrderItem
{
    public int Id { get; set; }
    public int Order { get; set; }
}

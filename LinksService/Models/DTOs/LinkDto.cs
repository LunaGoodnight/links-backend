using System.ComponentModel.DataAnnotations;

namespace LinksService.Models.DTOs;

public class LinkDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Order { get; set; }
}

public class CreateLinkDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    [Url]
    public string Url { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public List<string>? Tags { get; set; }

    [MaxLength(2000)]
    public string? ImageUrl { get; set; }

    public int Order { get; set; }
}

public class UpdateLinkDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    [Url]
    public string? Url { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public List<string>? Tags { get; set; }

    [MaxLength(2000)]
    public string? ImageUrl { get; set; }

    public int? Order { get; set; }
}

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

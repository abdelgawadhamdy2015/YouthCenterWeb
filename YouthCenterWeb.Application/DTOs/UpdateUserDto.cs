public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public int? FavActivityId { get; set; }

    public string? Bio { get; set; }
}
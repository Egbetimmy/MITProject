namespace AIScaling.Shared.DTOs;

/// <summary>Data transfer object for product creation.</summary>
public sealed class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

/// <summary>Data transfer object representing a product.</summary>
public sealed class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Data transfer object for updating a product.</summary>
public sealed class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

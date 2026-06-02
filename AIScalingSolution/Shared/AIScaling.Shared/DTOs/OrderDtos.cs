namespace AIScaling.Shared.DTOs;

/// <summary>Data transfer object for order creation.</summary>
public sealed class CreateOrderDto
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>Data transfer object representing an order.</summary>
public sealed class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Data transfer object for updating an order.</summary>
public sealed class UpdateOrderDto
{
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
}

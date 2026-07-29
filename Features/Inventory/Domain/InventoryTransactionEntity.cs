using Microsoft.EntityFrameworkCore;
using Nexus.Features.Inventory.Dto;

namespace Nexus.Features.Inventory.Domain;

[Index(nameof(UserId), nameof(CreatedAt))]
public class InventoryTransactionEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public InventoryTransactionReason Reason { get; set; }
    public CurrencyType? CurrencyType { get; set; }
    public int? CurrencyAmount { get; set; }

    public string? ItemId { get; set; }
    public int? ItemAmount { get; set; }

    public string? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; }
}
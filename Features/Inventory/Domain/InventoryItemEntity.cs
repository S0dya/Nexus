using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Nexus.Features.Inventory.Domain;

public class InventoryItemEntity
{
    public Guid UserId { get; set; }
    public string ItemId { get; set; }
    public int Amount { get; set; }
}

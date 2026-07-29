using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Nexus.Features.Inventory.Domain;

public class InventoryEntity
{
    public Guid UserId { get; set; }
    public int Coins { get; set; }
    public int Gems { get; set; }
}

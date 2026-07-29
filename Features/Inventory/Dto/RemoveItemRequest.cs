using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Inventory.Dto;

public class RemoveItemRequest
{
    [Required]
    public string ItemId { get; set; }

    [Required]
    public int Amount { get; set; }
}

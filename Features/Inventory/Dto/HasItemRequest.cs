using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Inventory.Dto;

public class HasItemRequest
{
    [Required]
    public string ItemId { get; set; }
}

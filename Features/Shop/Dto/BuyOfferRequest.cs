using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.Shop.Dto;

public class BuyOfferRequest
{
    [Required]
    public string OfferId { get; set; }
}

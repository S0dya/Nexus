using Microsoft.EntityFrameworkCore;

namespace Nexus.Features.Analytics.Domain;

[Index(nameof(UserId), IsUnique = true)]
public class PlayerAnalyticsEntity
{
    public Guid UserId {get; set; }
    public int Purchases {get; set; }
    public int CoinsSpent {get; set; }
    public int ItemsBought {get; set; }
}
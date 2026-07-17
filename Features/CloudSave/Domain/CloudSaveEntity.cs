using System.ComponentModel.DataAnnotations;

namespace Nexus.Features.CloudSave.Domain;

public class CloudSaveEntity
{
    public Guid UserId { get; set; }
    public string Data { get; set; }
    public int Version { get; set; }
    public DateTime LastModified { get; set; }
}

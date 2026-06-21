namespace Nexus.Options;

public class ProjectSettingsOptions
{
    public int DefaultDuration { get; set; }
    public int DefaultAmount { get; set; }
    
    public int MinDuration { get; set; }
    public int MinAmount { get; set; }
    public int MaxDuration { get; set; }
    public int MaxAmount { get; set; }
    public int SessionStopTimerDifferenceOffset { get; set; }

    public int MaxRunsHistoryLimit { get; set; }
}
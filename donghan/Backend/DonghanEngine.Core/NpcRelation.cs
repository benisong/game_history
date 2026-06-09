namespace DonghanEngine.Core;

public enum NpcRelationType
{
    Kinship,
    Patronage,
    FactionAlly,
    TeacherStudent,
    SwornBond,
    Rivalry,
    Hostility,
    Command,
    RegionalTie
}

public class NpcRelation
{
    public string FromNpcId { get; set; } = string.Empty;
    public string ToNpcId { get; set; } = string.Empty;
    public NpcRelationType Type { get; set; }
    public int Strength { get; set; } = 50;
    public bool IsMutual { get; set; } = true;
    public string Label { get; set; } = string.Empty;
    public string HistoricalBasis { get; set; } = string.Empty;
}

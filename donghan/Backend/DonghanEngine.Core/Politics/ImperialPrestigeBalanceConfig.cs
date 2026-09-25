namespace DonghanEngine.Core.Politics;

public sealed class ImperialPrestigeBalanceConfig
{
    public int PuppetMaxThreshold { get; set; } = 25;
    public double PuppetEfficiency { get; set; } = 0.35;
    public double PuppetRebellionModifier { get; set; } = 0.20;

    public int DisrespectedMaxThreshold { get; set; } = 45;
    public double DisrespectedEfficiency { get; set; } = 0.65;
    public double DisrespectedRebellionModifier { get; set; } = 0.05;

    public int GoldenBalanceMaxThreshold { get; set; } = 60;
    public double GoldenBalanceEfficiency { get; set; } = 1.0;
    public double GoldenBalanceRebellionModifier { get; set; } = -0.15;

    public int OppressiveMaxThreshold { get; set; } = 80;
    public double OppressiveEfficiency { get; set; } = 1.15;
    public double OppressiveTransferRate { get; set; } = 0.35;
    public double OppressiveRebellionModifier { get; set; } = 0.25;

    public double TyrannicalEfficiency { get; set; } = 1.25;
    public double TyrannicalTransferRate { get; set; } = 0.80;
    public double TyrannicalRebellionModifier { get; set; } = 0.60;

    public int OppressiveCorruptionThreshold { get; set; } = 40;
    public int OppressiveAmbitionThreshold { get; set; } = 70;
}

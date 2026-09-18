namespace DonghanEngine.Core;

public class CourtSpeech
{
    public string MinisterId { get; set; } = string.Empty;
    public string MinisterName { get; set; } = string.Empty;
    public string SpeechText { get; set; } = string.Empty;
    public string Stance { get; set; } = "OPPOSE"; // AGREED / OPPOSE / RETALIATE (迎合 / 反对 / 党羽弹劾反噬)
    public int ExpectedFavorabilityChange { get; set; }
    public int ExpectedPowerChange { get; set; }
}

public class RitualStageInfo
{
    public int StageIndex { get; set; } // 1, 2, 3
    public string Title { get; set; } = string.Empty;
    public string Narrative { get; set; } = string.Empty;
}

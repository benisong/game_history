using System.Collections.Generic;

namespace DonghanEngine.Core;

public class AIOrchestrationResult
{
    private List<CourtSpeech> _speeches = new();

    public string PrimaryIntent { get; set; } = "UNKNOWN"; // POLITICS / POLICY / PERSONAL / SACRIFICE
    public IReadOnlyList<CourtSpeech> Speeches
    {
        get => _speeches;
        set => _speeches = value != null ? new List<CourtSpeech>(value) : new List<CourtSpeech>();
    }
    public string NarrativeResponse { get; set; } = string.Empty;

    public void AddSpeech(CourtSpeech speech)
    {
        if (speech != null) _speeches.Add(speech);
    }
}

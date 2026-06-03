using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonghanEngine.Core;

public interface INarrator
{
    Task<string> RenderStoryAsync(
        string playerInput, 
        OracleEvent? triggeredEvent, 
        List<MinisterDialogue> ministerDialogues, 
        GameState state);
}
// 最终结算结果
public class TurnResult
{
    public string StoryText { get; set; } = string.Empty;
    public OracleEvent? TriggeredEvent { get; set; }
    public List<MinisterDialogue> Dialogues { get; set; } = new();
}

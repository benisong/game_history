using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonghanEngine.Core;

// 大臣反应数据结构
public class MinisterDialogue
{
    public string MinisterId { get; set; } = string.Empty;
    public string MinisterName { get; set; } = string.Empty;
    public string DialogueText { get; set; } = string.Empty;
    public int FavorabilityChange { get; set; } = 0;
    public int PowerChange { get; set; } = 0;
}

public interface IMinisterAgent
{
    Task<List<MinisterDialogue>> TalkToMinistersAsync(List<string> activeMinisters, string playerInput, GameState state);
}

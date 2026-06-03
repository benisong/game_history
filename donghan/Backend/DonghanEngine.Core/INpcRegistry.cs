namespace DonghanEngine.Core;

public interface INpcRegistry
{
    // 主动登庸注册一个 NPC 进游戏沙盒大局 (由可视化界面或 AI 导入)
    void RegisterNpc(NpcState npc, GameState state);

    // 罢免、下野或病逝死亡移出朝堂
    void DeregisterNpc(string npcId, string reason, GameState state);
}

public class NpcRegistry : INpcRegistry
{
    public void RegisterNpc(NpcState npc, GameState state)
    {
        if (string.IsNullOrWhiteSpace(npc.Id)) return;
        npc.IsActive = true;
        state.Npcs[npc.Id] = npc;
        state.AddToChronicle($"【登庸】朝廷引纳新臣【{npc.Name}】，授【{npc.Title}】。");
    }

    public void DeregisterNpc(string npcId, string reason, GameState state)
    {
        if (state.Npcs.TryGetValue(npcId, out var npc))
        {
            npc.IsActive = false;
            npc.DeathReason = reason;
            state.AddToChronicle($"【致仕/退场】大臣【{npc.Name}】由于“{reason}”，自此告退朝堂。");
            state.Npcs.Remove(npcId);
        }
    }
}

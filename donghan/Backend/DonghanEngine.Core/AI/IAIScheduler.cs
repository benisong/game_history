using System.Threading.Tasks;

namespace DonghanEngine.Core;

public interface IAIScheduler
{
    // 提供对 NPC 生命周期的引用，方便调度师随时自主发送指令调度演进
    INpcLifecycleManager NpcManager { get; }

    // 异步多通道核心编排：分析玩家在朝会上的输入/诏书，并调度多名在场大臣发表群辩演说
    Task<AIOrchestrationResult> OrchestrateGrandCourtAsync(string playerInput, string activeOfficerId, GameState state);

    // 旬更演进调度：每旬流逝时，AI 调度员自主为官员生成阴谋、天下天灾警报任务
    Task OrchestrateXunUpdateAsync(GameState state);
}

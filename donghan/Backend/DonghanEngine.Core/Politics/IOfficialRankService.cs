using System.Collections.Generic;

namespace DonghanEngine.Core.Politics;

public interface IOfficialRankService
{
    IReadOnlyList<OfficialPosition> GetAllPositions();
    OfficialPosition? GetPositionByTitle(string title);
    
    // 正规升迁 (一级一级 / 特旨超擢限3级以内)
    PromotionResult PromoteOfficial(GameState state, string npcId, string targetTitle);
    
    // 西园卖官 (无视等级，直通一品三公，收入进天子私库)
    OfficeSaleResult SellOfficeToNpc(GameState state, string buyerNpcId, string targetTitle);
}

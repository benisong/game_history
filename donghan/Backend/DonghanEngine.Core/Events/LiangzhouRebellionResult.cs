namespace DonghanEngine.Core.Events;

public enum LiangzhouRebellionOutcome
{
    HistoricalUprising, // 史实：北宫伯玉、韩遂、边章攻陷金城，凉州全境叛乱，董卓/马腾崛起
    FrontierContained,   // 边将死守/朝廷重赏：凉州边防稳固，叛乱被阻于金城关外，未危及三辅
    GarrisonSurrendered  // 凉州守军哗变：朝廷积欠军饷，凉州守军与羌胡合流，兵临三辅
}

public sealed record LiangzhouRebellionResult(
    LiangzhouRebellionOutcome Outcome,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int PopularSupportDelta,
    bool LiangzhouRebelling,
    bool DeployDongZhuo,
    bool DeployMaTeng);

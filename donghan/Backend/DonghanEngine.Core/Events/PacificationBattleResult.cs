using System.Collections.Generic;

namespace DonghanEngine.Core.Events;

public enum PacificationOutcome
{
    LuZhiTriumph,       // 卢植未受陷害/天子信任，率北军于广宗一战破张角，名动天下
    LuZhiFramed,        // 史实：左丰索贿未得构陷卢植，卢植槛车征还下狱，皇甫嵩接任大破曲阳
    HuangfuSongTriumph, // 皇甫嵩/朱儁直接挂帅大捷，平定广宗曲阳，斩张梁张宝
    RebelsPersist       // 汉军统帅不力或被中官掣肘，冀州贼势未灭
}

public sealed record PacificationBattleResult(
    PacificationOutcome Outcome,
    string GeneralId,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int PopularSupportDelta,
    bool JizhouPacified,
    bool LuZhiImprisoned);

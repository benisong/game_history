using System.Collections.Generic;

namespace DonghanEngine.Core.Events;

public enum RegionalGovernorDecision
{
    AdoptStatePastorSystem,  // 史实路线：准奏废史立牧，刘焉督益州、刘虞督幽州、刘表督荆州。地方治安迅速平定，但州牧割据雏形铸成，中央皇权下移
    RejectAndKeepCentralized // 驳回集权路线：天子驳回宗室分镇之议，坚持中央指派监察刺史，中央皇权集中，但地方偏远州郡平叛需朝廷大军远征
}

public sealed record RegionalGovernorResult(
    RegionalGovernorDecision Decision,
    string NarrativeTitle,
    string ChronicleText,
    int ImperialPowerDelta,
    int PopularSupportDelta,
    IReadOnlyList<string> AppointedGovernorIds,
    IReadOnlyDictionary<string, string> ProvinceGovernorMappings,
    int RemoteProvinceSupportBonus);

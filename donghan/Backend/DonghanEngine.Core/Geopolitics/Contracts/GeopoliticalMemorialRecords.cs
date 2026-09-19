using System.Collections.Generic;

namespace DonghanEngine.Core.Geopolitics.Contracts;

public enum GeopoliticalMemorialType
{
    PetitionRank,       // 战后大捷上表讨封（加封刺史/州牧/将军号）
    AppealForHelp,      // 孤城被围泣血求援（求天子救兵/调停敕令）
    TributeOffered,     // 忠藩恪守臣节进奉岁贡（献金充实国库）
    SecretIntelligence  // 地方密探呈报私下兼并军事调动
}

public sealed record GeopoliticalMemorial(
    string MemorialId,
    GeopoliticalMemorialType MemorialType,
    string SenderFactionId,
    string TargetProvinceId,
    string Title,
    string ContentText,
    int SuggestedTributeGold,
    IReadOnlyList<MemorialOption> Options);

public sealed record MemorialOption(
    string OptionId,
    string Label,
    string Description,
    GeopoliticalImperialEdict ResultingEdict);

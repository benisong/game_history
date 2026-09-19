using System.Collections.Generic;

namespace DonghanEngine.Core.Events;

public enum YellowTurbanIntensity
{
    Catastrophic,  // 全境溃决：6+ 州郡同时起事，直逼洛阳
    Historical,    // 史实中原三州起事：冀州、豫州、兖州
    Contained,     // 局部受控：仅首恶冀州起事，豫/兖稳守
    Localized      // 边缘流民化：中原稳固，仅边缘州郡起零星流民
}

public sealed record YellowTurbanScopeResult(
    YellowTurbanIntensity Intensity,
    IReadOnlyList<string> AffectedProvinceIds,
    string NarrativeTitle,
    string ChronicleText,
    int PopularSupportDrop);

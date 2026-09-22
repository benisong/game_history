using System.Collections.Generic;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Economy;

public interface IAgriculturalCarryingEngine
{
    ProvinceCarryingReport EvaluateProvince(Province province, int weatherSeverity = 0, double aristocracyLandRatio = 0.40);
}

public interface IBanditWarlordSymbiosisEngine
{
    BanditSpilloverResult EvaluateBanditSpillover(GameState state, string provinceId, int displacedRefugees);
}

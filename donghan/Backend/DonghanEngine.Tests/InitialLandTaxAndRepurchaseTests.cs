using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Economy;

namespace DonghanEngine.Tests;

public class InitialLandTaxAndRepurchaseTests
{
    private readonly LandOwnershipService _landService = new();

    [Fact]
    public void Test_InitialProvinces_SiliHasMajorityStateLand_OtherProvincesHaveMajorityGentryLand()
    {
        var provinces = ProvinceCatalog.CreateInitialProvinces();

        // 1. 司隶（洛阳京畿）：初始土地 120,000，国家控制 80,000 (占比 67%)
        var sili = provinces.FirstOrDefault(p => p.Id == ProvinceCatalog.Sili);
        Assert.NotNull(sili);
        Assert.Equal(80000, sili.StateControlledLand);
        Assert.Equal(40000, sili.GentryControlledLand);
        Assert.True(sili.StateControlledLand > sili.GentryControlledLand, "司隶京畿应以国家官田为主");

        // 2. 冀州/豫州/荆州：世家大族控制绝大部分土地 (国家仅少量)
        var jizhou = provinces.FirstOrDefault(p => p.Id == ProvinceCatalog.Jizhou);
        Assert.NotNull(jizhou);
        Assert.Equal(25000, jizhou.StateControlledLand);
        Assert.Equal(125000, jizhou.GentryControlledLand);
        Assert.True(jizhou.GentryControlledLand >= jizhou.StateControlledLand * 4, "关东大州应由世家坞堡控制绝大部分田产");
    }

    [Fact]
    public void Test_CollectQuarterlyLandTax_StateLandPaysFull_GentryLandPays40PercentLess()
    {
        var state = new GameState { Treasury = 0 };

        // 设置豫州土地：国家 50,000 亩，世家 50,000 亩，无焦土
        var yuzhou = state.Provinces["yuzhou"];
        yuzhou.StateControlledLand = 50000;
        yuzhou.GentryControlledLand = 50000;
        yuzhou.ScorchedLand = 0;

        // 国家土地 50,000 亩：(50000/1000)*10 = 500 万钱
        // 世家土地 50,000 亩：(50000/1000)*10 * 0.60 (少收40%) = 300 万钱
        // 单豫州贡献税收 = 800 万钱
        int totalTax = _landService.CollectQuarterlyLandTax(state, applyToTreasury: true);

        Assert.True(totalTax > 800, "十三州总税收应大于单豫州税收");
        Assert.Equal(totalTax, state.Treasury);
        Assert.Contains(state.Chronicle, c => c.Contains("季税入库") && c.Contains("少收四成"));
    }
}

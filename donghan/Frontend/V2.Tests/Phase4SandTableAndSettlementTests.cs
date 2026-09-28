using Xunit;
using DonghanEngine.Core;
using DonghanEngine.Core.Settlement;
using DonghanFrontend.V2.Contracts;
using DonghanFrontend.V2.Adapters;

namespace DonghanFrontend.V2.Tests;

public class Phase4SandTableAndSettlementTests
{
    [Fact]
    public void Test_SettlementSlideService_GeneratesHistoricalTurnPackage()
    {
        var state = new GameState
        {
            Year = 184,
            Month = 5,
            Xun = 2,
            Treasury = 9500,
            PrivateTreasury = 2200,
            ImperialPower = 62,
            PopularSupport = 68
        };

        var slideService = new TurnSettlementSlideService();
        var package = slideService.CompileTurnSettlementPackage(state);

        Assert.NotNull(package);
        Assert.Equal(184, package.Year);
        Assert.Equal(5, package.Month);
        Assert.Equal(2, package.Xun);
        Assert.Equal("184年5月中旬", package.DateText);
        Assert.True(package.Slides.Count >= 2);

        // 验证宏观天时与禁军军势两张基石幻灯片
        Assert.Contains(package.Slides, s => s.Kind == SettlementSlideKind.General);
        Assert.Contains(package.Slides, s => s.Kind == SettlementSlideKind.MilitaryPayroll);
    }
}

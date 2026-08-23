using System.Linq;
using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;
using Xunit;

namespace DonghanFrontend.V2.Tests;

public sealed class V2GovernorSelectorTests
{
    [Fact]
    public void Intel_service_assigns_selected_non_default_governor()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        var result = runtime.Intel.ExecuteProvinceAction(new ProvinceActionCommand(
            "jizhou",
            ProvinceActionKind.AssignGovernor,
            "he_jin"));

        Assert.True(result.Success);
        Assert.Equal("he_jin", runtime.State.GetProvince("jizhou")!.GovernorId);
    }
}

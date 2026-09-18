using System.Linq;
using Xunit;
using DonghanEngine.Core;
using DonghanFrontend.V2.Adapters;
using DonghanFrontend.V2.Contracts;

namespace DonghanFrontend.V2.Tests;

public class V2ThirteenProvincesReaderTests
{
    [Fact]
    public void Test_GetAllProvinces_ReturnsAllThirteenProvinces()
    {
        var runtime = V2RuntimeFactory.CreateDefault();
        var provinces = runtime.State.GetAllProvinces();

        Assert.Equal(13, provinces.Count);
        var ids = provinces.Select(p => p.Id).ToHashSet();
        
        Assert.Contains("sili", ids);
        Assert.Contains("jizhou", ids);
        Assert.Contains("bingzhou", ids);
        Assert.Contains("yanzhou", ids);
        Assert.Contains("yuzhou", ids);
        Assert.Contains("jingzhou", ids);
        Assert.Contains("qingzhou", ids);
        Assert.Contains("xuzhou", ids);
        Assert.Contains("yangzhou", ids);
        Assert.Contains("youzhou", ids);
        Assert.Contains("liangzhou", ids);
        Assert.Contains("yizhou", ids);
        Assert.Contains("jiaozhou", ids);
    }

    [Fact]
    public void Test_InspectProvince_AcrossAllThirteenProvinces_Success()
    {
        var runtime = V2RuntimeFactory.CreateDefault();

        foreach (var p in runtime.State.GetAllProvinces())
        {
            var intel = runtime.Intel.InspectProvince(new InspectProvinceCommand(p.Id));
            Assert.True(intel.Success);
            Assert.NotNull(intel.Province);
            Assert.Equal(p.Id, intel.Province!.Id);
            Assert.Equal(p.Name, intel.Province!.Name);
        }
    }
}

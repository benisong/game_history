namespace DonghanEngine.Core.Balance;

/// <summary>
/// 统一配置初始化接口：支持外部超级控制工具、配表解析器在运行时或初始化时动态注入与热调平衡常数
/// </summary>
/// <typeparam name="TConfig">强类型平衡配置参数</typeparam>
public interface IInitializableBalance<in TConfig>
{
    void InitializeConfig(TConfig config);
}

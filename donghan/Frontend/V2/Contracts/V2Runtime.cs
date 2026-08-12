namespace DonghanFrontend.V2.Contracts;

public sealed class V2Runtime
{
    public V2Runtime(
        IGameStateReader state,
        ITravelService travel,
        IWestGardenService westGarden,
        IIntelService intel,
        ICourtService court,
        ITurnService turns)
    {
        State = state;
        Travel = travel;
        WestGarden = westGarden;
        Intel = intel;
        Court = court;
        Turns = turns;
    }

    public IGameStateReader State { get; }
    public ITravelService Travel { get; }
    public IWestGardenService WestGarden { get; }
    public IIntelService Intel { get; }
    public ICourtService Court { get; }
    public ITurnService Turns { get; }
}

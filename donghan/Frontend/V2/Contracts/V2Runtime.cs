namespace DonghanFrontend.V2.Contracts;

public sealed class V2Runtime
{
    public V2Runtime(
        IGameStateReader state,
        ITravelService travel,
        IWestGardenService westGarden,
        IIntelService intel,
        ICourtService court,
        ITurnService turns,
        IEdictService edicts,
        ISpecialActionService specialActions)
    {
        State = state;
        Travel = travel;
        WestGarden = westGarden;
        Intel = intel;
        Court = court;
        Turns = turns;
        Edicts = edicts;
        SpecialActions = specialActions;
    }

    public IGameStateReader State { get; }
    public ITravelService Travel { get; }
    public IWestGardenService WestGarden { get; }
    public IIntelService Intel { get; }
    public ICourtService Court { get; }
    public ITurnService Turns { get; }
    public IEdictService Edicts { get; }
    public ISpecialActionService SpecialActions { get; }
}

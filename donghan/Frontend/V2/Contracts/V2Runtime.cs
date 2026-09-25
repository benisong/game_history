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
        ISpecialActionService specialActions,
        INominationUiService nominations,
        IOfficialRankUiService ranks,
        IAgriculturalPolicyUiService agriculture,
        IGovernorAppraisalUiService appraisal,
        IImperialHealthUiService health,
        ICourtDelegationUiService delegation,
        IPersistenceUiService persistence)
    {
        State = state;
        Travel = travel;
        WestGarden = westGarden;
        Intel = intel;
        Court = court;
        Turns = turns;
        Edicts = edicts;
        SpecialActions = specialActions;
        Nominations = nominations;
        Ranks = ranks;
        Agriculture = agriculture;
        Appraisal = appraisal;
        Health = health;
        Delegation = delegation;
        Persistence = persistence;
    }

    public IGameStateReader State { get; }
    public ITravelService Travel { get; }
    public IWestGardenService WestGarden { get; }
    public IIntelService Intel { get; }
    public ICourtService Court { get; }
    public ITurnService Turns { get; }
    public IEdictService Edicts { get; }
    public ISpecialActionService SpecialActions { get; }
    public INominationUiService Nominations { get; }
    public IOfficialRankUiService Ranks { get; }
    public IAgriculturalPolicyUiService Agriculture { get; }
    public IGovernorAppraisalUiService Appraisal { get; }
    public IImperialHealthUiService Health { get; }
    public ICourtDelegationUiService Delegation { get; }
    public IPersistenceUiService Persistence { get; }
}

using DonghanEngine.Core;

namespace DonghanEngine.Core.Economy;

public interface ICadastralSurveyService
{
    CadastralSurveyResult ExecuteCadastralSurvey(GameState state, string provinceId, CadastralSurveyIntensity intensity);
}

public interface IIrrigationService
{
    IrrigationProjectResult ConstructIrrigation(GameState state, string provinceId);
}

using System;

namespace DonghanEngine.Core;

public class ArmyState
{
    private int _size = 8000;
    private int _basePayPerTurn = 120;
    private int _morale = 55;
    private int _loyalty = 50;

    public int Size
    {
        get => _size;
        set => _size = Math.Max(0, value);
    }
    
    public int BasePayPerTurn
    {
        get => _basePayPerTurn;
        set => _basePayPerTurn = Math.Max(0, value);
    }
    
    public int Morale
    {
        get => _morale;
        set => _morale = Math.Clamp(value, 0, 100);
    }
    
    public int Loyalty
    {
        get => _loyalty;
        set => _loyalty = Math.Clamp(value, 0, 100);
    }

    // === 充血业务方法 ===
    public void AdjustMorale(int delta) => Morale += delta;
    public void AdjustLoyalty(int delta) => Loyalty += delta;
    public void AdjustSize(int delta) => Size += delta;
    public void TakeCasualties(int casualty) => Size = Math.Max(0, Size - casualty);
}

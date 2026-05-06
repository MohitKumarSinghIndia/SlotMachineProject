using UnityEngine;

public static class SymbolMapper
{
    public static SymbolType GetSymbolType(int id)
    {
        switch (id)
        {
            case 0: return SymbolType.Scatter;
            case 1: return SymbolType.Wild;
            case 2: return SymbolType.HV_1;
            case 3: return SymbolType.HV_2;
            case 4: return SymbolType.HV_3;
            case 5: return SymbolType.MV_1;
            case 6: return SymbolType.MV_2;
            case 7: return SymbolType.MV_3;
            case 8: return SymbolType.LV_1;
            case 9: return SymbolType.LV_2;
            case 10: return SymbolType.LV_3;

            default:
                Debug.LogWarning($"Unknown Symbol ID: {id}");
                return SymbolType.LV_1;
        }
    }
}
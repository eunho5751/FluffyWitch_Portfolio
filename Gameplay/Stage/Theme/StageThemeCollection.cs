using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class StageThemeCollection : IReadOnlyList<StageThemeSettings>
{
    [SerializeField]
    private StageThemeSettings[] _themes;
    
    public int GetThemeIndexByStage(int stage)
    {
        int maxStage = 0;
        for (int i = 0; i < _themes.Length; i++)
        {
            maxStage += _themes[i].MaxStage;
            if (stage <= maxStage)
                return i;
        }
        return -1;
    }

    public int GetLocalStage(int stage)
    {
        int themeIndex = GetThemeIndexByStage(stage);
        int localStage = stage;
        for (int i = 0; i < themeIndex; i++)
            localStage -= _themes[i].MaxStage;
        return localStage;
    }

    public IEnumerator<StageThemeSettings> GetEnumerator() => _themes.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int TotalStage
    {
        get
        {
            int total = 0;
            for (int i = 0; i < _themes.Length; i++)
                total += _themes[i].MaxStage;
            return total;
        }
    }

    public StageThemeSettings this[int index] => _themes[index];
    public int Count => _themes.Length;
}
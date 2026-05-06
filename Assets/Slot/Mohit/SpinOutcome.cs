using System.Collections.Generic;

[System.Serializable]
public class SpinOutcome
{
    public string SpinId;

    public List<ReelOutcome> Reels;
}

[System.Serializable]
public class ReelOutcome
{
    public int ReelIndex;

    public List<int> VisibleSymbolIds;
}
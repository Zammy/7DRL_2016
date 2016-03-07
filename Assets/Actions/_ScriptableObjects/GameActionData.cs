using UnityEngine;

public class GameActionData : ScriptableObject
{
    public string Name;
    public string Description;

    public int Length;
    public int StaminaCost;
    public int Range;
}

public static class GameActionDataExt
{
    public static string GetLengthInSecs(int length)
    {
        return ((double)length / 1000f).ToString("F");
    }
}
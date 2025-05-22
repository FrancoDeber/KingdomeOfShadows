using UnityEngine;

public class StatData
{
    public StatType Type;
    public float Value;

    public StatData(StatType type, float value)
    {
        Type = type;
        Value = value;
    }
}

using UnityEngine;
public class PowerUpData
{
    public PowerUpType type;
    public float value;
    public string diplayName;

    public PowerUpData(PowerUpType type, float value, string displayName)
    {
        this.type = type;
        this.value = value;
        this.diplayName = displayName;
    }
}

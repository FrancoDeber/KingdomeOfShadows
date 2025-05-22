using UnityEngine;

[System.Serializable]
public class BulletPassiveData
{
    public BulletPassiveType type = BulletPassiveType.None;
    public int dotDamage;
    public float dotDuration;
    public float dotTickRate;
    public Color color;

    public BulletPassiveData(BulletPassiveType type, int dotDamage, float dotDuration,float dotTickRate, Color color)
    {
        this.type = type;
        this.dotDamage = dotDamage;
        this.dotDuration = dotDuration;
        this.dotTickRate = dotTickRate;
        this.color = color;
    }
}
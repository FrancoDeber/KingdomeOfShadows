using UnityEngine;
using System.Collections.Generic;

public static class PowerUpInfo
{
    public static Dictionary<PowerUpType, PowerUpData> Data = new()
    {
        // Health
        { PowerUpType.HealOne,      new PowerUpData(PowerUpType.HealOne, 1f, "Heal One Hearth") },
        { PowerUpType.Life,      new PowerUpData(PowerUpType.Life, 1f, "One More Life") },
        { PowerUpType.HealFull,   new PowerUpData(PowerUpType.HealFull, 0f, "Full Health") },
        { PowerUpType.MaxHealth, new PowerUpData(PowerUpType.MaxHealth, 1f, "Max Hearth Up") },
        { PowerUpType.InvulnerableTime,     new PowerUpData(PowerUpType.InvulnerableTime, 0.2f, "Invulnerability Time Up") },
        { PowerUpType.KnockbackResistance,     new PowerUpData(PowerUpType.KnockbackResistance, 0.1f, "Knockback Resistance Up") },
        
        // Movement
        { PowerUpType.MoveSpeed,   new PowerUpData(PowerUpType.MoveSpeed,  1f, "Move Speed Up") },
        { PowerUpType.MaxDashCount,   new PowerUpData(PowerUpType.MaxDashCount,  1f, "Max Dash Up") },
        { PowerUpType.DashSpeed,   new PowerUpData(PowerUpType.DashSpeed,  1f, "Dash Speed Up") },
        { PowerUpType.DashDistance,   new PowerUpData(PowerUpType.DashDistance,  0.3f,"Dash Distance Up") },
        { PowerUpType.DashRechargeTime,   new PowerUpData(PowerUpType.DashRechargeTime,  -0.2f, "Dash Recharge Cooldown Reduce") },
        
        // Damage
        { PowerUpType.Damage,   new PowerUpData(PowerUpType.Damage, 1f, "Damage Up") },

        // Shot
        { PowerUpType.ShotRange,   new PowerUpData(PowerUpType.ShotRange,  1f, "Shot Range Up") },
        { PowerUpType.ShotSpeed,   new PowerUpData(PowerUpType.ShotSpeed,  1f, "Shot Speed Up") },
        { PowerUpType.ShotSize,   new PowerUpData(PowerUpType.ShotSize,  1f, "Shot Size Up") },
        { PowerUpType.ShotRate,   new PowerUpData(PowerUpType.ShotRate,  -0.2f, "Shot Rate Up") },
        { PowerUpType.ShotKnockback,   new PowerUpData(PowerUpType.ShotKnockback,  0.4f, "Shot Knockback Up") }
    };
}

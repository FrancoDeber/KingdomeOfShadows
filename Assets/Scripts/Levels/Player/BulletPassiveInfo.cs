using UnityEngine;
using System.Collections.Generic;

public static class BulletPassiveInfo
{
    public static Dictionary<BulletPassiveType, BulletPassiveData> Data = new()
    {
        { BulletPassiveType.None, new BulletPassiveData(BulletPassiveType.None,0,0f,0f,Color.red) },
        { BulletPassiveType.Poison, new BulletPassiveData(BulletPassiveType.Poison, 1,10f,2f,  new Color(0.2f, 1f, 0.2f) /*verde fosforescente*/) },
        { BulletPassiveType.Fire, new BulletPassiveData(BulletPassiveType.Fire, 1,10f,2f, new Color(1f, 0.4f, 0f) /* Naranja fuego */) },
        { BulletPassiveType.Acid,     new BulletPassiveData(BulletPassiveType.Acid, 1,10f,2f, new Color(0.6f, 1f, 0.2f) /* verde ácido brillante*/ )},
        { BulletPassiveType.Ice,     new BulletPassiveData(BulletPassiveType.Ice, 1,10f,2f, new Color(0.6f, 0.9f, 1f) /* azul muy claro */ )}
    };
}

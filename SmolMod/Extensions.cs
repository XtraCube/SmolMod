using UnityEngine;

namespace SmolMod;

public static class Extensions
{
    public static Transform AdjustScale(this Transform transform, float scaleMod)
    {
        transform.localScale *= scaleMod;
        return transform;
    }
    
    public static Transform AdjustPosition(this Transform transform, float scaleMod)
    {
        var pos = transform.localPosition;
        transform.localPosition = new Vector3(pos.x, pos.y, pos.z * scaleMod);
        return transform;
    }
}
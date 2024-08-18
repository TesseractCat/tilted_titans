using UnityEngine;

public static class Vector3Extensions
{
    public static Vector2 xz(this Vector3 p)
    {
        return new Vector2(p.x, p.z);
    }
}
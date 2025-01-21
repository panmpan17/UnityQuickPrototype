using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    public static Quaternion LookRotation2D(Vector2 direction, float offset = 0)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + offset);
    }
}

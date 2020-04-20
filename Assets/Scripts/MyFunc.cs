using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyFunc
{
    // Start is called before the first frame update
    public static Vector3 Div(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
    public static Vector3 Mul(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }
}

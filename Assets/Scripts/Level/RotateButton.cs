using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateButton : Button
{
    public Vector3 rotation;
    public Transform targetTransform;
    Vector3 originRotation;
    Quaternion targetRotation;
    float T = 0;
    float targetT = 0;
    public override void Start()
    {
        base.Start();
        originRotation = targetTransform.rotation.eulerAngles;
    }
    public override void FixedUpdate()
    {

        base.FixedUpdate();
        if (T < targetT)
            T = Mathf.Clamp(T + Time.fixedDeltaTime, T, targetT);
        else if (T > targetT)
            T = Mathf.Clamp(T - Time.fixedDeltaTime, targetT,T);
        targetTransform.rotation = Quaternion.Slerp(Quaternion.Euler(originRotation), Quaternion.Euler(rotation), T);
    }
    public override void EnterTouch()
    {
        targetT = 1;
    }
    public override void OutTouch()
    {
        targetT = 0;
    }
}

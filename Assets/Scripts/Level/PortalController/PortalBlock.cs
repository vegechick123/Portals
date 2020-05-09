using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalBlock : MonoBehaviour
{
    // Start is called before the first frame update

    private Material material;
    private Collider colider;
    public float dissolveSpeed=0.4f;
    public float min=0;
    public float max = 1f;
    public float time=0;
    float targetTime;
    private void Awake()
    {
        colider = GetComponent<Collider>();
        material = GetComponent<Renderer>().material;
        targetTime = time;
    }
    private void Update()
    {
        if (time < targetTime)
            time = Mathf.Clamp(time + Time.deltaTime*dissolveSpeed, time, targetTime);
        else if (time > targetTime)
            time = Mathf.Clamp(time - Time.deltaTime * dissolveSpeed, targetTime, time);
        material.SetFloat("_Dissolve", time);
    }
    public void Destory()
    {
        if(colider!=null)
            colider.enabled = false;
        targetTime = max;
    }
    public void Restore()
    {
        if (colider != null)
            colider.enabled = true;
        targetTime = min;
    }
}

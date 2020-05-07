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
    private void Awake()
    {
        colider = GetComponent<Collider>();
        material = GetComponent<Renderer>().material;
    }
    private void Start()
    {
        
    }
    private IEnumerator Dissolve()
    {
        float time = min;
        while(true)
        {
            time += Time.deltaTime*dissolveSpeed;
            if (time > max)
            {
                yield break;
            }
            else
            {
                material.SetFloat("_Dissolve", time);
                yield return null;
            }

        }
        
    }
    private IEnumerator RestoreDissolve()
    {
        float time = max;
        while (true)
        {
            time -= Time.deltaTime * dissolveSpeed;
            if (time <= min)
            {
                yield break;
            }
            else
            {
                material.SetFloat("_Dissolve", time);
                yield return null;
            }

        }

    }
    public void Destory()
    {
        if(colider!=null)
            colider.enabled = false;
        StartCoroutine(Dissolve());
    }
    public void Restore()
    {
        if (colider != null)
            colider.enabled = true;
        StartCoroutine(RestoreDissolve());
    }
}

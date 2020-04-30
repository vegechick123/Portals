using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalBlock : MonoBehaviour
{
    // Start is called before the first frame update

    private Material material;
    private Collider colider;
    private float dissolveSpeed=0.2f;
    private void Awake()
    {
        colider = GetComponent<Collider>();
        material = GetComponent<Renderer>().material;
    }
    private void Start()
    {
        //Destory();
    }
    private IEnumerator Dissolve()
    {
        float time = 0;
        while(true)
        {
            time += Time.deltaTime*dissolveSpeed;
            if (time > 1)
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
        float time = 1;
        while (true)
        {
            time -= Time.deltaTime * dissolveSpeed;
            if (time <= 0)
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
        colider.enabled = false;
        StartCoroutine(Dissolve());
    }
    public void Restore()
    {
        colider.enabled = true;
        StartCoroutine(RestoreDissolve());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    // Start is called before the first frame update
    protected Portal curPortal;
    protected bool lastVisble;
    protected void Awake()
    {
        curPortal = GetComponent<Portal>();
        lastVisble = curPortal.isVisble;
    }
    protected void LateUpdate()
    {
        //Debug.Log(lastVisble + " " + curPortal.isVisble);
        if (lastVisble == true && curPortal.isVisble == false)
            OnOutOfSight();
        lastVisble = curPortal.isVisble;
    }
    virtual protected void OnOutOfSight()
    {

    }
    virtual public void OnOutToPortal()
    {

    }
    virtual public void OnComeToPortal()
    {

    }
}

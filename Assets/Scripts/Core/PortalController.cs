using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    // Start is called before the first frame update
    protected Portal curPortal;
    protected bool lastVisble;
    virtual protected void Awake()
    {
        curPortal = GetComponent<Portal>();
        lastVisble = curPortal.isVisble;
    }
    protected void LateUpdate()
    {
        //Debug.Log(lastVisble + " " + curPortal.isVisble);
        if (lastVisble == true && curPortal.isVisble == false)
            OnOutOfSight();
        if (lastVisble == false&& curPortal.isVisble == true)
            OnIntoSight();
        lastVisble = curPortal.isVisble;
    }
    virtual protected void OnOutOfSight()
    {

    }
    virtual protected void OnIntoSight()
    {

    }
    virtual public void OnOutFromPortal(Portal toPortal)
    {

    }
    virtual public void OnComeToPortal(Portal FromPortal)
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal1 : PortalController
{
    // Start is called before the first frame update
    override protected void OnOutofSight()
    {
        curPortal.linkedPortal = curPortal.originLinkedPortal;
        Debug.Log(1);
    }
}

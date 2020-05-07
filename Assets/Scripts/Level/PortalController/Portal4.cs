using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal4 : PortalController
{
    // Start is called before the first frame update
    public Portal linkPortal1;
    public Portal linkPortal2;
    override public void OnComeToPortal(Portal fromPortal)
    {
        if (fromPortal = linkPortal1)
            curPortal.linkedPortal = linkPortal1;
        else if(fromPortal = linkPortal2)
            curPortal.linkedPortal = linkPortal2;

    }
    override public void OnOutFromPortal(Portal toPortal)
    {
        if (toPortal = linkPortal1)
            curPortal.linkedPortal = linkPortal2;
        else if (toPortal = linkPortal2)
            curPortal.linkedPortal = linkPortal1;

    }
}
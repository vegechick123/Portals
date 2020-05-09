using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal7 : PortalController
{
    
    private bool canDisappear=false;
    private void FixedUpdate()
    {
        if (canDisappear && !curPortal.isVisble)
            gameObject.SetActive(false);
    }
    override public void OnComeToPortal(Portal fromPortal)
    {
        canDisappear = true;
    }
}
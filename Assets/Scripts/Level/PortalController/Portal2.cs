﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal2 : PortalController
{
    // Start is called before the first frame update
    public PortalBlock portalBlock;
    
    private bool canDisappear=false;
    private void FixedUpdate()
    {
        if (canDisappear && !curPortal.isVisble)
            gameObject.SetActive(false);
    }
    override public void OnComeToPortal(Portal fromPortal)
    {
        canDisappear = true;
        portalBlock.Destory();
        Debug.Log(2);
    }
}
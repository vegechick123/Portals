using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal5 : PortalController
{
    public PortalBlock portalBlock;
    protected override void Awake()
    {
        base.Awake();
    }
    override public void OnComeToPortal(Portal FromPortal)
    {
        portalBlock.Destory();
        GetComponentInChildren<PortalBlock>().Destory();
    }
}
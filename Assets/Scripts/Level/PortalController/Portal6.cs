using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal6 : PortalController
{
    public PortalBlock portalBlock;
    public Portal portal;
    protected override void Awake()
    {
        base.Awake();
    }
    override public void OnOutFromPortal(Portal toPortal)
    {
        portal.enabled=true;
        portal.screen.enabled = true;
        portalBlock.Restore();
    }
}
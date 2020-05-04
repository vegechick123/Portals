using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal3 : PortalController
{
    // Start is called before the first frame update
    public PortalBlock portalBlock;
    protected override void Awake()
    {
        base.Awake();
        portalBlock = GetComponentInChildren<PortalBlock>();
    }
    override protected void OnOutOfSight()
    {
        //gameObject.SetActive(false);
    }
    override public void OnComeToPortal()
    {
        portalBlock.Destory();
    }
}
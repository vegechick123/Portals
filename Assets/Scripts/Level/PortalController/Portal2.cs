using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal2 : PortalController
{
    // Start is called before the first frame update
    public PortalBlock portalBlock;
    override protected void OnOutOfSight()
    {
        gameObject.SetActive(false);
    }
    override public void OnComeToPortal()
    {
        portalBlock.Destory();
        Debug.Log(2);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal2 : PortalController
{
    // Start is called before the first frame update
    public PortalBlock portalBlock;
    
    private bool canDisappear=false;
    override protected void OnOutOfSight()
    {
        if (canDisappear)
        {
            GetComponent<Portal>().ReleaseAllTraveller();
            gameObject.SetActive(false);
        }
    }
    override public void OnComeToPortal(Portal fromPortal)
    {
        canDisappear = true;
        portalBlock.Destory();
        Debug.Log(2);
    }
}
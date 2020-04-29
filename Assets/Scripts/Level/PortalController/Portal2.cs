using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal2 : PortalController
{
    // Start is called before the first frame update
    override protected void OnOutofSight()
    {
        gameObject.SetActive(false);
    }
}
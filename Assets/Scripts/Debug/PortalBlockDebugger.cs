using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalBlockDebugger : MonoBehaviour
{
    PortalBlock portalBlock;
    void Awake()
    {
        portalBlock = GetComponent<PortalBlock>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            portalBlock.Destory();
        else if(Input.GetKeyDown(KeyCode.E))
            portalBlock.Restore();
    }
}

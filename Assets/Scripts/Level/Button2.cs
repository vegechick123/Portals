using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button2: Button
{
    public PortalBlock portalBlockp;
    public Portal portal;
    public override void EnterTouch()
    {
        portal.enabled = true;
        portalBlockp.Restore();
    }
    public override void OutTouch()
    {
        portal.enabled = false;
        portalBlockp.Destory();
    }
}

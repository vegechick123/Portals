using UnityEngine;

public class MainCamera : MonoBehaviour
{

    public static Portal[] portals;
    Camera playerCam;
    void Awake()
    {
        portals = FindObjectsOfType<Portal>();
        playerCam = GetComponent<Camera>();
    }

    void OnPreCull()
    {

        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PrePortalRender();
        }
        bool[] vis = new bool[portals.Length];
        Portal.renderCount = 0;
        for (int i = 0; i < portals.Length; i++)
        {
            Portal curPortal = portals[i];
            if (curPortal.enabled == true)
            {
                if (!curPortal.Judge(playerCam, curPortal.recursionLimit))
                {
                    vis[i] = false;
                    continue;
                }
                else
                    vis[i] = true;

                curPortal.Render(playerCam, curPortal.recursionLimit);
            }
        }
        FPSDisplay.PutMessage("renderCount:" + Portal.renderCount, false);
        for (int i = 0; i < portals.Length; i++)
        {
            if (!vis[i])
                continue;
            Portal curPortal = portals[i];
            curPortal.SetViewTexture();
        }
        for (int i = 0; i < portals.Length; i++)
        {
            if (!vis[i])
                continue;
            Portal curPortal = portals[i];
            curPortal.ReleaseViewTexture();
        }
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].PostPortalRender();
        }

    }

}


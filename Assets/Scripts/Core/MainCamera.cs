using UnityEngine;

public class MainCamera : MonoBehaviour {

    public static Portal[] portals;
    public static int recursionLimit=1;
    Camera playerCam;
    void Awake () {
        portals = FindObjectsOfType<Portal> ();
        playerCam=GetComponent<Camera>();
    }

    void OnPreCull () {

        for (int i = 0; i < portals.Length; i++) {
            portals[i].PrePortalRender ();
        }
        Portal.renderCount = 0;
        for (int i = 0; i < portals.Length; i++) {
            portals[i].Render (playerCam, recursionLimit);
            
        }
        FPSDisplay.PutMessage("renderCount:" + Portal.renderCount,false);
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].SetViewTexture(recursionLimit);

        }
        for (int i = 0; i < portals.Length; i++) {
            portals[i].PostPortalRender ();
        }

    }

}


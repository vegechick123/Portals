using UnityEngine;

public class MainCamera : MonoBehaviour {

    Portal[] portals;
    Camera camera;
    void Awake () {
        portals = FindObjectsOfType<Portal> ();
    }

    void OnPreCull () {

        for (int i = 0; i < portals.Length; i++) {
            portals[i].PrePortalRender ();
        }
        for (int i = 0; i < portals.Length; i++) {
            portals[i].Render (camera);
        }

        for (int i = 0; i < portals.Length; i++) {
            portals[i].PostPortalRender ();
        }

    }

}


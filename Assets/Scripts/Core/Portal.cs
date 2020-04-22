using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {
    [Header ("Main Settings")]
    public Portal linkedPortal;
    public MeshRenderer screen;
    public Material testMat;
    public int recursionLimit = 5;

    [Header ("Advanced Settings")]
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;
    public float sliceOffset = 0.2f;
    // Private variables
    RenderTexture viewTexture;
    RenderTexture viewBuffer;
    Camera portalCam;
    Camera viewCam;
    
    Material firstRecursionMat;
    List<PortalTraveller> trackedTravellers;
    MeshFilter screenMeshFilter;
    public bool DebugButton;
    void Awake () {
        viewCam = Camera.main;
        portalCam = GetComponentInChildren<Camera> ();
        portalCam.enabled = false;
        portalCam.projectionMatrix = viewCam.projectionMatrix;
        trackedTravellers = new List<PortalTraveller> ();
        screenMeshFilter = screen.GetComponent<MeshFilter> ();
        screen.material.SetInt ("displayMask", 1);
    }

    void LateUpdate () {
        HandleTravellers ();
    }

    void HandleTravellers () {

        for (int i = 0; i < trackedTravellers.Count; i++) {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerT = traveller.transform;
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;
         
            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign (Vector3.Dot (offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign (Vector3.Dot (traveller.previousOffsetFromPortal, transform.forward));
            // Teleport the traveller if it has crossed from one side of the portal to the other
            if (portalSide != portalSideOld) {
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;
                traveller.Teleport (transform, traveller.graphicsClone.transform, m.GetColumn (3), m.rotation);
                traveller.graphicsClone.transform.SetPositionAndRotation (positionOld, rotOld);
                traveller.graphicsClone.transform.localScale = MyFunc.Div(transform.lossyScale, linkedPortal.transform.lossyScale);
                // Can't rely on OnTriggerEnter/Exit to be called next frame since it depends on when FixedUpdate runs
                linkedPortal.OnTravellerEnterPortal (traveller);
                trackedTravellers.RemoveAt (i);
                i--;

            } else {
                traveller.graphicsClone.transform.SetPositionAndRotation (m.GetColumn (3), m.rotation);  
                traveller.graphicsClone.transform.localScale =MyFunc.Div(linkedPortal.transform.lossyScale,transform.lossyScale);
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    // Called before any portal cameras are rendered for the current frame
    public void PrePortalRender () {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams (traveller);
        }
    }

    // Manually render the camera attached to this portal
    // Called after PrePortalRender, and before PostPortalRender
    public void Render(Camera _viewCamera,int limit)
    {
        // Skip rendering the view from this portal if player is not looking at the linked portal
        if (!CameraUtility.VisibleFromCamera(screen, _viewCamera)||limit<0)
        {
            return;
        }
        viewCam = _viewCamera;

        var localToWorldMatrix = viewCam.worldToCameraMatrix ;
        localToWorldMatrix = localToWorldMatrix * transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix;
        

        portalCam.worldToCameraMatrix = localToWorldMatrix;
        //for (int i = 0; i < MainCamera.portals.Length; i++)
        //{
        //    Portal curPortal = MainCamera.portals[i];
        //    if (curPortal != linkedPortal)
        //    {
        //        curPortal.Render(portalCam, limit - 1);
        //        portalCam.worldToCameraMatrix = localToWorldMatrix;
        //    }
        //}
        

        CreateViewTexture();
        CreateViewBuffer();

        SetNearClipPlane();
        HandleClipping();
        // Hide screen so that camera can see through portal
        linkedPortal.screen.enabled = false;
        portalCam.Render();
        Graphics.Blit(viewBuffer, viewTexture);
        screen.material.SetInt("displayMask", 1);
        linkedPortal.screen.enabled = true;
    }
    void SetNearClipPlane()
    {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        Transform clipPlane = linkedPortal.transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward,  linkedPortal.transform.position-portalCamPos));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float scalez = Mathf.Sqrt((portalCam.cameraToWorldMatrix[0, 2] * portalCam.cameraToWorldMatrix[0, 2] + portalCam.cameraToWorldMatrix[2, 2] * portalCam.cameraToWorldMatrix[2, 2]));
        //Debug.Log(scalez);
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset/scalez;

        // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            // Update projection based on new clip plane
            // Calculate matrix with player cam so that player camera settings (fov, etc) are used
            portalCam.projectionMatrix = viewCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCam.projectionMatrix = viewCam.projectionMatrix;
        }

    }
    void HandleClipping () {
        // There are two main graphical issues when slicing travellers
        // 1. Tiny sliver of mesh drawn on backside of portal
        //    Ideally the oblique clip plane would sort this out, but even with 0 offset, tiny sliver still visible
        // 2. Tiny seam between the sliced mesh, and the rest of the model drawn onto the portal screen
        // This function tries to address these issues by modifying the slice parameters when rendering the view from the portal
        // Would be great if this could be fixed more elegantly, but this is the best I can figure out for now
        const float hideDst = -1000;
        const float showDst = 1000;
        float screenThickness = ProtectScreenFromClipping (portalCamPos);

        foreach (var linkTraveller in linkedPortal.trackedTravellers) {
            if (linkedPortal.SameSideOfPortal (linkTraveller.transform.position, portalCamPos)) {
                // Addresses issue 1
                linkTraveller.SetSliceOffsetDst (hideDst, false);
            } else {
                // Addresses issue 2
                linkTraveller.SetSliceOffsetDst (showDst, false);
            }

            // Ensure clone is properly sliced, in case it's visible through this portal:
            int cloneSideOfLinkedPortal = -linkedPortal.SideOfPortal (linkTraveller.transform.position);
            bool camSameSideAsClone = SideOfPortal (portalCamPos) == cloneSideOfLinkedPortal;
            if (camSameSideAsClone) {
                linkTraveller.SetSliceOffsetDst (screenThickness, true);
            } else {
                linkTraveller.SetSliceOffsetDst (-screenThickness, true);
            }
        }

        var offsetFromPortalToCam = portalCamPos -  linkedPortal.transform.position;
        foreach (var traveller in trackedTravellers) {
            var travellerPos = traveller.graphicsObject.transform.position;
            var clonePos = traveller.graphicsClone.transform.position;
            // Handle clone of linked portal coming through this portal:
            bool cloneOnSameSideAsCam = SideOfPortal (travellerPos) != linkedPortal.SideOfPortal (portalCamPos);
            if (cloneOnSameSideAsCam) {
                // Addresses issue 1
                traveller.SetSliceOffsetDst (hideDst, true);
            } else {
                // Addresses issue 2
                traveller.SetSliceOffsetDst (showDst, true);
            }

            // Ensure traveller of linked portal is properly sliced, in case it's visible through this portal:
            bool camSameSideAsTraveller = SameSideOfPortal (traveller.transform.position, portalCamPos);
            if (camSameSideAsTraveller) {
                traveller.SetSliceOffsetDst (screenThickness, false);
            } else {
                traveller.SetSliceOffsetDst (-screenThickness, false);
            }
        }
    }

    // Called once all portals have been rendered, but before the player camera renders
    public void PostPortalRender () {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams (traveller);
        }
        ProtectScreenFromClipping (viewCam.transform.position);
    }
    void CreateViewTexture () {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height) {
            if (viewTexture != null) {
                viewTexture.Release ();
            }
            viewTexture = new RenderTexture (Screen.width, Screen.height, 24);
            // Render the view from the portal camera to the view texture
            if(testMat!=null)
                testMat.mainTexture = viewTexture;
            // Display the view texture on the screen of the linked portal
            screen.material.SetTexture("_MainTex", viewTexture);
        }
    }
    void CreateViewBuffer()
    {
        if (viewBuffer == null || viewBuffer.width != Screen.width || viewBuffer.height != Screen.height)
        {
            if (viewBuffer != null)
            {
                viewBuffer.Release();
            }
            viewBuffer = new RenderTexture(Screen.width, Screen.height, 24);
            portalCam.targetTexture = viewBuffer;
            // Render the view from the portal camera to the view texture
            if (testMat != null)
                testMat.mainTexture = viewBuffer;
            // Display the view texture on the screen of the linked portal

            
        }
    }

    // Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
    float ProtectScreenFromClipping (Vector3 viewPoint) {
        float halfHeight = viewCam.nearClipPlane * Mathf.Tan (viewCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * viewCam.aspect;
        float dstToNearClipPlaneCorner =Vector3.Scale(viewCam.transform.lossyScale,new Vector3 (halfWidth, halfHeight, viewCam.nearClipPlane)).magnitude;
        float screenThickness = dstToNearClipPlaneCorner+sliceOffset;

        Transform screenT = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot (transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3 (screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 1f: -1f);
        return screenThickness;
    }

    void UpdateSliceParams (PortalTraveller traveller) {
        // Calculate slice normal
        int side = SideOfPortal (traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;

        // Calculate slice centre
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = linkedPortal.transform.position;

        // Adjust slice offset so that when player standing on other side of portal to the object, the slice doesn't clip through
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = screen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal (viewCam.transform.position, traveller.transform.position);
        if (!playerSameSideAsTraveller) {
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsCloneAppearing = side != linkedPortal.SideOfPortal (viewCam.transform.position);
        if (!playerSameSideAsCloneAppearing) {
            cloneSliceOffsetDst = -screenThickness;
        }

        // Apply parameters
        for (int i = 0; i < traveller.originalMaterials.Length; i++) {
            traveller.originalMaterials[i].SetVector ("sliceCentre", slicePos);
            traveller.originalMaterials[i].SetVector ("sliceNormal", sliceNormal);
            traveller.originalMaterials[i].SetFloat ("sliceOffsetDst", sliceOffsetDst);

            traveller.cloneMaterials[i].SetVector ("sliceCentre", cloneSlicePos);
            traveller.cloneMaterials[i].SetVector ("sliceNormal", cloneSliceNormal);
            traveller.cloneMaterials[i].SetFloat ("sliceOffsetDst", cloneSliceOffsetDst);

        }

    }

    // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
    // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
  
    
    void OnTravellerEnterPortal (PortalTraveller traveller) {
        if (!trackedTravellers.Contains (traveller)) {
            traveller.EnterPortalThreshold ();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add (traveller);
        }
    }

    void OnTriggerEnter (Collider other) {
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller) {
            OnTravellerEnterPortal (traveller);
        }
    }

    void OnTriggerExit (Collider other) {
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller && trackedTravellers.Contains (traveller)) {
            traveller.ExitPortalThreshold ();
            trackedTravellers.Remove (traveller);
        }
    }

    /*
     ** Some helper/convenience stuff:
     */

    int SideOfPortal (Vector3 pos) {
        return System.Math.Sign (Vector3.Dot (pos - transform.position, transform.forward));
    }

    bool SameSideOfPortal (Vector3 posA, Vector3 posB) {
        return SideOfPortal (posA) == SideOfPortal (posB);
    }

    Vector3 portalCamPos {
        get {
            return portalCam.cameraToWorldMatrix.GetColumn(3);
        }
    }

    void OnValidate () {
        if (linkedPortal != null) {
            linkedPortal.linkedPortal = this;
        }
    }
}
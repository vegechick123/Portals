using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Main Settings")]
    public Portal linkedPortal;
    [HideInInspector]
    public Portal originLinkedPortal;
    public MeshRenderer screen;
    public Material testMat;
    
    [Header("Advanced Settings")]
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;
    public float sliceOffset = 0.2f;
    public static int renderCount = 0;
    public float hideDistance = 30;
    // Resursion 
    public int recursionLimit = 1;
    public bool useRecursionPortal;
    [SerializeField]
    public Portal[] recursionPortal;
    // Private variables
    Stack<RenderTexture> viewTexture;
    RenderTexture viewBuffer;
    Camera portalCam;
    Camera viewCam;

    new Collider collider;
    Material firstRecursionMat;
    List<PortalTraveller> trackedTravellers;
    MeshFilter screenMeshFilter;
    PortalController portalController;
    public bool DebugButton;
    void Awake()
    {
        portalController = GetComponent<PortalController>();
        viewCam = Camera.main;
        originLinkedPortal = linkedPortal;
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false;
        portalCam.projectionMatrix = viewCam.projectionMatrix;
        trackedTravellers = new List<PortalTraveller>();
        screenMeshFilter = screen.GetComponent<MeshFilter>();
        screen.material.SetInt("displayMask", 1);
        viewTexture = new Stack<RenderTexture>();
        collider=GetComponent<Collider>();
    }

    void LateUpdate()
    {
        HandleTravellers();
    }

    void HandleTravellers()
    {

        for (int i = 0; i < trackedTravellers.Count; i++)
        {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerT = traveller.transform;
            if (!traveller.gameObject.activeSelf||!traveller.collider.bounds.Intersects(collider.bounds))
            {
                traveller.ExitPortalThreshold();
                trackedTravellers.Remove(traveller);
                i--;
                continue;
            }
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));
            // Teleport the traveller if it has crossed from one side of the portal to the other
            if (portalSide != portalSideOld)
            {
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;
                traveller.Teleport(transform, traveller.graphicsClone.transform, m.GetColumn(3), m.rotation,m);
                traveller.graphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);
                traveller.graphicsClone.transform.localScale = MyFunc.Div(transform.lossyScale, linkedPortal.transform.lossyScale);
                // Can't rely on OnTriggerEnter/Exit to be called next frame since it depends on when FixedUpdate runs
                linkedPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                i--;
                Portal temp = linkedPortal;
                if (portalController != null)
                {
                    portalController.OnOutFromPortal(linkedPortal);
                }
                if (temp.portalController != null)
                    temp.portalController.OnComeToPortal(this);
            }
            else
            {
                traveller.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
                traveller.graphicsClone.transform.localScale = MyFunc.Div(m.lossyScale, travellerT.transform.lossyScale);
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    // Called before any portal cameras are rendered for the current frame
    public void PrePortalRender()
    {
        foreach (var traveller in trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
    }
    public bool Judge(Camera _viewCamera,int limit)
    {
        Vector3 viewCamerPos = _viewCamera.cameraToWorldMatrix.GetColumn(3);
        float distance = Vector3.Distance(viewCamerPos, transform.position);
        if (!CameraUtility.VisibleFromCamera(screen, _viewCamera) || limit < 0 || distance > hideDistance || linkedPortal.enabled == false || this.enabled == false || !linkedPortal.gameObject.activeInHierarchy || !gameObject.activeInHierarchy)
        {
            return false;
        }
        return true;
    }
    // Manually render the camera attached to this portal
    // Called after PrePortalRender, and before PostPortalRender
    public void Render(Camera _viewCamera, int limit)
    {
        if (linkedPortal == null)
            Debug.LogError(gameObject.name + "未连接传送门");
        // Skip rendering the view from this portal if player is not looking at the linked portal
        Vector3 viewCamerPos = _viewCamera.cameraToWorldMatrix.GetColumn(3);
        float distance = Vector3.Distance(viewCamerPos, transform.position);
        if (!CameraUtility.VisibleFromCamera(screen, _viewCamera) || limit < 0 || distance > hideDistance || linkedPortal.enabled == false || this.enabled == false || !linkedPortal.gameObject.activeInHierarchy || !gameObject.activeInHierarchy)
        {
            return;
        }
        viewCam = _viewCamera;

        var localToWorldMatrix = viewCam.worldToCameraMatrix;
        localToWorldMatrix = localToWorldMatrix * transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix;




        portalCam.worldToCameraMatrix = localToWorldMatrix;
        SetNearClipPlane();
        Matrix4x4 projection = portalCam.projectionMatrix;
        Portal[] portalArr;
        if (useRecursionPortal)
            portalArr = recursionPortal;
        else
            portalArr = MainCamera.portals;
        bool[] vis=new bool[portalArr.Length];
        for (int i = 0; i < portalArr.Length; i++)
        {
            Portal curPortal = portalArr[i];
            if (curPortal != linkedPortal && curPortal.enabled == true&&curPortal!=this)
            {
                if (!curPortal.Judge(portalCam, limit - 1))
                {
                    vis[i] = false;
                    continue;
                }
                else
                    vis[i] = true;

                curPortal.Render(portalCam, limit - 1);
                portalCam.worldToCameraMatrix = localToWorldMatrix;
                portalCam.projectionMatrix = projection;
                viewCam = _viewCamera;
            }

        }
        for (int i = 0; i < portalArr.Length; i++)
        {
            if (!vis[i])
                continue;
            Portal curPortal = portalArr[i];
            curPortal.SetViewTexture();
        }

        CreateViewTexture();
        CreateViewBuffer();

        SetNearClipPlane();
        HandleClipping();
        // Hide screen so that camera can see through portal
        linkedPortal.screen.enabled = false;
        portalCam.Render();
        renderCount++;
        Graphics.Blit(viewBuffer, viewTexture.Peek());
        RenderTexture.ReleaseTemporary(viewBuffer);
        for (int i = 0; i < portalArr.Length; i++)
        {
            if (!vis[i])
                continue;
            Portal curPortal = portalArr[i];
            curPortal.ReleaseViewTexture();
        }
        screen.material.SetInt("displayMask", 1);
        linkedPortal.screen.enabled = true;
        CancelClipping();
    }
    void SetNearClipPlane()
    {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        Transform clipPlane = linkedPortal.transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, linkedPortal.transform.position - portalCamPos));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float scalez = 1;// Mathf.Sqrt((portalCam.cameraToWorldMatrix[0, 2] * portalCam.cameraToWorldMatrix[0, 2] + portalCam.cameraToWorldMatrix[2, 2] * portalCam.cameraToWorldMatrix[2, 2]));
        //Debug.Log(scalez);
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset / scalez;

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
    void CancelClipping()
    {
        foreach (var linkTraveller in linkedPortal.trackedTravellers)
        {
            // Addresses issue 2
            linkTraveller.SetSliceOffsetDst(0, false);
        }



        foreach (var traveller in trackedTravellers)
        {
            traveller.SetSliceOffsetDst(0, true);

        }
    }
    void HandleClipping()
    {
        // There are two main graphical issues when slicing travellers
        // 1. Tiny sliver of mesh drawn on backside of portal
        //    Ideally the oblique clip plane would sort this out, but even with 0 offset, tiny sliver still visible
        // 2. Tiny seam between the sliced mesh, and the rest of the model drawn onto the portal screen
        // This function tries to address these issues by modifying the slice parameters when rendering the view from the portal
        // Would be great if this could be fixed more elegantly, but this is the best I can figure out for now
        const float hideDst = -1000;
        const float showDst = 1000;
        float screenThickness = ProtectScreenFromClipping(portalCamPos);

        foreach (var linkTraveller in linkedPortal.trackedTravellers)
        {
            if (linkedPortal.SameSideOfPortal(linkTraveller.transform.position, portalCamPos))
            {
                // Addresses issue 1
                linkTraveller.SetSliceOffsetDst(hideDst, false);
            }
            else
            {
                // Addresses issue 2
                linkTraveller.SetSliceOffsetDst(showDst, false);
            }

            // Ensure clone is properly sliced, in case it's visible through this portal:
            int cloneSideOfLinkedPortal = -linkedPortal.SideOfPortal(linkTraveller.transform.position);
            bool camSameSideAsClone = SideOfPortal(portalCamPos) == cloneSideOfLinkedPortal;
            if (camSameSideAsClone)
            {
                linkTraveller.SetSliceOffsetDst(screenThickness, true);
            }
            else
            {
                linkTraveller.SetSliceOffsetDst(-screenThickness, true);
            }
        }

        var offsetFromPortalToCam = portalCamPos - linkedPortal.transform.position;
        foreach (var traveller in trackedTravellers)
        {
            var travellerPos = traveller.graphicsObject.transform.position;
            var clonePos = traveller.graphicsClone.transform.position;
            // Handle clone of linked portal coming through this portal:
            bool cloneOnSameSideAsCam = SideOfPortal(travellerPos) != linkedPortal.SideOfPortal(portalCamPos);
            if (cloneOnSameSideAsCam)
            {
                // Addresses issue 1
                traveller.SetSliceOffsetDst(hideDst, true);
            }
            else
            {
                // Addresses issue 2
                traveller.SetSliceOffsetDst(showDst, true);
            }

            // Ensure traveller of linked portal is properly sliced, in case it's visible through this portal:
            bool camSameSideAsTraveller = SameSideOfPortal(traveller.transform.position, portalCamPos);
            if (camSameSideAsTraveller)
            {
                traveller.SetSliceOffsetDst(screenThickness, false);
            }
            else
            {
                traveller.SetSliceOffsetDst(-screenThickness, false);
            }
        }
    }

    // Called once all portals have been rendered, but before the player camera renders
    public void PostPortalRender()
    {
        foreach (var traveller in trackedTravellers)
        {
            UpdateSliceParams(traveller);
        }
        ProtectScreenFromClipping(Camera.main.transform.position);
    }
    void CreateViewTexture()
    {

        int level = 1;
        viewTexture.Push(RenderTexture.GetTemporary(Screen.width / level, Screen.height / level, 24));
    }
    public void ReleaseViewTexture()
    {
        RenderTexture.ReleaseTemporary(viewTexture.Pop());
    }
    public void SetViewTexture()
    {
        screen.material.mainTexture = viewTexture.Peek();
    }
    void CreateViewBuffer()
    {
        viewBuffer = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        portalCam.targetTexture = viewBuffer;

    }

    // Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
    float ProtectScreenFromClipping(Vector3 viewPoint)
    {

        float screenThickness;
        if (viewCam == Camera.main)
        {
            float halfHeight = viewCam.nearClipPlane * Mathf.Tan(viewCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * viewCam.aspect;
            float dstToNearClipPlaneCorner = Vector3.Scale(viewCam.transform.lossyScale, new Vector3(halfWidth, halfHeight, viewCam.nearClipPlane)).magnitude;
            screenThickness = dstToNearClipPlaneCorner + sliceOffset;
        }
        else
            screenThickness = 0;

        Transform screenT = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 1f : -1f);
        return screenThickness;
    }

    void UpdateSliceParams(PortalTraveller traveller)
    {
        // Calculate slice normal
        int side = SideOfPortal(traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;

        // Calculate slice centre
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = linkedPortal.transform.position;

        // Adjust slice offset so that when player standing on other side of portal to the object, the slice doesn't clip through
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = screen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal(viewCam.transform.position, traveller.transform.position);
        if (!playerSameSideAsTraveller)
        {
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsCloneAppearing = side != linkedPortal.SideOfPortal(viewCam.transform.position);
        if (!playerSameSideAsCloneAppearing)
        {
            cloneSliceOffsetDst = -screenThickness;
        }

        // Apply parameters
        for (int i = 0; i < traveller.originalMaterials.Length; i++)
        {
            traveller.originalMaterials[i].SetVector("sliceCentre", slicePos);
            traveller.originalMaterials[i].SetVector("sliceNormal", sliceNormal);
            traveller.originalMaterials[i].SetFloat("sliceOffsetDst", sliceOffsetDst);

            traveller.cloneMaterials[i].SetVector("sliceCentre", cloneSlicePos);
            traveller.cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
            traveller.cloneMaterials[i].SetFloat("sliceOffsetDst", cloneSliceOffsetDst);

        }

    }

    // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
    // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO


    void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        if (!trackedTravellers.Contains(traveller))
        {
            if (linkedPortal.linkedPortal != this)
                linkedPortal.linkedPortal = this;
            traveller.EnterPortalThreshold();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add(traveller);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && enabled)
        {
            OnTravellerEnterPortal(traveller);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && trackedTravellers.Contains(traveller) && enabled)
        {
            traveller.ExitPortalThreshold();
            trackedTravellers.Remove(traveller);
        }
    }
    public void ReleaseAllTraveller()
    {
        foreach (PortalTraveller traveller in trackedTravellers)
        {
            traveller.ExitPortalThreshold();

        }
        trackedTravellers.Clear();
    }
    /*
     ** Some helper/convenience stuff:
     */

    int SideOfPortal(Vector3 pos)
    {
        return System.Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
    }

    bool SameSideOfPortal(Vector3 posA, Vector3 posB)
    {
        return SideOfPortal(posA) == SideOfPortal(posB);
    }

    Vector3 portalCamPos
    {
        get
        {
            return portalCam.cameraToWorldMatrix.GetColumn(3);
        }
    }
    public bool isVisble
    {
        get
        {
            return CameraUtility.VisibleFromCamera(screen, Camera.main);
        }
    }
    //void OnValidate () {
    //    if (linkedPortal != null) {
    //        linkedPortal.linkedPortal = this;
    //    }
    //}
}
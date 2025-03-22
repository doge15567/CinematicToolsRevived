using HarmonyLib;
using Il2Cpp;
using Il2CppSLZ.Bonelab.SaveData;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using System.Collections;
using UnityEngine.Rendering.Universal;

namespace CinematicToolsRevived;

internal static class TheHandler
{
    public static float AvatarCloneDelay = 1f;
    public static bool makeHeldObjectsKinematic = false;
    private static Camera _cineCamera;
    private static float _cineCamFoV = 90f;
    private static GameObject _lastClonedAvatarRef;
    private static readonly List<GameObject> ClonedAvatars = [];
    private static bool _captureAvatar;
    
    #region Camera

    public static void SpawnCamera()
    {
        var head = Player.Head.transform;
        if (_cineCamera)
        {
            _cineCamera.enabled = true;
            _cineCamera.transform.SetPositionAndRotation(head.transform.position, head.transform.rotation);
            return;
        }
        
        
        var cameraObj = new GameObject("CineTools_Camera");
        if (!cameraObj) return;
        cameraObj.SetActive(false);
        cameraObj.transform.SetPositionAndRotation(head.transform.position, head.transform.rotation);
        var camera = cameraObj.AddComponent<Camera>();
        _cineCamera = camera;
        var UACD = cameraObj.AddComponent<UniversalAdditionalCameraData>();
        UACD.allowXRRendering = false;
        camera.stereoTargetEye = 0;
        camera.depth = 100f;
        camera.depthTextureMode = DepthTextureMode.Depth;
        camera.depthTextureMode = DepthTextureMode.MotionVectors;
        camera.fieldOfView = _cineCamFoV;
        camera.farClipPlane = 10000f;
        camera.nearClipPlane = 0.01f;

        VolumetricRendering volumetricRendering = cameraObj.AddComponent<VolumetricRendering>();
        //CopyComponent(Player.RigManager.GetComponentInChildren<VolumetricRendering>(), cameraObj);
        volumetricRendering.cam = camera;
        volumetricRendering.activeCam = camera;
        volumetricRendering.activeCamData = UACD;
        GraphicsManager.SetVolumetricVariables(GraphicsManager.graphicsSettings._Volumetrics_k__BackingField); // re-init with BL player volumetric settings

        cameraObj.SetActive(true);
        camera.enabled = true;

        //GraphicsManager.SetVolumetricVariables(GraphicsManager.graphicsSettings._Volumetrics_k__BackingField); // refresh volumetric settings as copying stuff from the player component causes it to no longer update from the player's perspective
        //volumetricRendering.CleanupOnReload();
    }

    /* NOTES:
     * While this does all work, I wonder if there is a more simple and effective/efficient way to do it.?
     * Il2CppSLZ.Bonelab.SaveData.GraphicsManager is the class that manages visuals, including the Volumetrics
     * In the old method for creating a VolumetricRendering component, a new VolumetricData was created.
     * The GraphicsManager class has one of these customized with the settings based on the SettingLevel the player had selected.
     * The function in charge of Seting Volumetric Variables (SetVolumetricVariables) does this:
     * 
     * VolumetricData customVolumetricRenderingSettings = GraphicsManager.CustomVolumetricRenderingSettings;
     * VolumetricRendering[] array = global::UnityEngine.Object.FindObjectsOfType<VolumetricRendering>();
     * foreach (var element in array)
     * {
     *     element.volumetricData = customVolumetricRenderingSettings;
     *     VolumetricRendering volumetricRendering = element;
     *     volumetricRendering.disable();
     *     volumetricRendering.enable();
     * }
     * 
     * This might be useful for setting up a VolumetricRendering but I am unsure of how the one on the rig starts.
     * Thinking about it, I'd assume that it gets somewhat setup as calling SetVolumetricVariables re-inits the component on the rig
     * I'll have to test further without the jank copy component functions
     * 
     * Oh nice lol, very simple solution
     */

    //public static Component CopyComponent(Component original, GameObject destination) // taken from some unity discussion thread idk https://discussions.unity.com/t/copy-a-component-at-runtime/71172/7
    // Removed as nolonger needed. was a field coppier basically


    public static void DestroyCamera() // Volumetric Rendering does not like being removed or disabled, disabling just the camera gets around this.
    {
        if (!_cineCamera) return;
        _cineCamera.enabled = false;
    }

    public static void UpdateCamera(float fov)
    {
        _cineCamFoV = fov;
        if (_cineCamera)
        {
            _cineCamera.fieldOfView = _cineCamFoV;
        }
    }
    
    #endregion
    
    #region Clone

    public static void StartCloneAvatar()
    {
        MelonCoroutines.Start(CloneAvatarCoroutine());
    }
    
    private static IEnumerator CloneAvatarCoroutine()
    {
        yield return new WaitForSeconds(AvatarCloneDelay);
        _captureAvatar = true;
    }

    [HarmonyPatch(typeof(PlayerAvatarArt), nameof(PlayerAvatarArt.UpdateAvatarHead))]
    private static class PlayerArtPatch
    {
        private static void Prefix(PlayerAvatarArt __instance)
        {
            if (!_captureAvatar) return;
            __instance.EnableHair();
            CloneAvatar();
            __instance.DisableHair();
            _captureAvatar = false;
        }
    }

    private static void CloneAvatar()
    {
        var avatarObj = Player.Avatar.gameObject;
        var clone = Object.Instantiate(avatarObj);
        clone.transform.SetPositionAndRotation(avatarObj.transform.position, avatarObj.transform.rotation);
        var grips = clone.GetComponentsInChildren<Grip>(true);
        foreach (var grip in grips)
        {
            Object.Destroy(grip);
        }
        _lastClonedAvatarRef = clone;
        ClonedAvatars.Add(clone);
        if (makeHeldObjectsKinematic)
        {
            MakeHeldObjectKinematic(Player.LeftHand);
            MakeHeldObjectKinematic(Player.RightHand);
        }
    }
    public static void MakeHeldObjectKinematic(Hand hand)
    {
        var heldgrip = Player.GetObjectInHand(hand);
        if (!heldgrip) 
            return;

        var entity = heldgrip.transform.root.GetComponentInChildren<MarrowEntity>();
        if (!entity) 
            return;

        foreach (MarrowBody body in entity.Bodies)
            body._rigidbody.isKinematic = true;
    }

    public static void DestroyLastClone()
    {
        if (!_lastClonedAvatarRef) return;
        Object.Destroy(_lastClonedAvatarRef);
        _lastClonedAvatarRef = null;
    }

    public static void DestroyAllClones()
    {
        foreach (var clone in ClonedAvatars)
        {
            Object.Destroy(clone);
        }
        ClonedAvatars.Clear();
    }
    
    #endregion
}
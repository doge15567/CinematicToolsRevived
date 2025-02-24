using System.Collections;
using HarmonyLib;
using Il2Cpp;
using Il2CppSLZ.Marrow;
using UnityEngine.Rendering.Universal;

namespace CinematicToolsRevived;

internal static class TheHandler
{
    public static float AvatarCloneDelay = 1f;
    private static Camera _cineCamera;
    private static float _cineCamFoV = 90f;
    private static GameObject _lastClonedAvatarRef;
    private static readonly List<GameObject> ClonedAvatars = [];
    private static bool _captureAvatar;
    
    #region Camera

    public static void SpawnCamera()
    {
        if (_cineCamera) Object.Destroy(_cineCamera.gameObject);
        _cineCamera = null;
        
        var head = Player.Head.transform;
        var cameraObj = new GameObject("CineTools_Camera");
        if (!cameraObj) return;
        cameraObj.SetActive(false);
        cameraObj.transform.position = head.transform.position;
        cameraObj.transform.rotation = head.transform.rotation;
        
        var camera = cameraObj.AddComponent<Camera>();
        _cineCamera = camera;
        cameraObj.AddComponent<UniversalAdditionalCameraData>().allowXRRendering = false;
        camera.stereoTargetEye = 0;
        camera.depth = 100f;
        camera.depthTextureMode = (DepthTextureMode) 1;
        camera.depthTextureMode = (DepthTextureMode) 4;
        camera.fieldOfView = _cineCamFoV;
        camera.farClipPlane = 10000f;
        camera.nearClipPlane = 0.01f;
        
        var volumetricRendering = cameraObj.AddComponent<VolumetricRendering>();
        var volumetricData = ScriptableObject.CreateInstance<VolumetricData>();
        volumetricData.near = 0.01f;
        volumetricData.far = 80f;
        volumetricData.FroxelWidthResolution = 24;
        volumetricData.FroxelHeightResolution = 24;
        volumetricData.FroxelDepthResolution = 12;
        volumetricData.ClipMapResolution = 64;
        volumetricData.ClipmapScale = 20;
        volumetricData.ClipmapScale2 = 200;
        volumetricData.ClipmapResampleThreshold = 3;
        
        volumetricRendering.volumetricData = volumetricData;
        volumetricRendering.reprojectionAmount = 0.95f;
        volumetricRendering.SliceDistributionUniformity = 0.5f;
        volumetricRendering.meanFreePath = 15f;
        volumetricRendering.StaticLightMultiplier = 1f;
        volumetricRendering.albedo = Color.white;
        volumetricRendering.tempOffset = 0f;
        volumetricRendering.FroxelBlur = VolumetricRendering.BlurType.None;
        volumetricRendering.cam = camera;
        
        cameraObj.SetActive(true);
        volumetricRendering.CleanupOnReload();
    }

    public static void DestroyCamera()
    {
        if (!_cineCamera) return;
        Object.Destroy(_cineCamera.gameObject);
        _cineCamera = null;
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
        clone.transform.position = avatarObj.transform.position;
        clone.transform.rotation = avatarObj.transform.rotation;
        _lastClonedAvatarRef = clone;
        ClonedAvatars.Add(clone);
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
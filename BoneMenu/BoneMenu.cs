using Il2CppSLZ.Marrow;

namespace CinematicToolsRevived.Menu;

internal static class BoneMenu
{
    private static bool _hairEnabled;
    
    public static void Setup()
    {
        var subCat = Page.Root.CreatePage("Cinematic Tools", Color.yellow);
        
        subCat.CreateFunction("Toggle Hair Mesh", Color.white, () =>
        {
            var avatarArt = Player.ControllerRig.GetComponent<PlayerAvatarArt>();
            if (_hairEnabled)
            {
                avatarArt.DisableHair();
                _hairEnabled = false;
            }
            else
            {
                avatarArt.EnableHair();
                _hairEnabled = true;
            }
        });
        
        var cameraCat = subCat.CreatePage("Custom Camera", Color.white);
        var cloneCat = subCat.CreatePage("Clone Avatar", Color.white);
        
        cameraCat.CreateFloat("FOV", Color.white, 90, 1, 0.0f, 180f, TheHandler.UpdateCamera);
        cameraCat.CreateFunction("Spawn Camera", Color.green, TheHandler.SpawnCamera);
        cameraCat.CreateFunction("Destroy Camera", Color.red, TheHandler.DestroyCamera);
         
        cloneCat.CreateFloat("Capture Delay", Color.white, 1f, 1f, 0.0f, 999f, (float value) =>
        {
            TheHandler.AvatarCloneDelay = value;
        });
        cloneCat.CreateBool("Freeze Held Objects On Clone", Color.white, TheHandler.makeHeldObjectsKinematic, (bool value) =>
        {
            TheHandler.makeHeldObjectsKinematic = value;
        });
        cloneCat.CreateFunction("Dupe Avatar", Color.green, TheHandler.StartCloneAvatar);
        cloneCat.CreateFunction("Delete Last Avatar", Color.yellow, TheHandler.DestroyLastClone);
        cloneCat.CreateFunction("Delete All Avatars", Color.red, TheHandler.DestroyAllClones);
    }
    
    
}
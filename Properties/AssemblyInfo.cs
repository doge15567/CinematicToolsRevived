[assembly: AssemblyTitle(CinematicToolsRevived.Main.Description)]
[assembly: AssemblyDescription(CinematicToolsRevived.Main.Description)]
[assembly: AssemblyCompany(CinematicToolsRevived.Main.Company)]
[assembly: AssemblyProduct(CinematicToolsRevived.Main.Name)]
[assembly: AssemblyCopyright("Developed by " + CinematicToolsRevived.Main.Author)]
[assembly: AssemblyTrademark(CinematicToolsRevived.Main.Company)]
[assembly: AssemblyVersion(CinematicToolsRevived.Main.Version)]
[assembly: AssemblyFileVersion(CinematicToolsRevived.Main.Version)]
[assembly:
    MelonInfo(typeof(CinematicToolsRevived.Main), CinematicToolsRevived.Main.Name,
        CinematicToolsRevived.Main.Version,
        CinematicToolsRevived.Main.Author, CinematicToolsRevived.Main.DownloadLink)]
[assembly: MelonColor(255, 255, 255, 255)]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
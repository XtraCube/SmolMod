using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace SmolMod;

[BepInAutoPlugin("dev.xtracube.smolmod")]
[BepInProcess("Among Us.exe")]
public partial class SmolModPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);
    
    public static ConfigEntry<float> ConfigScale { get; private set; }
    
    public static float ScaleMod => ConfigScale.Value;
    
    public override void Load()
    {
        ConfigScale = Config.Bind("Scale", "GlobalModifier", 1.5f, "Scale modifier used for SmolMod scaling");
        Harmony.PatchAll();
    }
}

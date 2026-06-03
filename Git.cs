using Git.Common.UI;
using Terraria.ModLoader;

namespace Git
{
    public class Git : Mod
    {
        public static ModKeybind ToggleUIKey { get; private set; }

        public override void Load()
        {
            ToggleUIKey = KeybindLoader.RegisterKeybind(this, "Toggle Schematic UI", "G");
        }

        public override void Unload()
        {
            ToggleUIKey = null;
        }
    }
}

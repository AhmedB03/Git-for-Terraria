using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Git.Common.Config
{
    // Gameplay rules — synced from the host in multiplayer.
    public class GitServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(true)]
        public bool RequirePasteResources;
    }

    // Personal settings — sharing site URL lives per-client.
    public class GitClientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue("")]
        public string SchematicServerUrl;
    }
}

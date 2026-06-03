using Git.Common.UI;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace Git.Common.Players
{
    public class GitPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Git.ToggleUIKey.JustPressed)
                UISystem.Toggle();
        }
    }
}

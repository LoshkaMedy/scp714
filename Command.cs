using CommandSystem;
using LabApi.Features.Wrappers;
using System;

namespace scp714
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RingCommand : ICommand
    {
        public string Command => "ring";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Gives SCP-714";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player admin = Player.Get(sender);

            if (arguments.Count < 1)
            {
                response = "Using: ring <id>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int playerId))
            {
                response = "Uncorrect ID";
                return false;
            }

            Player target = Player.Get(playerId);
            if (target == null)
            {
                response = "Unknown player.";
                return false;
            }

            if (target.IsInventoryFull)
            {
                response = "The inventory is full.";
                return false;
            }

            Item item = target.AddItem(ItemType.Coin);
            if (item != null)
            {
                SUB.CustomItems.Add(item.Serial);
                response = $"{target.Nickname} has got SCP-714";
                return true;
            }

            response = "Error.";
            return false;
        }
    }
}
using LobotJR.Command;
using LobotJR.Modules.Fishing.Model;
using LobotJR.Utils;
using System;
using System.Collections.Generic;

namespace LobotJR.Modules.Fishing
{
    public class FishingAdmin : ICommandModule
    {
        private readonly FishingSystem FishingSystem;

        public string Name => "Admin";

        public IEnumerable<CommandHandler> Commands { get; private set; }

        public IEnumerable<ICommandModule> SubModules => null;

        public FishingAdmin(FishingSystem fishingSystem)
        {
            FishingSystem = fishingSystem;
            Commands = new List<CommandHandler>(new CommandHandler[]
            {
                new CommandHandler("DebugTournament", DebugTournament, "debugtournament", "debug-tournament"),
                new CommandHandler("DebugCatch", DebugCatch, "debugcatch", "debug-catch")
            }); ;
        }

        public CommandResult DebugTournament(string data)
        {
            FishingSystem.Tournament.StartTournament();
            return null;
        }

        public CommandResult DebugCatch(string data)
        {
            var fisher = new Fisher();
            for (var i = 0; i < 50; i++)
            {
                FishingSystem.HookFish(fisher);
                var fish = FishingSystem.CalculateFishSizes(fisher);
                Console.WriteLine($"{fish.Fish.Name} ({Enum.GetName(typeof(FishRarity), fish.Fish.Rarity).ToPascalCase()}) caght.");
            }
            return null;
        }
    }
}

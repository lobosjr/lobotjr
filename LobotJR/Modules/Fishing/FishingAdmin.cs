using LobotJR.Command;
using LobotJR.Modules.Fishing.Model;
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
            return new CommandResult() { Processed = true };
        }

        public CommandResult DebugCatch(string data)
        {
            var fisher = new Fisher();
            var output = new List<string>();
            for (var i = 0; i < 50; i++)
            {
                FishingSystem.HookFish(fisher);
                var fish = FishingSystem.CalculateFishSizes(fisher);
                output.Add($"{fish.Fish.Name} ({fish.Fish.Rarity.Name}) caght.");
            }
            return new CommandResult() { Processed = true, Debug = output };
        }
    }
}

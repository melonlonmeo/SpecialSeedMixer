using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using ModFramework;
using Terraria.IO;

namespace SpecialSeedMixer
{
    [ApiVersion(2, 1)]
    public class SpecialSeedMixer : TerrariaPlugin
    {
        public override string Name => "SpecialSeedMixer";
        public override Version Version => new Version(1, 0);
        public override string Author => "GILX_TERRARIAVUI-DEV";
        public override string Description => "Mixes special seeds to create a new combined seed";

        public SpecialSeedMixer(Main game) : base(game) { }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("specialseed.mix", MixSeeds, "mixseeds"));
        }

        private void MixSeeds(CommandArgs args)
        {
            try
            {
                if (!args.Player.HasPermission("specialseed.mix"))
                {
                    args.Player.SendErrorMessage("You don't have permission to use this command.");
                    return;
                }

                // Set world size based on command argument
                int worldSize = args.Parameters.Count > 0 && int.TryParse(args.Parameters[0], out int size) 
                    ? Math.Clamp(size, 1, 3) 
                    : 3;

                // Tạo seed mới
                string newSeed = "combined_" + DateTime.Now.Ticks.ToString("x");
                int seedValue = Math.Abs(Guid.NewGuid().GetHashCode());

                // Đặt kích thước thế giới
                Main.maxTilesX = worldSize == 1 ? 4200 : worldSize == 2 ? 6400 : 8400;
                Main.maxTilesY = worldSize == 1 ? 1200 : worldSize == 2 ? 1800 : 2400;

                // Apply all special seed effects
                var specialSeeds = new Dictionary<string, Action>
                {
                    { "for the worthy", () => Main.ActiveWorldFileData.SetSeed("for the worthy") },
                    { "not the bees", () => Main.ActiveWorldFileData.SetSeed("not the bees") },
                    { "celebrationmk10", () => Main.ActiveWorldFileData.SetSeed("celebrationmk10") },
                    { "the constant", () => Main.ActiveWorldFileData.SetSeed("the constant") },
                    { "eureka", () => Main.ActiveWorldFileData.SetSeed("eureka") },
                    { "itsnotthesame", () => Main.ActiveWorldFileData.SetSeed("itsnotthesame") },
                    { "drunk world", () => Main.ActiveWorldFileData.SetSeed("drunk world") },
                    { "getfixedboi", () => Main.ActiveWorldFileData.SetSeed("getfixedboi") }
                };

                foreach (var seed in specialSeeds.Values)
                {
                    seed.Invoke();
                }

                // Tạo thế giới mới
                WorldGen.clearWorld();
                var progress = new Terraria.WorldBuilding.GenerationProgress();
                WorldGen.GenerateWorld(seedValue, progress);

                // Cập nhật thông tin thế giới
                Main.ActiveWorldFileData = new WorldFileData(Main.worldPathName, false)
                {
                    WorldGeneratorVersion = (ulong)seedValue
                };
                Main.ActiveWorldFileData.SetSeed(newSeed);

                // Thông báo cho người chơi
                TSPlayer.All.SendSuccessMessage($"All special seeds have been mixed and {GetWorldSizeName(worldSize)} world has been regenerated!");
                TSPlayer.All.SendInfoMessage($"New combined seed: {newSeed} (Value: {seedValue})");
                TSPlayer.All.SendInfoMessage($"World size: {GetWorldSizeName(worldSize)} ({Main.maxTilesX}x{Main.maxTilesY} tiles)");
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"Error mixing special seeds: {ex}");
                args.Player.SendErrorMessage("Failed to mix special seeds. Check server logs for details.");
            }
        }

        private string GetWorldSizeName(int size) => size switch
        {
            1 => "Small",
            2 => "Medium",
            3 => "Large",
            _ => "Unknown"
        };
    }
} 
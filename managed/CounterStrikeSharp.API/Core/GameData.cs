using System.Text.Json;
using CounterStrikeSharp.API.Core.Hosting;
using CounterStrikeSharp.API.Core.Interfaces;
using CounterStrikeSharp.API.Core.Model;
using Microsoft.Extensions.Logging;

namespace CounterStrikeSharp.API.Core
{
    public sealed class GameData : IGameData, IStartupService
    {
        private readonly string _gameDataDirectoryPath;
        private readonly Dictionary<string, GameDataEntry> _entries = [];
        private readonly ILogger<GameData> _logger;

        public GameData(IScriptHostConfiguration scriptHostConfiguration, ILogger<GameData> logger)
        {
            _logger = logger;
            _gameDataDirectoryPath = scriptHostConfiguration.GameDataPath;
        }

        public void Load()
        {
            try
            {
                // LINQ pipeline for processing so we
                // avoid intermediate object allocations
                var newEntries = Directory.EnumerateFiles(_gameDataDirectoryPath, "*.json")
                    .Select(filePath => (filePath, json: File.ReadAllText(filePath)))
                    .Select(file => (file.filePath, loadedMethods: JsonSerializer.Deserialize<Dictionary<string, LoadedGameData>>(file.json)))
                    .Where(file => file.loadedMethods is not null && file.loadedMethods.Any())
                    .SelectMany(file => file.loadedMethods!
                        .Select(kvp => new KeyValuePair<string, GameDataEntry>(
                            kvp.Key,
                            new GameDataEntry.Builder()
                                .WithSignatures(kvp.Value.Signatures!)
                                .WithOffsets(kvp.Value.Offsets!)
                                .Build()
                        ))
                    )
                    .GroupBy(e => e.Key)
                    .ToDictionary(g => g.Key, g => g.First().Value);

                // Add kvp to internal dictionary
                foreach (var (key, newEntry) in newEntries)
                {
                    if (!_entries.TryAdd(key, newEntry))
                    {
                        _logger.LogWarning("GameData Method \"{Key}\" loaded a duplicate entry from {filePath}.", key, newEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load game data");
            }
        }

        // This is mostly a passthrough with logging decorations
        // but is still useful for establishing a contract
        public GameDataEntry GetEntry(string key)
        {
            _logger.LogDebug("Getting game data entry: {Key}", key);

            if (!_entries.TryGetValue(key, out GameDataEntry? value))
            {
                throw new ArgumentException($"Game data entry not found for key: {key}");
            }

            return value;
        }
    }
}

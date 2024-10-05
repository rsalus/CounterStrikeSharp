using CounterStrikeSharp.API.Modules.Commands;
using System.Reflection;
using CounterStrikeSharp.API.Core.Commands;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Core.Interfaces;
using System.Text.Json;

namespace CounterStrikeSharp.API.Modules.Admin
{
    public partial class AdminManager : IAdminManager
    {
        private readonly ICommandManager _commandManager;
        private readonly ILogger<AdminManager> _logger;

        public AdminManager(ICommandManager commandManager, ILogger<AdminManager> logger)
        {
            _commandManager = commandManager;
            _logger = logger;
        }

        public void AddCommands()
        {
            _commandManager.RegisterCommand(new CommandDefinition("css_admins_reload", "Reloads the admin file.",
                ReloadAdminsCommand));
            _commandManager.RegisterCommand(new CommandDefinition("css_admins_list",
                "List admins and their flags.", ListAdminsCommand));
            _commandManager.RegisterCommand(new CommandDefinition("css_groups_reload",
                "Reloads the admin groups file.", ReloadAdminGroupsCommand));
            _commandManager.RegisterCommand(new CommandDefinition("css_groups_list",
                "List admin groups and their flags.", ListAdminGroupsCommand));
            _commandManager.RegisterCommand(new CommandDefinition("css_overrides_reload",
                "Reloads the admin command overrides file.", ReloadAdminOverridesCommand));
            _commandManager.RegisterCommand(new CommandDefinition("css_overrides_list",
                "List admin command overrides and their flags.", ListAdminOverridesCommand));
        }

        public void MergeGroupPermsIntoAdmins()
        {
            foreach (var (steamID, adminDef) in Admins)
            {
                AddPlayerToGroup(steamID, adminDef.Groups.ToArray());
            }
        }

        public void LoadCommandOverrides(string overridePath)
        {
            try
            {
                if (!File.Exists(overridePath))
                {
                    Console.WriteLine("Admin command overrides file not found. Skipping admin command overrides load.");
                    return;
                }

                var overridesFromFile = JsonSerializer.Deserialize<Dictionary<string, CommandData>>
                    (File.ReadAllText(overridePath), new JsonSerializerOptions() { ReadCommentHandling = JsonCommentHandling.Skip });
                if (overridesFromFile == null) { throw new FileNotFoundException(); }
                foreach (var (command, overrideDef) in overridesFromFile)
                {
                    if (CommandOverrides.ContainsKey(command))
                    {
                        CommandOverrides[command].Flags.UnionWith(overrideDef.Flags);
                    }
                    else
                    {
                        CommandOverrides.Add(command, overrideDef);
                    }
                }

                Console.WriteLine($"Loaded {CommandOverrides.Count} admin command overrides.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load admin command overrides: {ex}");
            }
        }




        [RequiresPermissions(permissions: "@css/generic")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void ReloadAdminsCommand(CCSPlayerController? player, CommandInfo command)
        {
            Admins.Clear();
            var rootDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent;

            try
            {
                LoadAdminData(Path.Combine(rootDir.FullName, "configs", "admins.json"));
                MergeGroupPermsIntoAdmins();
                _logger.LogInformation("Admin data reloaded successfully."); // Log success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload admin data."); // Log error
                // ... (Handle the exception or notify the user) ...
            }
        }

        [RequiresPermissions(permissions: "@css/generic")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void ListAdminsCommand(CCSPlayerController? player, CommandInfo command)
        {
            foreach (var (steamId, data) in Admins)
            {
                command.ReplyToCommand($"{steamId.SteamId64}, {steamId.SteamId2} - FLAGS: ");
                foreach (var domain in data.Flags.Keys)
                {
                    command.ReplyToCommand($"   Domain {domain}: {string.Join(", ", data.Flags[domain])}");
                }
            }
        }

        [RequiresPermissions(permissions: "@css/generic")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void ReloadAdminGroupsCommand(CCSPlayerController? player, CommandInfo command)
        {
            Groups.Clear();
            var rootDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent;
            LoadAdminGroups(Path.Combine(rootDir.FullName, "configs", "admin_groups.json"));
            MergeGroupPermsIntoAdmins();
        }

        [RequiresPermissions(permissions: "@css/generic")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void ListAdminGroupsCommand(CCSPlayerController? player, CommandInfo command)
        {
            foreach (var (groupName, groupDef) in Groups)
            {
                command.ReplyToCommand($"{groupName} - {string.Join(", ", groupDef.Flags)}");
            }
        }

        [RequiresPermissions(permissions: "@css/generic")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void ReloadAdminOverridesCommand(CCSPlayerController? player, CommandInfo command)
        {
            CommandOverrides.Clear();
            var rootDir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent;
            LoadCommandOverrides(Path.Combine(rootDir.FullName, "configs", "admin_overrides.json"));
        }

        [RequiresPermissions(permissions: "@css/generic")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private static void ListAdminOverridesCommand(CCSPlayerController? player, CommandInfo command)
        {
            foreach (var (commandName, commandDef) in CommandOverrides)
            {
                command.ReplyToCommand(
                    $"{commandName} (enabled: {commandDef.Enabled.ToString()}) - {string.Join(", ", commandDef.Flags)}");
            }
        }
    }
}

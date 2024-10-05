namespace CounterStrikeSharp.API.Core.Interfaces
{
    public interface IAdminManager
    {
        // AdminManager
        void AddCommands();
        void MergeGroupPermsIntoAdmins();

        // AdminCommandOverrides
        void LoadCommandOverrides(string overridePath);

        // AdminGroup
        void LoadAdminGroups(string adminGroupsPath);

        // AdminPermissions
        void LoadAdminData(string adminDataPath);
    }
}

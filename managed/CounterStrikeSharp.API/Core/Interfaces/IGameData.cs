using CounterStrikeSharp.API.Core.Model;

namespace CounterStrikeSharp.API.Core.Interfaces
{
    public interface IGameData
    {
        GameDataEntry GetEntry(string key);
    }
}

using System.Threading.Tasks;
using UnityEngine;

namespace OmmLand.Dungeon
{
    public abstract class RandomDungeonGenerator : MonoBehaviour
    {
        public class DungeonParameters
        {

        }

        public abstract Task<string> RetriveDungeonJson(DungeonParameters parameters);


        public abstract Task<Room> GetRoom(string name);

        public abstract Task<EnvironmentZone> GetEnvironmentZone(string name);

    }
}

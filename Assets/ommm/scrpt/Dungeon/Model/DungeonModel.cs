namespace OmmLand.Dungeon.Model
{
    [System.Serializable]
    public class DungeonModel
    {
        public int version = 1000;
        public RoomModel[] rooms;
        public EnvModel[] envModels;
    }
}

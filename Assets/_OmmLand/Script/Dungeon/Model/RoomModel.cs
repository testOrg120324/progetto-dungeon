using UnityEngine;

namespace OmmLand.Dungeon.Model
{
    [System.Serializable]
    public class RoomModel
    {
        public string objectName;

        /// <summary>
        /// On if the door must be open at the specific side
        /// </summary>
        public bool up;
        public bool down;
        public bool left;
        public bool right;

        public Vector3 position;
        public Vector3 eulerAngle;
        public Vector3 localScale;

        public int[] variantIndexes;
    }
}

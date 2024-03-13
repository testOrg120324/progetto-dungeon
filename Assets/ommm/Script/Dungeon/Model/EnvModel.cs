using UnityEngine;

namespace OmmLand.Dungeon.Model
{
    [System.Serializable]
    public class EnvModel
    {
        public string objectName;

        public Vector3 position;
        public Vector3 eulerAngle;
        public Vector3 localScale;

        public int[] variantIndexes;
    }
}

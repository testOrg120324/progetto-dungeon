using OmmLand.Dungeon.Model;
using UnityEngine;

namespace OmmLand.Dungeon
{
    public class Room : SpaceWithVariant
    {
        public NodeSize nodeSize;
        
        [Header("Object that rappresent the door position (mandatory)")]
        public GameObject upDoorPosition;

        public GameObject downDoorPosition;
        public GameObject leftDoorPosition;
        public GameObject rightDoorPosition;

        [Header("Object that rappresent the door, can be empty")]
        public GameObject upDoor;

        public GameObject downDoor;
        public GameObject leftDoor;
        public GameObject rightDoor;

        [Header("Object that rappresent the wall removed when the door is open")]
        public GameObject upNoDoor;

        public GameObject downNoDoor;
        public GameObject leftNoDoor;
        public GameObject rightNoDoor;

        [HideInInspector] public RoomModel roomModel;

        public void Setup(RoomModel roomModel)
        {
            this.roomModel = roomModel;
            if (upNoDoor) upNoDoor.SetActive(!roomModel.up);
            if (downNoDoor) downNoDoor.SetActive(!roomModel.down);
            if (leftNoDoor) leftNoDoor.SetActive(!roomModel.left);
            if (rightNoDoor) rightNoDoor.SetActive(!roomModel.right);

            if (upDoor) upDoor.SetActive(roomModel.up);
            if (downDoor) downDoor.SetActive(roomModel.down);
            if (leftDoor) leftDoor.SetActive(roomModel.left);
            if (rightDoor) rightDoor.SetActive(roomModel.right);

            SetupVariant(roomModel.variantIndexes);
        }
    }
}
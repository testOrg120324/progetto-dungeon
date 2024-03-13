using UnityEngine;
using static OmmLand.Dungeon.FakeRemoteDungeonGeneratorServerMultiElevation;

[System.Serializable]
public class NodeSize
{
    public int probabilityMultiplier = 1;

    public Vector2Int[] positions = new Vector2Int[] { new Vector2Int(0, 0) };
    public Vector2Int upDoorCellPosition = new Vector2Int(0, 0);
    public Vector2Int downDoorCellPosition = new Vector2Int(0, 0);
    public Vector2Int leftDoorCellPosition = new Vector2Int(0, 0);
    public Vector2Int rightDoorCellPosition = new Vector2Int(0, 0);

    public bool hasUp;
    public bool hasDown;
    public bool hasLeft;
    public bool hasRight;


    public bool isCompatible(Node node,NodeSize other)
    {
        if (hasUp && other.upDoorCellPosition != upDoorCellPosition) return false;
        if (hasDown && other.downDoorCellPosition != downDoorCellPosition) return false;
        if (hasLeft && other.leftDoorCellPosition != leftDoorCellPosition) return false;
        if (hasRight && other.rightDoorCellPosition != rightDoorCellPosition) return false;

        if (other.positions.Length != positions.Length) return false;

        //Same positions
        foreach (var pos in positions)
        {
            bool found = false;
            foreach (var otherPos in other.positions)
            {
                if (pos == otherPos) found = true;
            }
            if (!found) return false;
        }

        return true;
    }
}
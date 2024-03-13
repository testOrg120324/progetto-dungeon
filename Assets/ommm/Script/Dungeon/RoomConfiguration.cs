using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon Generator/New Room configuration",fileName ="Room Configuration")]
public class RoomConfiguration : ScriptableObject
{
    public Vector2 roomSize = new Vector2(15, 15);
}

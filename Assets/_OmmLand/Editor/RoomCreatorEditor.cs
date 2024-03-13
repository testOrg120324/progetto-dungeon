using OmmLand.Dungeon;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RoomCreatorEditor : EditorWindow
{
    private string roomName = "New Room";


    RoomConfiguration roomConfiguration;



    private float wallHeight = 3.0f;
    private float doorWidth = 2.0f;
    private float doorHeight = 3.0f;
    
    private Material wallMaterial;
    private Material doorMaterial;
    private Material groundMaterial;


    private GameObject floorPrefab;
    private GameObject doorPrefab, noDoorPrefab, halfWallPrefab,wallPrefab;

    bool[][] rows;
    bool[,] errors;

    public int sizeX,sizeY;

    [MenuItem("Menu/New Dungeon Room")]
    static void Init()
    {
        RoomCreatorEditor window = (RoomCreatorEditor)EditorWindow.GetWindow(typeof(RoomCreatorEditor));
        window.Show();
        window.sizeX = 3;
        window.sizeY = 3;
    }

    void Set(int x,int y,bool value)
    {
        rows[x][y] = value;
    }

    const int SPACE = 16;

    void OnGUI()
    {
        GUILayout.Label("Room Size");

        roomConfiguration = EditorGUILayout.ObjectField("Room configuration", roomConfiguration, typeof(RoomConfiguration),false) as RoomConfiguration;

        if (roomConfiguration!=null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Set the size in logic cell of the generated room", MessageType.Info);

            int newSizeX = EditorGUILayout.IntSlider("Width", sizeX - 2, 1, 5) + 2;
            int newSizeY = EditorGUILayout.IntSlider("Height", sizeY - 2, 1, 5) + 2;

            if (newSizeX != sizeX || newSizeY != sizeY || rows == null || newSizeX != rows.Length)
            {
                sizeX = newSizeX;
                sizeY = newSizeY;

                rows = new bool[sizeX][];
                for (int i = 0; i < rows.Length; i++) rows[i] = new bool[sizeY];

                // set default port
                Set(sizeX / 2, 0, true);
                Set(sizeX / 2, sizeY - 1, true);
                Set(0, sizeY / 2, true);
                Set(sizeX - 1, sizeY / 2, true);

                // set room size
                for (int x = 1; x < sizeX - 1; x++)
                {
                    for (int y = 1; y < sizeY - 1; y++)
                    {
                        Set(x, y, true);
                    }
                }

                errors = new bool[sizeX, sizeY];
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Draw the logic space occupated by the room", MessageType.Info);

            GUI.color = Color.green;
            GUILayout.Toggle(true, " Door");
            GUI.color = Color.white;
            GUILayout.Toggle(true, " Room zone");

            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++) errors[x, y] = false;
            string errorsText = "";


            //up/down port
            for (int x = 0; x < sizeX - 1; x++)
            {
                if (rows[x][0] && !rows[x][1])
                {
                    errors[x, 1] = true;
                    errorsText += "\n Up door is not near to a piece of room";
                }

                if (rows[x][sizeY - 1] && !rows[x][sizeY - 2])
                {
                    errors[x, sizeY - 2] = true;
                    errorsText += "\n Down door is not near to a piece of room";
                }
            }

            //right/left port
            for (int y = 0; y < sizeY - 1; y++)
            {
                if (rows[0][y] && !rows[1][y])
                {
                    errors[1, y] = true;
                    errorsText += "\n Left door is not near to a piece of room";
                }

                if (rows[sizeX - 1][y] && !rows[sizeX - 2][y])
                {
                    errors[sizeX - 2, y] = true;
                    errorsText += "\n Right door is not near to a piece of room";
                }
            }



            for (int y = 0; y < sizeY; y++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(100));


                for (int x = 0; x < sizeX; x++)
                {
                    if (x == 0 || y == 0 || x == sizeX - 1 || y == sizeY - 1)
                    {
                        bool doorEnabled = true;


                        // corner
                        if ((x == 0 && (y == 0 || y == sizeY - 1)) || (x == sizeX - 1 && (y == 0 || y == sizeY - 1))) doorEnabled = false;

                        if (!doorEnabled)
                        {
                            GUILayout.Label("", GUILayout.Width(SPACE));
                            continue;
                        };

                        if (!rows[x][y]) GUI.color = Color.green / 4;
                        else GUI.color = Color.green;

                        bool newDoor = GUILayout.Toggle(rows[x][y], "", GUILayout.Width(SPACE));
                        if (newDoor && newDoor != rows[x][y])
                        {
                            if (x == 0 || x == sizeX - 1)
                            {
                                for (int i = 0; i < sizeY; i++)
                                {
                                    Set(x, i, false);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < sizeX; i++)
                                {
                                    Set(i, y, false);
                                }
                            }
                        }
                        rows[x][y] = newDoor;
                    }
                    else
                    {
                        GUI.color = Color.white;
                        if (errors[x, y]) GUI.color = Color.red;
                        rows[x][y] = GUILayout.Toggle(rows[x][y], "", GUILayout.Width(SPACE));
                    }
                }

                GUI.color = Color.white;
                GUILayout.EndHorizontal();

            }

            if (!string.IsNullOrEmpty(errorsText))
            {
                EditorGUILayout.Space();
                GUI.enabled = false;
                GUI.color = Color.red;
                GUILayout.TextArea(errorsText);
                GUI.enabled = true;
                GUI.color = Color.white;
            }


            EditorGUILayout.Space();


            GUILayout.Label("Other Room attribute", EditorStyles.boldLabel);
            roomName = EditorGUILayout.TextField("Room Name", roomName);
            wallHeight = EditorGUILayout.FloatField("Wall Height", wallHeight);
            doorWidth = EditorGUILayout.FloatField("Door Width", doorWidth);
            doorHeight = EditorGUILayout.FloatField("Door Height", doorHeight);

            wallMaterial = (Material)EditorGUILayout.ObjectField("Wall Material", wallMaterial, typeof(Material), false);
            doorMaterial = (Material)EditorGUILayout.ObjectField("Door Material", doorMaterial, typeof(Material), false);
            groundMaterial =
                (Material)EditorGUILayout.ObjectField("Ground Material", groundMaterial, typeof(Material), false);

            floorPrefab = (GameObject)EditorGUILayout.ObjectField("Floor prefab", floorPrefab, typeof(GameObject), false);
            doorPrefab = (GameObject)EditorGUILayout.ObjectField("Door prefab", doorPrefab, typeof(GameObject), false);
            noDoorPrefab = (GameObject)EditorGUILayout.ObjectField("No Door prefab", noDoorPrefab, typeof(GameObject), false);
            halfWallPrefab = (GameObject)EditorGUILayout.ObjectField("Half Wall prefab", halfWallPrefab, typeof(GameObject), false);
            wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall prefab", wallPrefab, typeof(GameObject), false);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Room"))
            {
                CreateRoom();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (GUILayout.Button("Env space"))
            {
                CreateEnv();
            }

        }

    }


    void CreateEnv()
    {
        GameObject oldRoom = GameObject.Find(roomName);
        if (oldRoom != null) DestroyImmediate(oldRoom);

        GameObject roomObj = new GameObject(roomName);
        var envModel = roomObj.AddComponent<EnvironmentZone>();

        Vector2 roomSize = roomConfiguration.roomSize;

        List<Vector2Int> positions = new List<Vector2Int>();


        for (int x = 1; x < sizeX - 1; x++)
        {
            for (int y = 1; y < sizeY - 1; y++)
            {
                int iy = sizeY - 1 - y;

                if (!rows[x][y]) continue;


                var cellPosition = new Vector2Int(x - 1, iy - 1);
                positions.Add(cellPosition);

                if (floorPrefab!=null)
                {
                    // Instantiate floor
                    GameObject floor = Instantiate(floorPrefab);
                    floor.transform.SetParent(roomObj.transform);
                    floor.transform.localPosition = new Vector3(roomSize.x * (x - 1), 0, roomSize.y * (iy - 1));
                }
                else
                {
                    // Instantiate floor
                    GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    floor.transform.SetParent(roomObj.transform);
                    floor.transform.localPosition = new Vector3(roomSize.x * (x - 1), 0, roomSize.y * (iy - 1));
                    floor.transform.localScale = new Vector3(roomSize.x / 10, 1, roomSize.y / 10);
                    var floorRenderer = floor.GetComponent<Renderer>();
                    floorRenderer.sharedMaterial = groundMaterial;
                }
            }
        }

        envModel.positions = positions.ToArray();

        return;
    }



    private Room roomModel;

    void CreateRoom()
    {
        GameObject dungeonContainer = GameObject.Find("Dungeon");

        if (dungeonContainer != null) DestroyImmediate(dungeonContainer);

        GameObject oldRoom = GameObject.Find(roomName);
        if (oldRoom != null) DestroyImmediate(oldRoom);

        GameObject roomObj = new GameObject(roomName);
        roomModel = roomObj.AddComponent<Room>();

        Vector2 roomSize = roomConfiguration.roomSize;
        roomModel.nodeSize = new NodeSize();
        roomModel.nodeSize.hasUp = false;
        roomModel.nodeSize.hasDown = false;
        roomModel.nodeSize.hasRight = false;
        roomModel.nodeSize.hasLeft = false;

        List<Vector2Int> positions = new List<Vector2Int>();


        for (int x = 1; x < sizeX-1; x++)
        {
            for (int y = 1; y < sizeY-1; y++)
            {
                int iy = sizeY - 1 - y;

                if (!rows[x][y]) continue;


                var cellPosition = new Vector2Int(x - 1, iy - 1);
                positions.Add(cellPosition);


                // Instantiate floor
                GameObject floor = new GameObject();
                floor.name = string.Format("Floor{0}x{1}", x, y);
                floor.transform.SetParent(roomObj.transform);
                floor.transform.localPosition = new Vector3(roomSize.x * (x - 1), 0, roomSize.y * (iy - 1));
                floor.transform.localScale = new Vector3(1, 1, 1);

                // Floor
                if (floorPrefab != null)
                {
                    // Instantiate floor
                    GameObject pav = PrefabUtility.InstantiatePrefab(floorPrefab) as GameObject;
                    pav.name = string.Format("Pav{0}x{1}", x, y);
                    pav.transform.SetParent(floor.transform);
                    pav.transform.localPosition = -10 * Vector3.up;
                    pav.transform.localScale = new Vector3(15, 20, 15);
                }
                else
                {
                    GameObject pav = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pav.name = string.Format("Pav{0}x{1}", x, y);
                    pav.transform.SetParent(floor.transform);
                    pav.transform.localPosition = -10 * Vector3.up;
                    pav.transform.localScale = new Vector3(roomSize.x/10, 20, roomSize.x / 10);
                    var floorRenderer = pav.GetComponent<Renderer>();
                    floorRenderer.sharedMaterial = groundMaterial;
                }


                //up
                if (rows[x][y - 1])
                {
                    //is the updoor???
                    if (y-1==0)
                    {
                        roomModel.nodeSize.hasUp = true;
                        roomModel.nodeSize.upDoorCellPosition = cellPosition;
                        
                        
                        float wallSize = roomSize.x / 2 - doorWidth / 2;
                        if (halfWallPrefab != null)
                        {
                            CreateHalfWall(new Vector3(-roomSize.x / 2 + wallSize / 2, 0, roomSize.y / 2), Vector3.up * 90, floor);
                            CreateHalfWall(new Vector3(roomSize.x / 2 - wallSize / 2, 0, roomSize.y / 2), Vector3.up * 90, floor);
                        }
                        else
                        {
                            CreateWall(new Vector3(wallSize, wallHeight, 0.1f), new Vector3(-roomSize.x / 2 + wallSize / 2, wallHeight / 2, roomSize.y / 2), floor, wallMaterial);
                            CreateWall(new Vector3(wallSize, wallHeight, 0.1f), new Vector3(roomSize.x / 2 - wallSize / 2, wallHeight / 2, roomSize.y / 2), floor, wallMaterial);
                        }

                        if (noDoorPrefab != null)
                        {
                            (var pivot, var door, var noDoor)=CreatePrefabedDoor(new Vector3(0, 0, roomSize.y / 2 / floor.transform.localScale.z), Vector3.up * 90, floor, "up");
                            roomModel.upDoorPosition = pivot;
                            roomModel.upDoor = door;
                            roomModel.upNoDoor = noDoor;
                        }
                        else
                        {
                            roomModel.upNoDoor = CreateWall(new Vector3(doorWidth, doorHeight, 0), new Vector3(0, doorHeight / 2, roomSize.y / 2), floor, doorMaterial);
                            roomModel.upNoDoor.name = "upNoDoor";
                            roomModel.upDoorPosition = CreateDoorPivot(roomModel.upNoDoor);
                        }

                    }
                }
                else
                {
                    if (wallPrefab != null) CreateWallPrefab(new Vector3(0, 0, roomSize.y / 2), Vector3.up * 90, floor);
                    else CreateWall(new Vector3(roomSize.x, wallHeight, 0.1f), new Vector3(0, wallHeight / 2, roomSize.y / 2), floor, wallMaterial);
                }

                //down
                if (rows[x][y + 1])
                {
                    //is the downdoor???
                    if (y + 1 == sizeY-1)
                    {
                        roomModel.nodeSize.hasDown = true;
                        roomModel.nodeSize.downDoorCellPosition = cellPosition;

                        float wallSize = roomSize.x / 2 - doorWidth / 2;
                        if (halfWallPrefab!=null)
                        {
                            CreateHalfWall(new Vector3(-roomSize.x / 2 + wallSize / 2, 0, -roomSize.y / 2), -Vector3.up * 90, floor);
                            CreateHalfWall(new Vector3( roomSize.x / 2 - wallSize / 2, 0, -roomSize.y / 2), -Vector3.up * 90, floor);
                        }
                        else
                        {
                            CreateWall(new Vector3(wallSize, wallHeight, 0.1f), new Vector3(-roomSize.x / 2 + wallSize / 2, wallHeight / 2, -roomSize.y / 2), floor, wallMaterial);
                            CreateWall(new Vector3(wallSize, wallHeight, 0.1f), new Vector3(roomSize.x / 2 - wallSize / 2, wallHeight / 2, -roomSize.y / 2), floor, wallMaterial);
                        }


                        if (noDoorPrefab!=null)
                        {
                            (var pivot, var door, var noDoor) = CreatePrefabedDoor(new Vector3(0, 0, -roomSize.y / 2 / floor.transform.localScale.z), -Vector3.up * 90, floor, "down");
                            roomModel.downDoorPosition = pivot;
                            roomModel.downDoor = door;
                            roomModel.downNoDoor = noDoor;
                        }
                        else
                        {
                            roomModel.downNoDoor = CreateWall(new Vector3(doorWidth, doorHeight, 0), new Vector3(0, doorHeight / 2, -roomSize.y / 2), floor, doorMaterial);
                            roomModel.downNoDoor.name = "downNoDoor";
                            roomModel.downDoorPosition = CreateDoorPivot(roomModel.downNoDoor);
                        }

                    }
                }
                else
                {
                    if (wallPrefab != null) CreateWallPrefab(new Vector3(0, 0, -roomSize.y / 2), -Vector3.up * 90, floor);
                    else CreateWall(new Vector3(roomSize.x, wallHeight, 0.1f), new Vector3(0, wallHeight / 2, -roomSize.y / 2), floor, wallMaterial);
                }

                //left
                if (rows[x-1][y])
                {
                    //is the lefdoor???
                    if (x - 1 == 0)
                    {
                        roomModel.nodeSize.hasLeft = true;
                        roomModel.nodeSize.leftDoorCellPosition = cellPosition;

                        float wallSize = roomSize.y / 2 - doorWidth / 2;
                        if (halfWallPrefab != null)
                        {
                            CreateHalfWall(new Vector3(-roomSize.x / 2, 0, -roomSize.y / 2 + wallSize / 2), -Vector3.up * 0, floor);
                            CreateHalfWall(new Vector3(-roomSize.x / 2, 0, +roomSize.y / 2 - wallSize / 2), -Vector3.up * 0, floor);
                        }
                        else
                        {
                            CreateWall(new Vector3(0.1f, wallHeight, wallSize), new Vector3(-roomSize.x / 2, wallHeight / 2, -roomSize.y / 2 + wallSize / 2), floor, wallMaterial);
                            CreateWall(new Vector3(0.1f, wallHeight, wallSize), new Vector3(-roomSize.x / 2, wallHeight / 2, +roomSize.y / 2 - wallSize / 2), floor, wallMaterial);
                        }

                        if (noDoorPrefab != null)
                        {
                            (var pivot, var door, var noDoor) = CreatePrefabedDoor(new Vector3(-roomSize.x / 2 / floor.transform.localScale.x, 0, 0), -Vector3.up * 0, floor, "left");
                            roomModel.leftDoorPosition = pivot;
                            roomModel.leftDoor = door;
                            roomModel.leftNoDoor = noDoor;
                        }
                        else
                        {
                            roomModel.leftNoDoor = CreateWall(new Vector3(0, doorHeight, doorWidth), new Vector3(-roomSize.x / 2, doorHeight / 2, 0), floor, doorMaterial);
                            roomModel.leftNoDoor.name = "leftNoDoor";
                            roomModel.leftDoorPosition = CreateDoorPivot(roomModel.leftNoDoor);
                        }
                    }
                }
                else
                {
                    if (wallPrefab != null) CreateWallPrefab(new Vector3(-roomSize.x / 2, 0, 0), -Vector3.up * 0, floor);
                    else CreateWall(new Vector3(0.1f, wallHeight, roomSize.y), new Vector3(-roomSize.x / 2, wallHeight / 2, 0), floor, wallMaterial);
                }

                //right
                if (rows[x + 1][y])
                {
                    //is the rightdoor???
                    if (x + 1 == sizeX-1)
                    {
                        roomModel.nodeSize.hasRight = true;
                        roomModel.nodeSize.rightDoorCellPosition = cellPosition;

                        float wallSize = roomSize.y / 2 - doorWidth / 2;
                        if (halfWallPrefab != null)
                        {
                            CreateHalfWall(new Vector3(roomSize.x / 2, 0, -roomSize.y / 2 + wallSize / 2), -Vector3.up * 180, floor);
                            CreateHalfWall(new Vector3(roomSize.x / 2, 0, +roomSize.y / 2 - wallSize / 2), -Vector3.up * 180, floor);
                        }
                        else
                        {
                            CreateWall(new Vector3(0.1f, wallHeight, wallSize), new Vector3(roomSize.x / 2, wallHeight / 2, -roomSize.y / 2 + wallSize / 2), floor, wallMaterial);
                            CreateWall(new Vector3(0.1f, wallHeight, wallSize), new Vector3(roomSize.x / 2, wallHeight / 2, +roomSize.y / 2 - wallSize / 2), floor, wallMaterial);
                        }

                        if (noDoorPrefab != null)
                        {
                            (var pivot, var door, var noDoor) = CreatePrefabedDoor(new Vector3(roomSize.x / 2 / floor.transform.localScale.x, 0, 0), -Vector3.up * 180, floor, "right");
                            roomModel.rightDoorPosition = pivot;
                            roomModel.rightDoor = door;
                            roomModel.rightNoDoor = noDoor;
                        }
                        else
                        {
                            roomModel.rightNoDoor = CreateWall(new Vector3(0, doorHeight, doorWidth), new Vector3(roomSize.x / 2, doorHeight / 2, 0), floor, doorMaterial);
                            roomModel.rightNoDoor.name = "rightNoDoor";
                            roomModel.rightDoorPosition = CreateDoorPivot(roomModel.rightNoDoor);
                        }
                    }
                }
                else
                {
                    if (wallPrefab != null) CreateWallPrefab(new Vector3(roomSize.x / 2, 0, 0), -Vector3.up * 180, floor);
                    else CreateWall(new Vector3(0.1f, wallHeight, roomSize.y), new Vector3(roomSize.y / 2, wallHeight / 2, 0), floor, wallMaterial);
                }
            }
        }

        roomModel.nodeSize.positions = positions.ToArray();

        return;

        

        // Save as prefab
        string prefabPath = "Assets/Prefabs/" + roomName + ".prefab";
        // PrefabUtility.SaveAsPrefabAsset(roomObj, prefabPath);
        //  DestroyImmediate(roomObj);
    }


    (GameObject pivot, GameObject door, GameObject noDoor) CreatePrefabedDoor(Vector3 localposition, Vector3 rotation, GameObject floor, string direction)
    {
        GameObject pivot = new GameObject(direction + "Door" + "_pivot");
        pivot.transform.SetParent(floor.transform);
        pivot.transform.localPosition = localposition;
        pivot.transform.localScale = Vector3.one;

        var noDoorGO = PrefabUtility.InstantiatePrefab(noDoorPrefab, pivot.transform) as GameObject;

        noDoorGO.transform.localPosition = new Vector3(0, 0, 0);
        noDoorGO.transform.localEulerAngles = rotation;
        noDoorGO.name = direction + "NoDoor";

        GameObject doorGO=null;

        if (doorPrefab != null)
        {
            doorGO = PrefabUtility.InstantiatePrefab(doorPrefab, pivot.transform) as GameObject;
            doorGO.transform.localPosition = new Vector3(0, 0, 0);
            doorGO.transform.localEulerAngles = rotation;
            doorGO.name = direction + "Door";
        }

        return (pivot, doorGO, noDoorGO);
    }

    GameObject CreateWallPrefab(Vector3 localPosition,Vector3 rotation, GameObject floor, string name = "Wall")
    {
        GameObject wall = PrefabUtility.InstantiatePrefab(wallPrefab, floor.transform) as GameObject;
        wall.name = name;
        wall.transform.localPosition = localPosition;
        wall.transform.localEulerAngles = rotation;

        return wall;
    }

    GameObject CreateWall(Vector3 size, Vector3 localPosition, GameObject floor, Material material, string name="Wall")
    {
        if (wallPrefab!=null)
        {
            GameObject wall = PrefabUtility.InstantiatePrefab(wallPrefab,floor.transform) as GameObject;
            wall.name = name;
            wall.transform.localScale = size;
            wall.transform.position = localPosition;
            wall.transform.SetParent(floor.transform);
            var wallRenderer = wall.GetComponent<Renderer>();
            wallRenderer.sharedMaterial = material;

            return wall;
        }
        else
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.localScale = size;
            wall.transform.position = floor.transform.position + localPosition;
            wall.transform.SetParent(floor.transform);
            var wallRenderer = wall.GetComponent<Renderer>();
            wallRenderer.sharedMaterial = material;

            return wall;
        }

    }


    GameObject CreateDoorPivot(GameObject door)
    {
        GameObject pivot = new GameObject(door.name + "_pivot");
        pivot.transform.SetParent(door.transform);
        pivot.transform.localPosition = -Vector3.up * 0.5f;
        pivot.transform.SetParent(door.transform.parent);
        pivot.transform.localScale = Vector3.one;
        door.transform.SetParent(pivot.transform);
        return pivot;
    }

    GameObject CreateHalfWall(Vector3 localPosition, Vector3 rotation, GameObject floor, string name = "HalfWall")
    {
        GameObject wall = PrefabUtility.InstantiatePrefab(halfWallPrefab, floor.transform) as GameObject;
        wall.name = name;
        wall.transform.position = floor.transform.position + localPosition;
        wall.transform.SetParent(floor.transform);
        wall.transform.localEulerAngles = rotation;

        return wall;
    }

}
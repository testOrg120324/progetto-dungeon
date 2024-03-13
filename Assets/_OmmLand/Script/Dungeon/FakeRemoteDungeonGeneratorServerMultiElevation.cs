using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmmLand.Dungeon.Model;
using UnityEngine;

namespace OmmLand.Dungeon
{
    public class FakeRemoteDungeonGeneratorServerMultiElevation : RandomDungeonGenerator
    {
        public Transform parent;

        public RoomConfiguration roomConfiguration;

        public int roomBeforeBoss = 10;
        public int minRoomsForFakePath = 3;
        public int maxRoomsForFakePath = 6;

        [Range(0,3)]
        public int envOutlineSize = 2;

        public Room[] startRooms;
        public Room[] normalRooms;
        public Room[] uniqueRooms;
        public Room[] bossRooms;

        public EnvironmentZone[] envZonesLayer1;
        public EnvironmentZone[] envZonesLayer2;

        public int specifSeed = 123;

        public class Node
        {
            public Vector2Int position;

            public enum RoomType
            {
                StartNode,
                Normal,
                Boss
            };

            public RoomType roomType = RoomType.Normal;

            public Node left, right, up, down;

            public Node nextOnMainPath, prevOnMainPath;
            public int deep = 0;
            public Direction direction;

            public NodeSize size;

            public Room graphicRoom;
            public RoomModel roomModel;
            public EnvModel envModel;
        }



        public class Direction
        {
            public enum DirectionType
            {
                UP,
                DOWN,
                RIGHT,
                LEFT
            };

            public Vector2Int delta;
            public DirectionType directionType;

            public Direction(DirectionType directionType)
            {
                this.directionType = directionType;
                switch (directionType)
                {
                    case DirectionType.UP:
                        delta = new Vector2Int(0, 1);
                        break;
                    case DirectionType.DOWN:
                        delta = new Vector2Int(0, -1);
                        break;
                    case DirectionType.RIGHT:
                        delta = new Vector2Int(1, 0);
                        break;
                    case DirectionType.LEFT:
                        delta = new Vector2Int(-1, 0);
                        break;
                    default:
                        delta = new Vector2Int(0, 0);
                        break;
                }

                ;
            }
        }


        public Dictionary<Node.RoomType, List<NodeSize>> nodeSizes = new Dictionary<Node.RoomType, List<NodeSize>>();


        public override async Task<Room> GetRoom(string name)
        {
            Room[][] alls = new Room[][] { startRooms, normalRooms, uniqueRooms, bossRooms };

            foreach (var collection in alls)
            {
                foreach (var item in collection)
                {
                    if (item.gameObject.name == name)
                    {
                        var newGo = Instantiate(item.gameObject, parent);
                        newGo.gameObject.SetActive(false);
                        return newGo.GetComponent<Room>();
                    }
                }
            }

            return null;
        }

        public override async Task<EnvironmentZone> GetEnvironmentZone(string name)
        {
            foreach (var item in envZonesLayer1)
            {
                if (item.gameObject.name == name)
                {
                    var newGo = Instantiate(item.gameObject, parent);
                    newGo.gameObject.SetActive(true);
                    return newGo.GetComponent<EnvironmentZone>();
                }
            }

            foreach (var item in envZonesLayer2)
            {
                if (item.gameObject.name == name)
                {
                    var newGo = Instantiate(item.gameObject, parent);
                    newGo.gameObject.SetActive(true);
                    return newGo.GetComponent<EnvironmentZone>();
                }
            }

            return null;
        }


        System.Random random = new System.Random();

        public override async Task<string> RetriveDungeonJson(DungeonParameters parameters)
        {
            int seed = (int)System.DateTime.Now.Ticks;

            if (specifSeed != 0) seed = specifSeed;

            Debug.LogFormat("SEED:{0}", seed);

            random = new System.Random(seed);

            DungeonModel dungeon = GenerateDungeon();

            return JsonUtility.ToJson(dungeon);
        }


        void PrepareRoomSizes()
        {
            nodeSizes = new Dictionary<Node.RoomType, List<NodeSize>>();

            List<NodeSize> bossSizes = new List<NodeSize>();
            foreach (var item in bossRooms)
            {
                for (int i = 0; i < item.nodeSize.probabilityMultiplier; i++)
                    bossSizes.Add(item.nodeSize);
            }
            nodeSizes.Add(Node.RoomType.Boss, bossSizes);

            List<NodeSize> normalSizes = new List<NodeSize>();
            foreach (var item in normalRooms)
            {
                for (int i = 0; i < item.nodeSize.probabilityMultiplier; i++)
                    normalSizes.Add(item.nodeSize);
            }
            nodeSizes.Add(Node.RoomType.Normal, normalSizes);

            List<NodeSize> startSizes = new List<NodeSize>();
            foreach (var item in startRooms)
            {
                for (int i = 0; i < item.nodeSize.probabilityMultiplier; i++)
                    startSizes.Add(item.nodeSize);
            }
            nodeSizes.Add(Node.RoomType.StartNode, startSizes);
        }

        NodeSize GetRandomNodeSize(Node.RoomType roomType)
        {
            var list = nodeSizes[roomType];
            return list[random.Next(0, list.Count)];
        }

        DungeonModel GenerateDungeon()
        {

            Vector2 roomSize = new Vector2(15, 15);
            if (roomConfiguration == null)
            {
                Debug.LogErrorFormat(this.gameObject, "No room configuration specified, please setup");
            }
            else roomSize = roomConfiguration.roomSize;

            PrepareRoomSizes();            


            DungeonModel dungeon = new DungeonModel();

            Node startNode = new Node();
            startNode.roomType = Node.RoomType.StartNode;
            startNode.position = new Vector2Int(0, 0);
            startNode.size = GetRandomNodeSize(Node.RoomType.StartNode);
            startNode.deep = 0;

            List<Node> nodes = new List<Node>();
            List<Node> mainPath = new List<Node>();
            nodes.Add(startNode);
            mainPath.Add(startNode);

            // Add Main path
            AddPath(mainPath, nodes, startNode, roomBeforeBoss, true);


            // ramification from main path
            foreach (var node in mainPath)
            {
                if (node.roomType == Node.RoomType.Boss) continue;

                List<Node> fakePath = new List<Node>();
                AddPath(fakePath, nodes, node, roomBeforeBoss, false);

                //sub fake path
                foreach (var fakeNode in fakePath)
                {
                    int fakePathLenght = random.Next(minRoomsForFakePath, maxRoomsForFakePath + 1);
                    List<Node> fakeSubPath = new List<Node>();
                    AddPath(fakeSubPath, nodes, fakeNode, fakeNode.deep + fakePathLenght, false);
                }
            }

            List<Room> uniqueRoomPool = (new List<Room>());
            uniqueRoomPool.AddRange(uniqueRooms);

            Room[] randomStartRoom = (new List<Room>(startRooms)).ToArray();
            Room[] randomBossRooms = (new List<Room>(bossRooms)).ToArray();

            int uniqueProbability = -1;

            // create all room from node
            List<RoomModel> rooms = new List<RoomModel>();
            foreach (var node in nodes)
            {
                // Create a list with all normal and unique room. The unique one will be removed from the list
                var allRoom = new List<Room>(normalRooms);
                for (int i = 0; i < uniqueProbability && uniqueRoomPool.Count>0; i++)
                {
                    allRoom.AddRange(uniqueRoomPool);
                }
                Room[] randomNormalRooms = (allRoom).ToArray();
                uniqueProbability++;

                randomStartRoom.Shuffle(random);
                randomNormalRooms.Shuffle(random);
                randomBossRooms.Shuffle(random);

                RoomModel room = new RoomModel();
                room.left = node.left != null;
                room.right = node.right != null;
                room.up = node.up != null;
                room.down = node.down != null;
                room.eulerAngle = Vector3.zero;
                room.localScale = Vector3.one;
                room.objectName = "unknow";
                room.position = new Vector3(roomSize.x * node.position.x, 0, roomSize.y * node.position.y);

                // save the room model associated
                node.roomModel = room;

                Room[] collection = randomNormalRooms;
                if (node.roomType == Node.RoomType.StartNode) collection = randomStartRoom;
                if (node.roomType == Node.RoomType.Boss) collection = randomBossRooms;

                //Debug.LogFormat("Choosing room for type:{0} size:", node.roomType, JsonUtility.ToJson(node.size));
                bool roomFounded = false;
                foreach (var item in collection)
                {
                    bool isRoomValid = true;
                    if (node.left != null && !item.nodeSize.hasLeft) isRoomValid = false;
                    if (node.right != null && !item.nodeSize.hasRight) isRoomValid = false;
                    if (node.up != null && !item.nodeSize.hasUp) isRoomValid = false;
                    if (node.down != null && !item.nodeSize.hasDown) isRoomValid = false;

                    if (!node.size.isCompatible(node,item.nodeSize)) isRoomValid = false;

                    if (isRoomValid)
                    {
                        if (uniqueRoomPool.Contains(item)) uniqueRoomPool.Remove(item);

                        roomFounded = true;
                        node.graphicRoom = item;
                        room.objectName = item.gameObject.name;
                        break;
                    }
                }

                if (!roomFounded)
                {
                    Debug.LogErrorFormat("Cannot found a room for node type:{0} size:{1} node:{2}", node.roomType, JsonUtility.ToJson(node.size), JsonUtility.ToJson(node));
                }

                rooms.Add(room);
            }

            // adjust elevation with a second pass (now that we have choice all graphics room)
            foreach (var node in nodes)
            {
                // is not start room
                if (node.prevOnMainPath != null)
                {
                    Transform prevDoor = null;
                    Transform currentDoor = null;

                    try
                    {
                        //Elevate room in base of door position. The direction is considered by the prev room
                        switch (node.direction.directionType)
                        {
                            case Direction.DirectionType.UP:
                                prevDoor = node.prevOnMainPath.graphicRoom.upDoorPosition.transform;
                                currentDoor = node.graphicRoom.downDoorPosition.transform;
                                break;
                            case Direction.DirectionType.DOWN:
                                prevDoor = node.prevOnMainPath.graphicRoom.downDoorPosition.transform;
                                currentDoor = node.graphicRoom.upDoorPosition.transform;
                                break;
                            case Direction.DirectionType.RIGHT:
                                prevDoor = node.prevOnMainPath.graphicRoom.rightDoorPosition.transform;
                                currentDoor = node.graphicRoom.leftDoorPosition.transform;
                                break;
                            case Direction.DirectionType.LEFT:
                                prevDoor = node.prevOnMainPath.graphicRoom.leftDoorPosition.transform;
                                currentDoor = node.graphicRoom.rightDoorPosition.transform;
                                break;
                            default:
                                break;
                        }


                        float prevDoorElevation = prevDoor.transform.position.y - node.prevOnMainPath.graphicRoom.transform.position.y;
                        float currentDoorElevation = currentDoor.transform.position.y - node.graphicRoom.transform.position.y;

                        node.roomModel.position += Vector3.up * (node.prevOnMainPath.roomModel.position.y +
                            prevDoorElevation - currentDoorElevation);
                    }
                    catch (System.Exception e)
                    {
                        

                        Debug.LogErrorFormat(" Error working on Room {0} ", node.graphicRoom.name);
                        throw e;
                    }



                    /*
                    Debug.LogFormat("Room {0} prev Room {1} Direction:{5} prevY={2} prevDoor={3} currentDoor:{4}"
                        , node.graphicRoom.gameObject.name
                        , node.prevOnMainPath.graphicRoom.gameObject.name
                        , node.prevOnMainPath.roomModel.position.y
                        , prevDoorElevation
                        , currentDoorElevation
                        , node.direction.directionType
                        );
                    */

                }

                node.roomModel.variantIndexes = node.graphicRoom.GetRandomIndexes(random);

                // THE PREVIOUS NODE HAS THE SOME ROOM? The room has variants?
                if (node.prevOnMainPath!=null && node.prevOnMainPath.graphicRoom==node.graphicRoom && node.roomModel.variantIndexes.Length>0)
                {
                    // while the previuos room is the same in term of variants, try to make it different (try it max 4 time)
                    int maxTentative = 10;
                    while (maxTentative>0 && node.graphicRoom.Likeness(node.roomModel.variantIndexes,node.prevOnMainPath.roomModel.variantIndexes)>0.5f)
                    {
                        node.roomModel.variantIndexes = node.graphicRoom.GetRandomIndexes(random);
                        maxTentative--;
                    }
                }
            }


            dungeon.rooms = rooms.ToArray();

            PrepareEnv(dungeon, nodes, envZonesLayer1);
            PrepareEnv(dungeon, nodes, envZonesLayer2);

            return dungeon;
        }


        HashSet<Vector2Int> RandomHashSet(HashSet<Vector2Int> oldHashset)
        {
            List<Vector2Int> list = new();
            foreach (var item in oldHashset)
            {
                list.Add(item);
            }

            list.Shuffle(random);

            HashSet<Vector2Int> hashset = new HashSet<Vector2Int>();
            foreach (var item in list)
            {
                hashset.Add(item);
            }

            return hashset;
        }


        void PrepareEnv(DungeonModel dungeon, List<Node> nodes, EnvironmentZone[] envZones)
        {
            Vector2 roomSize = roomConfiguration.roomSize;

            HashSet<Vector2Int> allPoints = new HashSet<Vector2Int>();

            foreach (var node in nodes)
            {
                foreach (var nodeSubPosition in node.size.positions)
                {
                    for (int x = -envOutlineSize; x <= envOutlineSize; x++)
                    {
                        for (int y = -envOutlineSize; y <= envOutlineSize; y++)
                        {
                            //if (x == 0 && y == 0) continue; // the node subposition itself

                            Vector2Int position = node.position + nodeSubPosition + new Vector2Int(x, y);

                            if (IsPositionFree(nodes, position) && !allPoints.Contains(position))
                            {
                                // new free env point
                                allPoints.Add(position);
                            }
                        }
                    }
                }
            }

            

            List<EnvModel> models = new List<EnvModel>();

            allPoints = RandomHashSet(allPoints);

            List<EnvironmentZone> zones = new List<EnvironmentZone>();
            zones.AddRange(envZones);

            while (allPoints.Count>0)
            {
                zones.Shuffle(random);

                var position = allPoints.First();

                bool zoneGenerated = false;

                foreach (var zone in zones)
                {
                    // this zone can fit here?
                    if (IsPositionFree(nodes,position,zone.positions))
                    {
                        float y = GetMedianHeightOfZone(nodes, position, zone.positions);

                        var env = new EnvModel();
                        env.objectName = zone.name;
                        env.position = new Vector3(roomSize.x * position.x, y, roomSize.y * position.y);
                        env.eulerAngle = Vector3.zero;
                        env.localScale = Vector3.one;
                        env.variantIndexes = zone.GetRandomIndexes(random);

                        models.Add(env);

                        // Save the position occupied
                        Node node = new Node();
                        node.position = position;
                        node.size = new NodeSize();
                        node.envModel = env;
                        node.size.positions = zone.positions;
                        nodes.Add(node);

                        //Marks as done all position of the env
                        foreach (var subPosition in zone.positions)
                        {
                            var futurePosition = position + subPosition;
                            allPoints.Remove(futurePosition);
                        }
                        zoneGenerated = true;
                        break;
                    }
                }

                if (!zoneGenerated) allPoints.Remove(position); // Save point, if no zone is valid, remove it


            }

            if (dungeon.envModels == null) dungeon.envModels = new EnvModel[0];

            models.AddRange(dungeon.envModels.ToList());

            dungeon.envModels = models.ToArray();
        }


        bool AddPath(List<Node> currentPath, List<Node> nodes, Node from, int maxDeep, bool isMainPath)
        {
            Direction[] allDirections = new Direction[]
            {
                new Direction(Direction.DirectionType.UP), new Direction(Direction.DirectionType.DOWN),
                new Direction(Direction.DirectionType.LEFT), new Direction(Direction.DirectionType.RIGHT)
            };


            allDirections.Shuffle(random);

            int nextDeep = from.deep + 1;
            Node.RoomType roomType = isMainPath && (nextDeep >= maxDeep) ? Node.RoomType.Boss : Node.RoomType.Normal;

            //Debug.LogFormat("Depth path:{0}", nextDeep);

            foreach (var direction in allDirections)
            {
                List<NodeSize> randomSizes = new List<NodeSize>();
                randomSizes.AddRange(nodeSizes[roomType]);
                randomSizes.Shuffle(random);

                foreach (var size in randomSizes)
                {
                    // size compatible with directions
                    if (direction.directionType == Direction.DirectionType.UP && (!from.size.hasUp || !size.hasDown)) continue;
                    if (direction.directionType == Direction.DirectionType.DOWN && (!from.size.hasDown || !size.hasUp)) continue;
                    if (direction.directionType == Direction.DirectionType.LEFT && (!from.size.hasLeft || !size.hasRight)) continue;
                    if (direction.directionType == Direction.DirectionType.RIGHT && (!from.size.hasRight || !size.hasLeft)) continue;

                    // Calculate the next base position of the node, starting from the "from node" position and the position of their door
                    var nextNodePosition = from.position;
                    if (direction.directionType == Direction.DirectionType.UP) nextNodePosition += from.size.upDoorCellPosition;
                    if (direction.directionType == Direction.DirectionType.DOWN) nextNodePosition += from.size.downDoorCellPosition;
                    if (direction.directionType == Direction.DirectionType.LEFT) nextNodePosition += from.size.leftDoorCellPosition;
                    if (direction.directionType == Direction.DirectionType.RIGHT) nextNodePosition += from.size.rightDoorCellPosition;

                    // we need to translate the room in base of the next door position
                    if (direction.directionType == Direction.DirectionType.UP) nextNodePosition -= size.downDoorCellPosition;
                    if (direction.directionType == Direction.DirectionType.DOWN) nextNodePosition -= size.upDoorCellPosition;
                    if (direction.directionType == Direction.DirectionType.LEFT) nextNodePosition -= size.rightDoorCellPosition;
                    if (direction.directionType == Direction.DirectionType.RIGHT) nextNodePosition -= size.leftDoorCellPosition;


                    nextNodePosition += direction.delta;

                    // is the space in the next direction free?
                    if (!IsPositionFree(nodes, nextNodePosition,size)) continue;

                    // is free... create the next node
                    Node nextNode = new Node();
                    nextNode.position = nextNodePosition;
                    nextNode.deep = nextDeep;
                    nextNode.roomType = roomType;
                    nextNode.direction = direction;
                    nextNode.size = size;

                    nodes.Add(nextNode);
                    currentPath.Add(nextNode);

                    // Where are at the end of the path or the path is successfly created on the next?
                    if ((nextDeep >= maxDeep) || AddPath(currentPath, nodes, nextNode, maxDeep, isMainPath))
                    {
                        // the node is valid, let configure it

                        // if the first time this node has a path?
                        if (from.nextOnMainPath == null) from.nextOnMainPath = nextNode;

                        // register the back path
                        nextNode.prevOnMainPath = from;
                        switch (direction.directionType)
                        {
                            case Direction.DirectionType.UP:
                                from.up = nextNode;
                                nextNode.down = from;
                                break;
                            case Direction.DirectionType.DOWN:
                                from.down = nextNode;
                                nextNode.up = from;
                                break;
                            case Direction.DirectionType.RIGHT:
                                from.right = nextNode;
                                nextNode.left = from;
                                break;
                            case Direction.DirectionType.LEFT:
                                from.left = nextNode;
                                nextNode.right = from;
                                break;
                            default:
                                break;
                        }


                        //Debug.LogFormat("Added Node type {0} from Node:{1} result:{2}", nextNode.roomType, JsonUtility.ToJson(from), JsonUtility.ToJson(nextNode));


                        return true;
                    }
                    else
                    {
                        // fail to path, remove the node
                        nodes.Remove(nextNode);
                        currentPath.Remove(nextNode);
                    }
                }
            }

            return !isMainPath;
        }

        bool IsPositionFree(List<Node> nodes, Vector2Int pos, NodeSize size)
        {
            // for all nodes allocated
            foreach (var item in nodes)
            {
                // for each position occupied from a node
                foreach (var cell in item.size.positions)
                {
                    // for each position that will be occupied from the new room
                    foreach (var futureCell in size.positions)
                    {
                        if ((item.position + cell) == (pos+futureCell)) return false;
                    }
                }
            }

            return true;
        }

        bool IsPositionFree(List<Node> nodes, Vector2Int pos, Vector2Int[] futurePositions)
        {
            // for all nodes allocated
            foreach (var item in nodes)
            {
                // for each position occupied from a node
                foreach (var cell in item.size.positions)
                {
                    // for each position that will be occupied from the new room
                    foreach (var futureCell in futurePositions)
                    {
                        if ((item.position + cell) == (pos + futureCell)) return false;
                    }
                }
            }

            return true;
        }

        float GetMedianHeightOfZone(List<Node> nodes, Vector2Int pos, Vector2Int[] futurePositions)
        {
            List<Node> aroundNodes = new List<Node>();

            // for all nodes allocated
            foreach (var item in nodes)
            {
                if (aroundNodes.Contains(item)) continue;

                // for each position occupied from a node
                foreach (var cell in item.size.positions)
                {
                    if (aroundNodes.Contains(item)) continue;

                    Vector2Int nodePosition = item.position + cell;

                    // for each position that will be occupied from the new room
                    foreach (var futureCell in futurePositions)
                    {
                        if (aroundNodes.Contains(item)) continue;

                        Vector2Int futureCellPosition = futureCell + pos;

                        for (int x = -1; x <= 1; x++)
                        {
                            if (aroundNodes.Contains(item)) continue;

                            for (int y = -1; y <= 1; y++)
                            {
                                Vector2Int searchPosition = futureCellPosition + new Vector2Int(x, y);

                                if (searchPosition == nodePosition)
                                {
                                    aroundNodes.Add(item);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (aroundNodes.Count == 0) return 0;

            // Median point fo around point
            float elevation = 0;
            foreach (var node in aroundNodes)
            {
                if (node.roomModel != null) elevation += node.roomModel.position.y;
                else elevation += node.envModel.position.y;
            }
            return elevation / aroundNodes.Count;
        }


        bool IsPositionFree(List<Node> nodes, Vector2Int pos)
        {
            // for all nodes allocated
            foreach (var item in nodes)
            {
                // for each position occupied from a node
                foreach (var cell in item.size.positions)
                {
                    if ((item.position + cell) == (pos)) return false;
                }
            }

            return true;
        }

    }
}
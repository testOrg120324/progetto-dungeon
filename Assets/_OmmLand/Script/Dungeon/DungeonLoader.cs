using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using OmmLand.Dungeon.Model;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace OmmLand.Dungeon
{
    public class DungeonLoader : MonoBehaviour
    {
        public RandomDungeonGenerator generator;
        public NavMeshSurface surface;

        [ContextMenu("CreateDungeon")]
        public async UniTask<bool> Init()
        {
            Time.timeScale = 0;

            //Retrive the json
            Task<string> dungeonTask = generator.RetriveDungeonJson(null);
            await UniTask.WaitUntil(() => dungeonTask.IsCompleted);
            Debug.LogFormat("Load Json:{0}", dungeonTask.Result);

            //convert to the model
            DungeonModel dungeon = JsonUtility.FromJson<DungeonModel>(dungeonTask.Result);


            GameObject dungeonContainer=GameObject.Find("Dungeon");

            if (dungeonContainer != null) DestroyImmediate(dungeonContainer);

            dungeonContainer = new GameObject("Dungeon");
            dungeonContainer.transform.position = Vector3.zero;
            dungeonContainer.transform.localRotation = Quaternion.identity;
            dungeonContainer.transform.localScale = Vector3.one;



            //Load rooms
            foreach (var roomModel in dungeon.rooms)
            {
                Task<Room> roomTask = generator.GetRoom(roomModel.objectName);
                await UniTask.WaitUntil(() => roomTask.IsCompleted);

                Room room = roomTask.Result;
                // todo   // if(room.name.Contains("StartRoom"))
                // todo   //     monsterSpawnAreas.Add(room.GetComponentInChildren<GeneratedSpawnArea>());


                room.transform.SetParent(dungeonContainer.transform);
                room.transform.position = roomModel.position;
                room.transform.eulerAngles = roomModel.eulerAngle;
                room.transform.localScale = roomModel.localScale;
                room.gameObject.SetActive(true);
                room.Setup(roomModel);

                if (roomModel.objectName.Contains("Boss"))
                {
                    //this.ExitPortal.gameObject.transform.position += room.transform.position;
                    //this.ExitPortal.gameObject.transform.eulerAngles = room.transform.eulerAngles;
                    // todo    // ExitPortal.transform.position += room.transform.position;
                    // todo   // ExitPortal.transform.eulerAngles = room.transform.eulerAngles;
                }
            }


            //Load rooms
            foreach (var envModel in dungeon.envModels)
            {
                Task<EnvironmentZone> envTask = generator.GetEnvironmentZone(envModel.objectName);
                await UniTask.WaitUntil(() => envTask.IsCompleted);

                EnvironmentZone env = envTask.Result;

                env.transform.SetParent(dungeonContainer.transform);
                env.transform.position = envModel.position;
                env.transform.eulerAngles = envModel.eulerAngle;
                env.transform.localScale = envModel.localScale;

                env.SetupVariant(envModel.variantIndexes);
            }


            Debug.Log("start build nav mesh data");

            // get the data for the surface
            var data = InitializeBakeData(surface);

            NavMesh.RemoveAllNavMeshData();

            // start building the navmesh
            var async = surface.UpdateNavMesh(data);

            // wait until the navmesh has finished baking
            await async;

            Debug.Log("finished");

            // you need to save the baked data back into the surface
            surface.navMeshData = data;

            // call AddData() to finalize it
            surface.AddData();
            Time.timeScale = 1;
            await UniTask.WaitForSeconds(1f);
            return true;
            // StartCoroutine(SearchAndMovePlayersRoutine());
            /*foreach (var spawnArea in monsterSpawnAreas)
        {
            spawnArea.ManualSpawn = true;
            spawnArea.SpawnAll();
        }*/
        }

        IEnumerator SearchAndMovePlayersRoutine()
        {
            yield return new WaitForSeconds(1f);
            // todo  GameObject player = FindObjectOfType<PlayerCharacterEntity>().gameObject;
            // todo  player.transform.position = Vector3.zero + Vector3.up;
            /*var players = BaseGameNetworkManager.Singleton.GetPlayers();
        Debug.Log(players + " --- players");
        
        foreach(var player in players)
        {
            Debug.Log("player: " + player.Manager.gameObject.name);
            player.Manager.gameObject.transform.position = Vector3.zero + Vector3.up;
        }*/
            // todo   loadingCanvas.SetActive(true);
        }

        // creates the navmesh data
        static NavMeshData InitializeBakeData(NavMeshSurface surface)
        {
            var emptySources = new List<NavMeshBuildSource>();
            var emptyBounds = new Bounds();

            return UnityEngine.AI.NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds,
                surface.transform.position, surface.transform.rotation);
        }
    }
}
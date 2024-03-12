using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;

namespace OmmLand.Dungeon
{
    [CustomEditor(typeof(DungeonLoader))]
    public class DungeonLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
            {
                var loader = target as DungeonLoader;
                if (loader != null) loader.Init().Forget();
            }
        }
    }

}


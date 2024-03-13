using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SpaceVariantBehavioursSimpleList))]
public class SpaceVariantBehavioursSimpleListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var spaceVariant = target as SpaceVariantBehavioursSimpleList;

        EditorGUILayout.Space();

        int childs = spaceVariant.transform.childCount;

        if (spaceVariant.variantsList!=null)
        {
            foreach (var item in spaceVariant.variantsList)
            {

                EditorGUILayout.BeginHorizontal();

                item.onGameobject = EditorGUILayout.ObjectField(item.onGameobject, typeof(GameObject), true) as GameObject;
                item.probability = EditorGUILayout.IntSlider(item.probability, 1, 200);

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();


        if (spaceVariant.variantsList==null || spaceVariant.variantsList.Length!= childs || GUILayout.Button("Autopopolate"))
        {
            var variantsList = new SpaceVariant[childs];
            for (int i = 0; i < childs; i++)
            {
                variantsList[i] = new SpaceVariant();
                variantsList[i].probability = 1;
                variantsList[i].onGameobject = spaceVariant.transform.GetChild(i).gameObject;

                foreach (var item in spaceVariant.variantsList)
                {
                    if (item.onGameobject== variantsList[i].onGameobject)
                    {
                        variantsList[i].probability = item.probability;
                    }
                }
            }
            spaceVariant.variantsList = variantsList;
            EditorUtility.SetDirty(spaceVariant);
        }


    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceVariantBehavioursSimpleList : SpaceVariantBehaviours
{
    public SpaceVariant[] variantsList;

    public override int GetRandomIndex(System.Random random)
    {
        //Weigthed probability
        int totalSize = 0;
        for (int i = 0; i < variantsList.Length; i++)
        {
            totalSize += variantsList[i].probability;
        }

        // -------------------
        // 1 1 1
        // 0 - 3
        // Case 0; i=0;cursor=1;cursor>0->return 0
        // Case 1; i=0;cursor=1;cursor>1->no;i=2;cursor=2;cursor>1->return 1
        // case 2; i=0;cursor=1;cursor>2->no;i=2;cursor=2;cursor>2->no;i=3;cursor=3;cursor>2-> return 2;

        int result = random.Next(0, totalSize);
        int cursor = 0;
        for (int i = 0; i < variantsList.Length; i++)
        {
            cursor += variantsList[i].probability;

            if (cursor > result) return i;
        }

        return variantsList.Length-1;
    }

    public override void SetupWithIndex(int index)
    {
        for (int i = 0; i < variantsList.Length; i++) variantsList[i].onGameobject.SetActive(false);
        if (index>=0 && index<variantsList.Length) variantsList[index].onGameobject.SetActive(true);

        for (int i = 0; i < variantsList.Length; i++)
        {
            if (variantsList[i]!=null && !variantsList[i].onGameobject.activeSelf)
            {
                if (Application.isPlaying) Destroy(variantsList[i].onGameobject);
                else DestroyImmediate(variantsList[i].onGameobject);
            }
        }
    }

}

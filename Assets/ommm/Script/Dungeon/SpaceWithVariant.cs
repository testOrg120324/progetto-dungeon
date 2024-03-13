using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceWithVariant : MonoBehaviour
{
    public SpaceVariantBehaviours[] variantBehaviours;


    public void SetupVariant(int[] indexes)
    {

        for (int i=0;i<indexes.Length && i< variantBehaviours.Length;i++)
        {
            variantBehaviours[i].SetupWithIndex(indexes[i]);
        }

    }

    public int[] GetRandomIndexes(System.Random random)
    {
        int[] indexes = new int[variantBehaviours.Length];
        for (int i = 0; i < indexes.Length; i++)
        {
            indexes[i] = variantBehaviours[i].GetRandomIndex(random);
        }

        return indexes;
    }


    public float Likeness(int[] indexes, int[] otherIndex)
    {
        if (indexes.Length != otherIndex.Length) return 0;

        float someElement = 0;
        for (int i = 0; i < otherIndex.Length; i++)
        {
            if (indexes[i] == otherIndex[1]) someElement++;
        }

        return ((float)(someElement))/indexes.Length;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpaceVariantBehaviours : MonoBehaviour
{
    /// <summary>
    /// Return a random index for this variants
    /// </summary>
    /// <returns></returns>
    public abstract int GetRandomIndex(System.Random random);

    /// <summary>
    /// Setup the variant with a specific index
    /// </summary>
    /// <param name="index"></param>
    public abstract void SetupWithIndex(int index);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolablePrefab : MonoBehaviour
{
    public int ModelVariantCount = 1;

    protected abstract string LookupPrefabAddress(int modelVariantIndex);

    public string GetPrefabAddress(int modelVariantIndex)
    {
        if (modelVariantIndex > 0 && modelVariantIndex < ModelVariantCount))
        {
            return LookupPrefabAddress(modelVariantIndex);
        }
        else
        {
            throw new System.Exception("Looking up Model Variant Prefab, but given model variant index was out of bounds! Index: " + modelVariantIndex);
        }
    }
}
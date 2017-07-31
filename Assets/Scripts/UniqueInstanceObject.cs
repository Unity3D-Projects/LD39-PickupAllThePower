using System.Collections.Generic;
using UnityEngine;

public class UniqueInstanceObject : MonoBehaviour
{
    private static readonly HashSet<string> ObjectNames =
        new HashSet<string>();

    private void Awake()
    {
        if (!ObjectNames.Add(gameObject.name))
        {
            DestroyImmediate(gameObject);
        }
    }
}

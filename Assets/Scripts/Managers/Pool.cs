using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;

public class Pool : MonoBehaviour
{
    public static Pool IN;
    public static Dictionary<string, GameObject> PrefabDictionary = new();
    public static HashSet<GameObject> SpawnedItems = new();
    [SerializeField] private GameObject[] prefabs;

    private void Awake()
    {
        foreach (var prefab in prefabs)
        {
            PrefabDictionary.Add(prefab.name, prefab);
        }
    }

    public static GameObject Get(string inName)
    {
        if (PrefabDictionary.TryGetValue(inName, out GameObject prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogError($"Prefab with name {inName} not found.");
            return null;
        }
    }

    public static T Spawn<T>(string inName, Transform inParent, Vector3 inPosition, Quaternion inRotation) where T : class
    {
        var prefab = Get(inName);
        if (prefab)
        {
            var instance = LeanPool.Spawn(prefab, inPosition, inRotation, inParent);
            if (instance)
            {
                SpawnedItems.Add(instance);

                if (instance.TryGetComponent<T>(out var component))
                    return component;
                else
                {
                    Debug.LogError($"Prefab with name {inName} does not have component of type {typeof(T)}.");
                    Destroy(instance);
                    return null;
                }
            }
            else
            {
                Debug.LogError($"LeanPool failed to instantiate Prefab with name {inName} of type {typeof(T)}.");
                Destroy(instance);
                return null;
            }
        }
        
        return null;
    }

    public static T Spawn<T>(string inName) where T : class
    {
        var prefab = Get(inName);
        if (prefab)
        {
            var instance = LeanPool.Spawn(prefab);
            if (instance)
            {
                SpawnedItems.Add(instance);
                
                if (instance.TryGetComponent<T>(out var component))
                    return component;
                else
                {
                    Debug.LogError($"Prefab with name {inName} does not have component of type {typeof(T)}.");
                    Destroy(instance);
                    return null;
                }
            }
            else
            {
                Debug.LogError($"Prefab with name {inName} does not have component of type {typeof(T)}.");
                Destroy(instance);
                return null;
            }
        }

        return null;
    }

    public static bool Despawn(GameObject inGameObject, bool inShouldDestroyIfLeanPoolDespawnFails = true)
    {
        if(inGameObject == null)
        {
            Debug.LogError("GameObject to despawn is null.");
            return false;
        }

        if (SpawnedItems.Contains(inGameObject))
        {
            LeanPool.Despawn(inGameObject);
            SpawnedItems.Remove(inGameObject);
            return true;
        }
        else
        {
            if(inShouldDestroyIfLeanPoolDespawnFails)
                Destroy(inGameObject);

            return false;
        }
    }

    private static T Get<T>(string inName) where T : class
    {
        if (PrefabDictionary.TryGetValue(inName, out GameObject prefab))
        {
            if (prefab.TryGetComponent<T>(out T component))
            {
                return component;
            }
            else
            {
                Debug.LogError($"Prefab with name {inName} does not have component of type {typeof(T)}.");
                return null;
            }
        }
        else
        {
            Debug.LogError($"Prefab with name {inName} not found.");
            return null;
        }
    }
}
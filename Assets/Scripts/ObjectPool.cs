using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Prefab to Pool")]
    [SerializeField] private GameObject prefab;

    [Header("Pool Settings")]
    [SerializeField] private int initialSize = 20;
    [SerializeField] private bool expandable = true;

    private readonly List<GameObject> pool = new List<GameObject>();

    private void Awake()
    {
        // Создаём стартовый пул
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    /// <summary>
    /// Берёт объект из пула
    /// </summary>
    public GameObject GetObject(Vector3? nullablePosition = null, Quaternion? nullableRotation = null)
    {
        Vector3 position = nullablePosition ?? Vector3.zero;
        Quaternion rotation = nullableRotation ?? Quaternion.identity;

        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
                obj.SetActive(true);
                return obj;
            }
        }

        if (expandable)
        {
            return CreateNewObject(position, rotation);
        }

        return null; // если пул заполнен и не расширяется
    }

    /// <summary>
    /// Возвращает объект в пул
    /// </summary>
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    public int PoolsCount()
    {
        return pool.Count;
    }

    /// <summary>
    /// true - активные, false - неактивные
    /// </summary>
    public int PoolsCount(bool aoi)
    {
        int activePools = 0;

        for(int i = 0;i < pool.Count;i++)
        {
            if (pool[i].activeSelf) activePools++;
        }

        return aoi ? activePools : pool.Count - activePools;
    }

    /// <summary>
    /// Создание нового объекта в пул
    /// </summary>
    private GameObject CreateNewObject(Vector3 position = default, Quaternion rotation = default)
    {
        var obj = Instantiate(prefab, position, rotation, transform);
        obj.SetActive(false);
        pool.Add(obj);
        return obj;
    }
}

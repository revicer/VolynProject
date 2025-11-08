// File: ObjectSpawner.cs
using UnityEngine;
using UnityEngine.AI; // For NavMesh

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public Collider spawnArea; // Сюда мы перетащим нашу "Зону Спавна"

    // Вызывается в самом начале
    void Start()
    {
        MoveToRandomLocation();
    }

    // Этот метод будет "телепортировать" объект
    public void MoveToRandomLocation()
    {
        if (spawnArea == null)
        {
            Debug.LogError("Spawn Area is not set!", this);
            return;
        }

        Bounds bounds = spawnArea.bounds;

        // 1. Находим случайную точку ВНУТРИ "Зоны Спавна"
        Vector3 randomPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            transform.position.y, // Оставляем Y как есть
            Random.Range(bounds.min.z, bounds.max.z)
        );

        // 2. Находим БЛИЖАЙШУЮ точку на NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 5.0f, NavMesh.AllAreas))
        {
            // 3. Телепортируем объект в эту точку
            transform.position = hit.position;
        }
        else
        {
            // Если не нашли точку, просто остаемся на месте
            Debug.LogWarning("Could not find NavMesh point near " + randomPoint, this);
        }
    }
}
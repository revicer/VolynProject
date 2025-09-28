using DefaultNamespace;
using UnityEngine;
using System.Collections.Concurrent;
using NUnit.Framework;
using System.Collections.Generic;

public class SpawnLocatorService : MonoBehaviour
{
    
    public List<Transform> spawnPoint;
        
    public Vector3 FindLocalPosition(EntityType entityType)
    {
        if (entityType == EntityType.Zubr)
        {
            var currentSpawnPoint = spawnPoint[Random.Range(0, spawnPoint.Count)];
            return new Vector3(Random.Range(0, 100), 0, Random.Range(0, 100));
        }
        if (entityType == EntityType.Fox)
        {
            var currentSpawnPoint = spawnPoint[Random.Range(0, spawnPoint.Count)];
            return new Vector3(Random.Range(50, 150), 0, Random.Range(100, 150));
        }
        if (entityType == EntityType.Rat)
        {
            var currentSpawnPoint = spawnPoint[Random.Range(0, spawnPoint.Count)];
            return new Vector3(Random.Range(0, 300), 0, Random.Range(0, 300));
        }
        if (entityType == EntityType.Rabbit)
        {
            var currentSpawnPoint = spawnPoint[Random.Range(0, spawnPoint.Count)];
            return new Vector3(Random.Range(0, 200), 0, Random.Range(0, 200));
        }
        return Vector3.zero;    
    }
    
}

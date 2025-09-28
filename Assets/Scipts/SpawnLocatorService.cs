using DefaultNamespace;
using UnityEngine;
using System.Collections.Concurrent;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace DefaultNamespace
{
    public class SpawnLocatorService : MonoBehaviour
    {
        public List<SpawnPoint> spawnPoints;

        public GameObject SpawnEntity(EntityType entityType)
        {
            var candidates = spawnPoints.Where(sp => sp.entityType == entityType).ToList();

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"Нет спавнпоинтов для {entityType}");
                return null;
            }

            var chosenPoint = candidates[Random.Range(0, candidates.Count)];
            return chosenPoint.Spawn();
        }
    }
}

using UnityEngine;

namespace DefaultNamespace
{
    public class SpawnPoint : MonoBehaviour
    {
        public EntityType entityType;

        [Header("Animal Prefab")]
        public GameObject prefab;

        [Header("X Offset Ranges")]
        public float minX = 0f;
        public float maxX = 0f;

        [Header("Z Offset Ranges")]
        public float minZ = 0f;
        public float maxZ = 0f;

        public Vector3 GetRandomPosition()
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

            return transform.position + new Vector3(x, 0, z);
        }

        // Method for spawning a creature
        public GameObject Spawn()
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab for {entityType} didn't set!");
                return null;
            }

            return GameObject.Instantiate(prefab, GetRandomPosition(), Quaternion.identity);
        }
    }
}

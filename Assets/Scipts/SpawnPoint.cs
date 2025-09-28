using UnityEngine;

namespace DefaultNamespace
{
    public class SpawnPoint : MonoBehaviour
    {
        public EntityType entityType;

        [Header("������ ���������")]
        public GameObject prefab;

        [Header("��������� �������� �� X")]
        public float minX = 0f;
        public float maxX = 0f;

        [Header("��������� �������� �� Z")]
        public float minZ = 0f;
        public float maxZ = 0f;

        public Vector3 GetRandomPosition()
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

            return transform.position + new Vector3(x, 0, z);
        }

        // ����� ��� ������ ��������
        public GameObject Spawn()
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab ��� {entityType} �� ��������!");
                return null;
            }

            return GameObject.Instantiate(prefab, GetRandomPosition(), Quaternion.identity);
        }
    }
}

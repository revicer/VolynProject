using UnityEngine;

namespace DefaultNamespace
{
    public abstract class Animal : MonoBehaviour
    {
        [SerializeField] protected EntityType entityType;
        public EntityType EntityType => entityType;

        public virtual void OnSpawn()
        {
            Debug.Log($"{entityType} spawned at {transform.position}");
        }

        public abstract void Speak();
    }
}

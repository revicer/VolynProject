using UnityEngine;
using UnityEngine.AI;

namespace DefaultNamespace
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class Animal : MonoBehaviour
    {
        [SerializeField] protected EntityType entityType;
        protected NavMeshAgent agent;

        [Header("Movement Settings")]
        public float wanderRadius = 10f;
        public float idleTime = 2f; 

        private float timer;

        protected virtual void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            timer = idleTime;
        }

        protected virtual void Update()
        {
            Wander();
        }

        private void Wander()
        {
            timer += Time.deltaTime;

            if (timer >= idleTime)
            {
                Vector3 newPos = GetRandomNavmeshPosition();
                agent.SetDestination(newPos);
                timer = 0;
            }
        }

        private Vector3 GetRandomNavmeshPosition()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return transform.position;
        }

        public virtual void OnSpawn()
        {
            Debug.Log($"{entityType} spawned at {transform.position}");
        }

        public abstract void Speak();
    }
}

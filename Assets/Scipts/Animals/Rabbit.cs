using UnityEngine;

namespace DefaultNamespace
{
    public class Rabbit : Animal
    {
        private void Awake()
        {
            entityType = EntityType.Rabbit;
        }
        protected override void Start()
        {
            base.Start();
            agent.speed = 4f;
            wanderRadius = 17f;
        }
        public override void Speak()
        {
            Debug.Log("Rabbit says: *carrot carrot*");
        }
    }
}

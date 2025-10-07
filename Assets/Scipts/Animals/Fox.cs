using UnityEngine;

namespace DefaultNamespace
{
    public class Fox : Animal
    {
        private void Awake()
        {
            entityType = EntityType.Fox;
        }

        protected override void Start()
        {
            base.Start();
            agent.speed = 5f;
            wanderRadius = 15f;
        }

        public override void Speak()
        {
            Debug.Log("Fox says: What does the fox say?");
        }
    }
}

using UnityEngine;

namespace DefaultNamespace
{
    public class Zubr : Animal
    {
        private void Awake()
        {
            entityType = EntityType.Zubr;
        }
        protected override void Start()
        {
            base.Start();
            agent.speed = 2f;
            wanderRadius = 7f;
        }
        public override void Speak()
        {
            Debug.Log("Zubr says: *snort snort*");
        }
    }
}

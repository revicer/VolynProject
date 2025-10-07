using UnityEngine;

namespace DefaultNamespace
{
    public class Zubr : Animal
    {
        private void Awake()
        {
            entityType = EntityType.Zubr;
        }

        public override void Speak()
        {
            Debug.Log("Zubr says: *snort snort*");
        }
    }
}

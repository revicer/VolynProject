using UnityEngine;

namespace DefaultNamespace
{
    public class Fox : Animal
    {
        private void Awake()
        {
            entityType = EntityType.Fox;
        }

        public override void Speak()
        {
            Debug.Log("Fox says: *What does the fox say?*");
        }
    }
}

using DefaultNamespace;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ButtonSpawn : MonoBehaviour
    {
        public SpawnLocatorService spawnService; // сюда перетаскиваешь сервис из инспектора

        public void SpawnZubr()
        {
            spawnService.SpawnEntity(EntityType.Zubr);
        }

        public void SpawnFox()
        {
            spawnService.SpawnEntity(EntityType.Fox);
        }

        public void SpawnRat()
        {
            spawnService.SpawnEntity(EntityType.Rat);
        }

        public void SpawnRabbit()
        {
            spawnService.SpawnEntity(EntityType.Rabbit);
        }
    }
}
using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace DefaultNamespace
{
    [RequireComponent(typeof(AnimalStats))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class RabbitAgent : Agent
    {
        [Header("Component References")]
        private AnimalStats stats;
        private NavMeshAgent navAgent;

        [Header("Movement")]
        public float moveSpeed = 7f;

        private Vector3 startPosition; // Для сброса позиции
        private Animator animator;

        // Вызывается ОДИН раз при инициализации
        public override void Initialize()
        {
            stats = GetComponent<AnimalStats>();
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            navAgent.speed = moveSpeed;
            startPosition = transform.position;
        }

        // Вызывается в НАЧАЛЕ каждого "эпизода" (новой "жизни" зайца)
        public override void OnEpisodeBegin()
        {
            // Сбрасываем статы зайца
            stats.currentHealth = stats.maxHealth;
            stats.currentHunger = 50f;
            stats.currentThirst = 50f;
            stats.isDead = false;

            // Сбрасываем позицию (в случайную точку, чтобы он не "запомнил" одно место)
            Vector3 randomPos = GetRandomPointOnNavMesh(startPosition, 20f);
            transform.position = randomPos;
            navAgent.ResetPath();
        }

        // СБОР НАБЛЮДЕНИЙ ("Что я вижу/чувствую?")
        // Это входные данные для нейросети
        public override void CollectObservations(VectorSensor sensor)
        {
            // 1. Наблюдения за "внутренним состоянием" (Нормализуем данные от 0 до 1)
            sensor.AddObservation(stats.currentHunger / stats.maxHunger);
            sensor.AddObservation(stats.currentThirst / stats.maxThirst);

            // 2. Наблюдения за "внешним миром" (скорость)
            sensor.AddObservation(navAgent.velocity.magnitude / moveSpeed);

            // 3. "Зрение" - мы добавим отдельным компонентом (RayPerceptionSensor),
            // он добавит свои наблюдения автоматически.
        }

        // ОБРАБОТКА ДЕЙСТВИЙ ("Что мне делать?")
        // Это выходные данные из нейросети
        public override void OnActionReceived(ActionBuffers actions)
        {
            // Мы будем использовать "непрерывные" (Continuous) действия
            // actions.ContinuousActions[0] -> Движение по X (от -1 до +1)
            // actions.ContinuousActions[1] -> Движение по Z (от -1 до +1)

            Vector3 moveDirection = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);

            // Перемещаем агента
            Vector3 targetPosition = transform.position + moveDirection.normalized * 2f;

            // Используем NavMesh, чтобы он не ходил сквозь стены
            navAgent.SetDestination(targetPosition);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                navAgent.SetDestination(hit.position);
            }

            // --- Система Вознаграждений (Rewards) ---

            // 1. Маленький отрицательный штраф за существование
            // (Стимулирует делать что-то быстро, а не стоять на месте)
            AddReward(-0.005f);
        }

        // РУЧНОЕ УПРАВЛЕНИЕ (Для отладки)
        // Позволяет вам "быть" зайцем, используя WASD
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Debug.Log($"HEURISTIC CALLED! H: {horizontal}, V: {vertical}"); // Test log for moving

            ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
            continuousActions[0] = horizontal; // A/D
            continuousActions[1] = vertical;   // W/S
        }

        // --- Взаимодействие (Когда дошли до цели) ---
        // Эта часть остается почти такой же, как в FSM,
        // но мы добавляем Вознаграждения (Rewards)

        void OnTriggerEnter(Collider other) // <-- "other" "живет" ЗДЕСЬ
        {
            if (stats.isDead) return;

            // --- ЭТО НАША НОВАЯ ЛОГИКА ---
            if (other.gameObject.layer == LayerMask.NameToLayer("Food"))
            {
                // 1. Получаем "спавнер" этого куста
                ObjectSpawner foodSpawner = other.GetComponent<ObjectSpawner>();

                // 2. "Говорим" ЭТОМУ кусту "переместиться"
                if (foodSpawner != null)
                {
                    foodSpawner.MoveToRandomLocation();
                }

                // 3. Получаем награду
                if (animator != null) animator.SetTrigger("Eat"); // "animator" "виден" здесь
                AddReward(2.0f); // (Используем нашу "усиленную" награду)
                stats.Eat(30f);
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                // 1. Получаем "спавнер" этой воды
                ObjectSpawner waterSpawner = other.GetComponent<ObjectSpawner>();

                // 2. "Говорим" ЭТОЙ воде "переместиться"
                if (waterSpawner != null)
                {
                    waterSpawner.MoveToRandomLocation();
                }

                // 3. Получаем награду
                AddReward(2.0f); // (Используем нашу "усиленную" награду)
                stats.Drink(40f);
            }
            // --- КОНЕЦ НОВОЙ ЛОГИКИ ---

            if (other.gameObject.CompareTag("Enemy"))
            {
                stats.TakeDamage(100f);
            }
        }

        // --- Вспомогательная функция (из FSM) ---
        Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return center; // Возвращаем центр, если не нашли точку
        }
    }
}
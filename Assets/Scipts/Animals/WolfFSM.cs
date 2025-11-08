// File: WolfFSM.cs (v2 - Now with Thirst!)
using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent

// Define the possible states
public enum WolfState
{
    WANDERING,    // Moving aimlessly
    SEEKING_WATER, // <-- НОВОЕ: Looking for water
    HUNTING,      // Actively looking for prey
    CHASING       // Chasing a specific target
}

[RequireComponent(typeof(AnimalStats))]
[RequireComponent(typeof(NavMeshAgent))]
public class WolfFSM : MonoBehaviour
{
    [Header("FSM (Brain)")]
    public WolfState currentState = WolfState.WANDERING;

    [Header("Component References")]
    private AnimalStats stats;
    private NavMeshAgent agent;
    private Animator animator; // For animations

    [Header("Vision Parameters")]
    public float sightRadius = 30f;
    public string preyTag = "Rabbit";

    [Tooltip("LayerMask for water sources")]
    public LayerMask waterLayer; // <-- НОВОЕ
                                 // ---------------------------------------------

    private Transform currentTarget = null; // Target (the rabbit)

    [Header("Wandering Parameters")]
    public float wanderRadius = 25f;
    private float wanderTimer = 0f;
    public float wanderInterval = 5f;

    void Start()
    {
        stats = GetComponent<AnimalStats>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        wanderTimer = wanderInterval;

        stats.hungerRate = 0.0f;
        stats.thirstRate = 0.0f; // Жажда растет!
        stats.hungerThreshold = 1.0f;
        stats.thirstThreshold = 60.0f; // <-- НОВОЕ: Порог жажды
    }

    void Update()
    {
        if (stats.isDead)
        {
            if (agent.isOnNavMesh) agent.isStopped = true; // Correct way to stop
            return;
        }

        EvaluateState();
        ActOnState();

        if (animator != null)
        {
            animator.SetBool("IsMoving", agent.velocity.magnitude > 0.1f);
        }
    }

    void EvaluateState()
    {
        // --- НОВЫЙ БЛОК: Приоритет №1 - Вода ---
        if (stats.currentThirst > stats.thirstThreshold)
        {
            currentState = WolfState.SEEKING_WATER;
            currentTarget = null; // Stop chasing prey if thirsty
            return;
        }
        // --------------------------------------

        // (Логика Погони осталась)
        if (currentTarget != null)
        {
            if (Vector3.Distance(transform.position, currentTarget.position) > sightRadius * 1.5f)
            {
                currentTarget = null;
                currentState = WolfState.WANDERING;
            }
            else
            {
                currentState = WolfState.CHASING;
            }
            return;
        }

        // --- ИЗМЕНЕНО: Приоритет №2 - Голод ---
        if (stats.currentHunger > stats.hungerThreshold)
        {
            currentState = WolfState.HUNTING;
            return;
        }
        // --------------------------------------

        // Default: Wander
        currentState = WolfState.WANDERING;
    }

    void ActOnState()
    {
        switch (currentState)
        {
            case WolfState.WANDERING:
                Wander();
                break;
            // --- НОВЫЙ БЛОК ---
            case WolfState.SEEKING_WATER:
                SeekResource(waterLayer);
                break;
            // ------------------
            case WolfState.HUNTING:
                Hunt();
                break;
            case WolfState.CHASING:
                Chase(currentTarget);
                break;
        }
    }

    // --- Behavior Logic ---

    void Wander()
    {
        if (agent.remainingDistance < 0.5f || wanderTimer <= 0)
        {
            Vector3 randomPoint = GetRandomPointOnNavMesh(transform.position, wanderRadius);
            if (randomPoint != Vector3.zero) agent.SetDestination(randomPoint);
            wanderTimer = wanderInterval;
        }
        wanderTimer -= Time.deltaTime;
    }

    void Hunt()
    {
        currentTarget = FindNearestTargetWithTag(preyTag);

        if (currentTarget == null)
        {
            // Не вижу Зайца? Продолжаю бродить в поисках.
            Wander();
        }
        else
        {
            // Вижу Зайца!
            currentState = WolfState.CHASING;
        }
    }

    void Chase(Transform target)
    {
        agent.SetDestination(target.position);
    }

    // --- НОВЫЙ МЕТОД (Скопирован у "глупого" зайца) ---
    void SeekResource(LayerMask resourceLayer)
    {
        Transform resourceTarget = FindNearestTarget(resourceLayer);
        if (resourceTarget == null)
        {
            // Can't see the resource, wander until we find it
            Wander();
        }
        else
        {
            agent.SetDestination(resourceTarget.position);
        }
    }
    // ------------------------------------------------

    // --- Interaction (When catching prey) ---
    void OnTriggerEnter(Collider other)
    {
        if (stats.isDead) return;

        // Поймали Зайца
        if (other.CompareTag(preyTag))
        {
            Debug.Log("Wolf caught the Rabbit!");
            stats.Eat(60f);
            if (animator != null) animator.SetTrigger("Eat");

            AnimalStats rabbitStats = other.GetComponent<AnimalStats>();
            if (rabbitStats != null) rabbitStats.TakeDamage(100f);

            currentTarget = null;
            currentState = WolfState.WANDERING;
        }

        // --- НОВЫЙ БЛОК: Пьем Воду ---
        if (currentState == WolfState.SEEKING_WATER && other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            stats.Drink(60f); // Drink a lot
            // (Не нужно запускать "Eat" trigger, если нет анимации "Drink")
        }
        // -----------------------------

        // --- (Опционально) Волк ест ягоды, если ОЧЕНЬ голоден ---
        if (currentState == WolfState.HUNTING && other.gameObject.layer == LayerMask.NameToLayer("Food"))
        {
            // (Можно раскомментировать, если нет зайцев, а волк умирает)
            // stats.Eat(20f); // Eat berries as a last resort
            // if (animator != null) animator.SetTrigger("Eat");
        }
    }

    // --- Helper Functions ---

    // (Этот метод теперь ищет и Еду/Воду по СЛОЮ)
    Transform FindNearestTarget(LayerMask layer)
    {
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, sightRadius, layer);
        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var targetCollider in targetsInView)
        {
            float distance = Vector3.Distance(transform.position, targetCollider.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = targetCollider.transform;
            }
        }
        return closestTarget;
    }

    // (Этот метод ищет Зайца по ТЕГУ)
    Transform FindNearestTargetWithTag(string tag)
    {
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, sightRadius);
        Transform closestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var targetCollider in targetsInView)
        {
            if (targetCollider.CompareTag(tag))
            {
                float distance = Vector3.Distance(transform.position, targetCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = targetCollider.transform;
                }
            }
        }
        return closestTarget;
    }

    Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return Vector3.zero;
    }
}
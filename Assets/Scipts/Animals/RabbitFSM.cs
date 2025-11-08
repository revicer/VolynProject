// File: RabbitFSM.cs
using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent

// Defines the possible states the animal can be in
public enum AnimalState
{
    WANDERING,    // Moving aimlessly
    SEEKING_FOOD, // Looking for food
    SEEKING_WATER, // Looking for water
    FLEEING       // Running from a predator
}

[RequireComponent(typeof(AnimalStats))]
[RequireComponent(typeof(NavMeshAgent))]
public class RabbitFSM : MonoBehaviour
{
    [Header("FSM (Brain)")]
    public AnimalState currentState = AnimalState.WANDERING;

    [Header("Component References")]
    private AnimalStats stats;
    private NavMeshAgent agent;

    [Header("Vision Parameters")]
    public float sightRadius = 20f;
    public LayerMask foodLayer;
    public LayerMask waterLayer;
    public LayerMask enemyLayer;

    [Header("Wandering Parameters")]
    public float wanderRadius = 15f;
    private float wanderTimer = 0f;
    public float wanderInterval = 5f; // Choose a new point every 5 seconds

    private Transform currentTarget = null; // Target (food, water)
    private Transform enemyTarget = null; // Target (enemy)

    void Start()
    {
        stats = GetComponent<AnimalStats>();
        agent = GetComponent<NavMeshAgent>();
        wanderTimer = wanderInterval; // Force finding a new point immediately
    }

    void Update()
    {
        if (stats.isDead)
        {
            agent.Stop(true); // Stop moving if dead
            return;
        }

        // 1. EVALUATE - Decide which state we should be in
        EvaluateState();

        // 2. ACT - Execute the logic for the current state
        ActOnState();
    }

    void EvaluateState()
    {
        // Check for enemies within sight radius
        enemyTarget = FindNearestTarget(enemyLayer);

        // Priority 1: Flee!
        if (enemyTarget != null)
        {
            currentState = AnimalState.FLEEING;
            return;
        }

        // Priority 2: Drink
        if (stats.currentThirst > stats.thirstThreshold)
        {
            currentState = AnimalState.SEEKING_WATER;
            return;
        }

        // Priority 3: Eat
        if (stats.currentHunger > stats.hungerThreshold)
        {
            currentState = AnimalState.SEEKING_FOOD;
            return;
        }

        // Default: Wander
        currentState = AnimalState.WANDERING;
    }

    void ActOnState()
    {
        // Execute behavior based on the current state
        switch (currentState)
        {
            case AnimalState.WANDERING:
                Wander();
                break;
            case AnimalState.SEEKING_FOOD:
                Seek(foodLayer);
                break;
            case AnimalState.SEEKING_WATER:
                Seek(waterLayer);
                break;
            case AnimalState.FLEEING:
                Flee(enemyTarget);
                break;
        }
    }

    // --- Behavior Logic ---

    void Wander()
    {
        // Pick a new random point if we've reached the destination or timer runs out
        if (agent.remainingDistance < 0.5f || wanderTimer <= 0)
        {
            Vector3 randomPoint = GetRandomPointOnNavMesh(transform.position, wanderRadius);
            if (randomPoint != Vector3.zero)
            {
                agent.SetDestination(randomPoint);
            }
            wanderTimer = wanderInterval;
        }
        wanderTimer -= Time.deltaTime;
    }

    void Seek(LayerMask targetLayer)
    {
        // Find the closest target (food or water)
        currentTarget = FindNearestTarget(targetLayer);

        if (currentTarget == null)
        {
            // If we can't see food/water, just wander hoping to find some
            currentState = AnimalState.WANDERING;
        }
        else
        {
            // Move towards the target
            agent.SetDestination(currentTarget.position);
        }
    }

    void Flee(Transform target)
    {
        // Calculate a vector pointing away from the enemy
        Vector3 fleeDirection = transform.position - target.position;
        Vector3 fleePoint = transform.position + fleeDirection.normalized * (sightRadius / 2); // Flee half a sight radius away

        // Find the nearest valid point on the NavMesh in that direction
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePoint, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // If no valid flee point, just wander (better than standing still)
            Wander();
        }
    }

    // --- Interaction (When reaching a target) ---

    void OnTriggerEnter(Collider other)
    {
        if (stats.isDead) return;

        // If we are seeking food and collide with a food object
        if (currentState == AnimalState.SEEKING_FOOD && other.gameObject.layer == LayerMask.NameToLayer("Food"))
        {
            stats.Eat(30f); // Restore 30 hunger
            // Optionally, destroy the food source: Destroy(other.gameObject);
        }

        // If we are seeking water and collide with a water object
        if (currentState == AnimalState.SEEKING_WATER && other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            stats.Drink(40f); // Restore 40 thirst
        }
    }

    public void GetEatenAndRespawn()
    {
        Debug.Log("Rabbit was eaten, respawning...");

        // 1. Reset stats
        if (stats == null) stats = GetComponent<AnimalStats>();
        stats.currentHealth = stats.maxHealth;
        stats.currentHunger = 50f;
        stats.currentThirst = 50f;
        stats.isDead = false; // "Оживляем" его

        // 2. Teleport to a new random location
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // (Мы "хардкодим" центр спавна '0,0,0' и радиус '20' -
        // измените, если ваша карта в других координатах)
        Vector3 randomPos = GetRandomPointOnNavMesh(Vector3.zero, 20f);

        if (randomPos != Vector3.zero)
        {
            // Сбрасываем "путь" и "телепортируем"
            agent.Warp(randomPos);
        }

        currentState = AnimalState.WANDERING; // Возвращаемся к "брожению"
    }

    // --- Helper Functions ---

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

    Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return Vector3.zero; // Failed to find a point
    }
}
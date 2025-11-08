using UnityEngine;
using UnityEngine.Events; // Used for events like OnDeath
using Unity.MLAgents;
using UnityEngine.AI;

public class AnimalStats : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Needs")]
    public float maxHunger = 100f;
    public float currentHunger = 50f; // Start at 50
    public float hungerRate = 0.1f; // Hunger points per second

    public float maxThirst = 100f;
    public float currentThirst = 50f;
    public float thirstRate = 0.2f; // Thirst increases faster than hunger

    [Header("Thresholds")]
    public float hungerThreshold = 60f; // When to start seeking food
    public float thirstThreshold = 60f; // When to start seeking water

    public bool isDead = false;

    // An event that fires on death (useful for other scripts)
    public UnityEvent OnDeath;
    private Agent agent;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<Agent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return; // Stop processing if dead

        // 1. Increase hunger and thirst over time
        currentHunger += hungerRate * Time.deltaTime;
        currentThirst += thirstRate * Time.deltaTime;

        // Clamp values to their maximum
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);

        // 2. Start taking damage if needs are critical
        if (currentHunger >= maxHunger)
        {
            TakeDamage(1f * Time.deltaTime); // Damage from starvation
        }
        if (currentThirst >= maxThirst)
        {
            TakeDamage(2f * Time.deltaTime); // Damage from dehydration (faster)
        }
    }

    // --- Public Methods for Interaction ---

    public void Eat(float amount)
    {
        currentHunger -= amount;
        Debug.Log(gameObject.name + " ate. Hunger: " + currentHunger);
    }

    public void Drink(float amount)
    {
        currentThirst -= amount;
        Debug.Log(gameObject.name + " drank. Thirst: " + currentThirst);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        OnDeath.Invoke();
        Debug.Log(gameObject.name + " died.");

        if (animator != null)
        {
            animator.SetTrigger("Died");
        }

        // --- ВОТ ИСПРАВЛЕНИЕ ---

        // Проверяем, является ли это животное ML-Агентом
        if (agent != null)
        {
            // Это "Умный" Заяц (ML-Agent)
            agent.SetReward(-2.0f); // Штраф за смерть
            agent.EndEpisode();     // Это вызовет OnEpisodeBegin() для "перерождения"
        }
        else
        {
            // Это "Глупый" Волк (FSM)
            // Он не агент, так что просто отключаем его "мозг" и движение.
            // (Он просто останется "мертвым" на сцене)
            if (GetComponent<WolfFSM>() != null)
                GetComponent<WolfFSM>().enabled = false;

            if (GetComponent<NavMeshAgent>() != null)
                GetComponent<NavMeshAgent>().isStopped = true;

            // (Позже мы можем добавить сюда логику "удаления" тела)
            // Destroy(gameObject, 20f); 
        }

        // --- КОНЕЦ ИСПРАВЛЕНИЯ ---
    }
}
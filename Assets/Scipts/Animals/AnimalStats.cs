using UnityEngine;
using UnityEngine.Events; // Used for events like OnDeath
using Unity.MLAgents;

public class AnimalStats : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Needs")]
    public float maxHunger = 100f;
    public float currentHunger = 50f; // Start at 50
    public float hungerRate = 0.5f; // Hunger points per second

    public float maxThirst = 100f;
    public float currentThirst = 50f;
    public float thirstRate = 0.8f; // Thirst increases faster than hunger

    [Header("Thresholds")]
    public float hungerThreshold = 60f; // When to start seeking food
    public float thirstThreshold = 60f; // When to start seeking water

    public bool isDead = false;

    // An event that fires on death (useful for other scripts)
    public UnityEvent OnDeath;
    private Agent agent;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<Agent>();
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
        OnDeath.Invoke(); // Notify other scripts that this animal died
        Debug.Log(gameObject.name + " died.");

        agent.SetReward(-5.0f); // Big penalty for death
        agent.EndEpisode(); // End this "round" of learning

        //Destroy(gameObject, 5f); // Remove the body after 5 seconds
    }
}
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Health Bus References")]
    public GameObject bus1; // Drag your first bus sprite here
    public GameObject bus2; // Drag your second bus sprite here
    public GameObject bus3; // Drag your third bus sprite here
    
    [Header("Health Settings")]
    public int maxHealth = 3;
    
    private int currentHealth;
    private GameObject[] healthBuses;
    
    void Start()
    {
        // Store references to all health buses in order: bus3, bus2, bus1
        // This means bus3 (index 0) will disappear first, then bus2 (index 1), then bus1 (index 2)
        healthBuses = new GameObject[] { bus3, bus2, bus1 };
        
        // Start with full health
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        UpdateHealthDisplay();
        
        // Check if game over
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }
    
    void UpdateHealthDisplay()
    {
        // Show/hide buses based on current health (order: bus3, bus2, bus1)
        for (int i = 0; i < healthBuses.Length; i++)
        {
            if (healthBuses[i] != null)
            {
                // Hide buses from right to left (bus3 first, then bus2, then bus1)
                // Array order: [0] = bus3, [1] = bus2, [2] = bus1
                // When health = 3: show buses 0,1,2 (all visible)
                // When health = 2: show buses 1,2 (bus3 hidden)
                // When health = 1: show bus 2 (bus2, bus3 hidden)
                // When health = 0: show none (all hidden)
                healthBuses[i].SetActive(i >= (maxHealth - currentHealth));
            }
        }
    }
    
    void GameOver()
    {
        Debug.Log("Game Over! All health buses are gone!");
        // You can add game over logic here
        // For example, find the player bus and destroy it
        GameObject playerBus = GameObject.FindGameObjectWithTag("Player");
        if (playerBus != null)
        {
            playerBus.SetActive(false);
        }
    }
    
    // Method to reset health (useful for restarting the game)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }
}


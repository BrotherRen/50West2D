using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Settings")]
    public int score = 0;
    public TextMeshProUGUI scoreText;
    
    [Header("Life Reward Settings")]
    public int pointsPerLife = 125;
    public HealthManager healthManager;
    
    private int lastLifeRewardScore = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        // Verify setup
        if (healthManager == null)
        {
            Debug.LogWarning("[ScoreManager] HealthManager reference is missing! Please assign it in the Inspector.");
        }
        else
        {
            Debug.Log("[ScoreManager] HealthManager connected successfully!");
        }
        Debug.Log($"[ScoreManager] Life rewards set to every {pointsPerLife} points");
    }

    public void AddPoints(int amount)
    {
        score += amount;
        UpdateUI();
        CheckForLifeReward();
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
    
    void CheckForLifeReward()
    {
        // Check if player has earned enough points for a life reward
        if (score >= lastLifeRewardScore + pointsPerLife)
        {
            lastLifeRewardScore += pointsPerLife;
            
            Debug.Log($"[ScoreManager] Life reward earned at {score} points! (Next reward at {lastLifeRewardScore + pointsPerLife})");
            
            if (healthManager != null)
            {
                healthManager.RewardLife();
                Debug.Log("[ScoreManager] Called HealthManager.RewardLife()");
            }
            else
            {
                Debug.LogError("[ScoreManager] Cannot reward life - HealthManager reference is NULL!");
            }
        }
    }
}

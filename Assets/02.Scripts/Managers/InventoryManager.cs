using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // different by stage
	public List<GameObject> scores = new List<GameObject>(); // list of scores to collect
    public Dictionary<GameObject, bool> _scoreMap = new Dictionary<GameObject, bool>(); // map of scores and their collection status
    
    public GameObject inventoryIconUI;
    public TMPro.TextMeshProUGUI scoreText;
    
    [SerializeField]
    private int _collectedScores = 0;
    
	void Start()
    {

    }

    public void OnSceneLoaded()
    {
        inventoryIconUI = GameObject.Find("InventoryIcon");
        if (inventoryIconUI)
        {
            scoreText = inventoryIconUI.GetComponentInChildren<TextMeshProUGUI>();
        }
        _collectedScores = 0;
        DoorController doorController = FindObjectOfType<DoorController>();
        if (doorController == null)
        {
            Debug.LogWarning("DoorController is not found in the scene. Ignore this message if it's not a game scene.");
            return;
        }
        scores = doorController.scores;
        if (scores == null || scores.Count == 0)
        {
            Debug.LogWarning("Scores list is empty or not initialized!");
            return;
        }

        foreach (var score in scores)
        {
            if (score != null)
            {
                _scoreMap[score] = false;
                Debug.Log("Score: " + score.name);
            }
            else
            {
                Debug.LogWarning("Score is null in the scores list!");
            }
        }

        Invoke("UpdateInventoryUI", 0.1f);
    }
    
    public void UpdateInventoryUI()
    {
        // update inventory UI text
        if (scoreText != null)
        {
            scoreText.text = $"{_collectedScores}/{scores.Count}";

            scoreText.color = HasAllScores() ? Color.yellow : Color.white;
        }
    }

    public void AddScore(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("Attempted to add a null score object!");
            return;
        }

        if (!_scoreMap.ContainsKey(target))
        {
            Debug.LogWarning($"Score '{target.name}' is not in the list!");
            return;
        }

        Debug.Assert(target.tag == "Score", "Target must have the tag 'Score'");
        
        if (!_scoreMap[target]) // 중복 추가 방지
        {
            _scoreMap[target] = true;
            _collectedScores++;
            UpdateInventoryUI();
        }
    }
    
    
    public bool HasAllScores()
    {
        return _collectedScores == scores.Count;
    }
}
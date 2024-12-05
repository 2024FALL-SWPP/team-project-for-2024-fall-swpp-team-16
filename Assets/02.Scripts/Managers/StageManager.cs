using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public int totalStages = 10; // Set this to the total number of stages
    public int currentStage = 0; // Set this to the current stage number
    
    private string filePath;
    private bool[] accomplishedStages;
    private string fileName = "progress.txt"; // file to save the stage progress data
    
    public void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log("progress file path: " + filePath);
        LoadStages();
    }

    private void LoadStages()
    {
        accomplishedStages = new bool[totalStages];

        if (File.Exists(filePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (int.TryParse(line, out int stage) && stage > 0 && stage <= totalStages)
                    {
                        accomplishedStages[stage - 1] = true;
                    }
                }    
            }
            catch (IOException e)
            {
                Debug.LogError("Error reading progress file: " + e.Message);
            }
        } else
        {
            Debug.Log("No progress file found. Creating a new one.");
            SaveStages();
        }
    }
    
    public void SetCurrentStage(int stage)
    {
        currentStage = stage;
    }
    
    public void CompleteCurrentStage()
    {
        CompleteStage(currentStage);
    }
    
    public void CompleteStage(int stage)
    {
        if (stage > 0 && stage <= totalStages && !accomplishedStages[stage - 1])
        {
            accomplishedStages[stage - 1] = true;
            SaveStages();
        }
    }

    private void SaveStages()
    {
        try
        {
            File.WriteAllLines(filePath, accomplishedStages
                .Select((accomplished, index) => accomplished ? (index + 1).ToString() : null)
                .Where(stage => stage != null)
                .ToArray());    
        } 
        catch (IOException e)
        {
            Debug.LogError("Error writing progress file: " + e.Message);
        }
    }

    public bool IsStageAccomplished(int stage)
    {
        return stage > 0 && stage <= totalStages && accomplishedStages[stage - 1];
    }

    public bool[] GetStagesStatus()
    {
        return accomplishedStages;
    }
}
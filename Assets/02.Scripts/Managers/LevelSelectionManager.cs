using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Collections.Shaders.CircleTransition;

public class LevelSelectionManager : MonoBehaviour
{
    public List<LevelSelectionNoteController> levelNotes; // Assign all notes (UI Images) in the Inspector
    public TMP_Text stageNumberText;
     public float fadeDuration = 0.1f; // Duration of the fade
    public Image fadeImage;         // Reference to the UI Image for fading


    private int currentSelectedIndex = 0;

    // Sliding animation properties
    public float slideDuration = 1.5f;
    private RectTransform rectTransform;  // To move the UI panel
    private Vector2 offScreenPosition;
    private Vector2 onScreenPosition;

    void Awake()
    {
        // Get RectTransform and define positions
      /*  rectTransform = GetComponent<RectTransform>();
        onScreenPosition = rectTransform.anchoredPosition; // Current position
        offScreenPosition = new Vector2(onScreenPosition.x, -Screen.height); // Off-screen position

        // Start off-screen
        rectTransform.anchoredPosition = offScreenPosition; */
    }

    void Start()
    {
         StartCoroutine(FadeOut());
        // Ensure the first unlocked stage is selected at the start
        currentSelectedIndex = FindLastUnlockedIndex();
       // StartCoroutine(SlideIn()); // Slide in the level selection screen
        UpdateSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveSelection(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveSelection(-1);
        }

        if (Input.GetKeyDown(KeyCode.Return)) // Confirm selection
        {
            ConfirmSelection();
        }
    }
     IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); // Fade alpha to 0
            fadeImage.color = color;
            yield return null;
        }

        // Ensure it’s fully transparent at the end
        color.a = 0f;
        fadeImage.color = color;

        // Optionally deactivate the fade image
        fadeImage.gameObject.SetActive(false);
    }

    void MoveSelection(int direction)
    {
        // Deselect the current note
        levelNotes[currentSelectedIndex].isSelected = false;

        // Move to the next unlocked stage
        do
        {
            currentSelectedIndex += direction;

            // Wrap around the list if needed
            if (currentSelectedIndex < 0)
            {
                currentSelectedIndex = levelNotes.Count - 1;
            }
            else if (currentSelectedIndex >= levelNotes.Count)
            {
                currentSelectedIndex = 0;
            }
        } while (levelNotes[currentSelectedIndex].isLocked()); // Skip locked stages

        // Update the selection
        UpdateSelection();
    }

    void UpdateSelection()
    {
        for (int i = 0; i < levelNotes.Count; i++)
        {
            levelNotes[i].isSelected = (i == currentSelectedIndex);
            levelNotes[i].UpdateNoteAppearance();
        }
        stageNumberText.text = $"- {levelNotes[currentSelectedIndex].stageNumber} -";
    }

    void ConfirmSelection()
    {
        LevelSelectionNoteController selectedNote = levelNotes[currentSelectedIndex];

        if (selectedNote.isLocked())
        {
            Debug.LogWarning($"Stage {selectedNote.stageNumber} is locked and cannot be selected.");
            return; // Prevent loading locked stages
        }

        Debug.Log($"Loading Stage {selectedNote.stageNumber}...");

        // Load the scene corresponding to the selected stage
        string sceneName = $"Test Stage{selectedNote.stageNumber} JS"; // Example: Stage1, Stage2, etc.

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            CircleTransition ct = FindObjectOfType<CircleTransition>();
            if (ct != null)
            {
                ct.FadeOut();
                GameManager.stm.SetCurrentStage(selectedNote.stageNumber);
                StartCoroutine(WaitAndLoadScene(sceneName));
            }
            else
            {
                Debug.LogError("CircleTransition not found in the scene.");
            }
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found. Please ensure it is added to the Build Settings.");
        }
    }
    
    // Coroutine to wait for 2 seconds and then load the scene
    private IEnumerator WaitAndLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }


    int FindLastUnlockedIndex()
    {
        for (int i = 0; i < levelNotes.Count; i++)
        {
            if (levelNotes[i].isLocked())
            {
                if (i == 0) return 0;
                return i - 1;
            }
        }
        return 0; // Default to the first stage if no unlocked stages are found
    }

  /*  IEnumerator SlideIn()
{
    float elapsedTime = 0f;

    while (elapsedTime < slideDuration)
    {
        elapsedTime += Time.deltaTime;

        // Apply an ease-out effect using Mathf.SmoothStep
        float t = elapsedTime / slideDuration;
        t = Mathf.SmoothStep(0f, 1.1f, t);

        rectTransform.anchoredPosition = Vector2.Lerp(offScreenPosition, onScreenPosition, t);
        yield return null;
    }

    // Snap to the final position to ensure precision
    rectTransform.anchoredPosition = onScreenPosition;
}*/

}

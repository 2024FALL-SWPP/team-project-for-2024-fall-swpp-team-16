using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionNoteController : MonoBehaviour
{
    public Sprite lockedSprite;   // Sprite for locked state
    public Sprite selectedSprite;   // Sprite for selected state
    public Sprite unlockedSprite;     // Sprite for unlocked state


    private Image imageComponent;   // UI Image component
    public int stageNumber;         // Set this for each stage note in the Inspector
    public bool isLocked;         // Whether this stage is unlocked
    public bool isSelected;         // Whether this stage is currently selected

    void Start()
    {
        imageComponent = GetComponent<Image>();
        UpdateNoteAppearance();
    }

    public void UpdateNoteAppearance()
    {
        if (isSelected)
        {
            imageComponent.sprite = selectedSprite;
        }
        else if (isLocked)
        {
            imageComponent.sprite = lockedSprite;
        }
        else
        {
            imageComponent.sprite = unlockedSprite;
        }
    }

}

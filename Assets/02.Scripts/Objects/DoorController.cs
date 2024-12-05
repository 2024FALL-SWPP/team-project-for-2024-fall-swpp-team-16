using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For Image components

public class DoorController : MonoBehaviour
{
    public GameObject scoreUIPanel; 
    public int[] answerNotes; // assign note values in inspector (do, re, mi)
    private List<int> playedNotes = new List<int>(); 
    public GameObject doorWing; 

    public GameObject rippleEffectPrefab; 
    public GameObject[] coloredNotes; // assign objects (in hierarchy)

    void Start()
    {
        scoreUIPanel.SetActive(false);

        // Initialize all notes to black
        foreach (var note in coloredNotes)
        {
            Image image = note.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.black;
            }
            else
            {
                Debug.LogError($"Missing Image component on {note.name}");
            }
        }
    }

    public void Inspect(GameObject floatingText)
    {
        if (GameManager.im.HasAllScores())
        {
            scoreUIPanel.SetActive(true);
            Debug.Log("Play the music to open the door");
            floatingText.SendMessage("Hide");
        }
        else
        {
            Debug.Log("You need to collect all the scores to open the door");
        }
    }

    void Update()
    {
        if (scoreUIPanel.activeSelf)
        {
            for (int i = 1; i <= 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + (i - 1)))
                {
                    playedNotes.Add(i);

                    if (CheckNotes())
                    {
                        UpdateColoredNote(i);

                        if (playedNotes.Count == answerNotes.Length)
                        {
                            Debug.Log("Correct notes played. Door is opening.");
                            GameManager.stm.CompleteCurrentStage();
                            StartCoroutine(OpenDoor());
                            playedNotes.Clear();
                        }
                    }
                    else
                    {
                        Debug.Log("Incorrect notes played. Try again.");
                        GameManager.sm.PlaySound("wrong-answer");
                        playedNotes.Clear();
                        ResetColoredNotes();
                    }
                }
            }
        }
    }

    private void UpdateColoredNote(int note)
    {
        if (playedNotes.Count - 1 < coloredNotes.Length)
        {
            GameObject noteObject = coloredNotes[playedNotes.Count - 1];
            Image image = noteObject.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetNoteColor(note);

                // Create ripple effect
                GameObject ripple = Instantiate(rippleEffectPrefab, noteObject.transform.position + new Vector3(0, 0, -1), Quaternion.identity);
                ripple.transform.SetParent(noteObject.transform);
                ripple.transform.localScale = Vector3.one * 10;
                Destroy(ripple, 1.0f);

               // StartCoroutine(ScaleEffect(noteObject.transform)); // IF NOTE SHOULD SCALE WHEN PLAYED UNCOMMENT THIS
            }
        }
    }

    private Color GetNoteColor(int note)
    {
        switch (note)
        {
            case 1: return Color.red;
            case 2: return new Color(1f, 0.5f, 0f); // Orange
            case 3: return Color.yellow;
            case 4: return Color.green;
            case 5: return Color.blue;
            case 6: return new Color(0.5f, 0f, 0.5f); // Purple
            case 7: return new Color(1f, 0.75f, 0.8f); // Pink
            default: return Color.white;
        }
    }

    private IEnumerator ScaleEffect(Transform targetTransform)
    {
        float duration = 0.15f;
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = Vector3.one;

        float time = 0;
        while (time < duration)
        {
            targetTransform.localScale = Vector3.Lerp(initialScale, finalScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        targetTransform.localScale = finalScale;
    }

    private void ResetColoredNotes()
    {
        foreach (GameObject note in coloredNotes)
        {
            Image image = note.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.black;
            }
        }
    }

    private bool CheckNotes()
    {
        for (int i = 0; i < playedNotes.Count; i++)
        {
            if (playedNotes[i] != answerNotes[i])
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(1.5f);
        scoreUIPanel.SetActive(false);
        GameManager.sm.PlaySound("opening-door");
        StartCoroutine(OpenDoorAnimation(doorWing));
    }

    private IEnumerator OpenDoorAnimation(GameObject door)
    {
        float duration = 1f;
        float startRotation = door.transform.eulerAngles.y;
        float endRotation = startRotation + 90;
        float time = 0;

        while (time < duration)
        {
            float yRotation = Mathf.Lerp(startRotation, endRotation, time / duration);
            door.transform.eulerAngles = new Vector3(door.transform.eulerAngles.x, yRotation, door.transform.eulerAngles.z);
            time += Time.deltaTime;
            yield return null;
        }

        door.transform.eulerAngles = new Vector3(door.transform.eulerAngles.x, endRotation, door.transform.eulerAngles.z);
    }
}

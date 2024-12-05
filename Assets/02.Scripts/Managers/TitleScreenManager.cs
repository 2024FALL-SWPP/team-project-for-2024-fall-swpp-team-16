using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenManager : MonoBehaviour
{
    public string gameSceneName = "StageScene"; // Name of the main game scene
    public Image fadeImage;         // Reference to the UI Image for fading
    public float fadeDuration = 0.1f; // Duration of the fade

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Detect Enter key press
        {
            StartCoroutine(FadeAndStartGame());
        }
    }

    IEnumerator FadeAndStartGame()
    {
        // Ensure the fade image is active
        fadeImage.gameObject.SetActive(true);

        // Gradually increase the alpha value to fade to black
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration); // Fade to black
            fadeImage.color = color;
            yield return null;
        }

        // Ensure the screen is fully black
        color.a = 1f;
        fadeImage.color = color;

        // Load the new scene
        SceneManager.LoadScene(gameSceneName);
    }
}

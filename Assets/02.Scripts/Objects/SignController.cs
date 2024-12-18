using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SignController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialSignUI; // UI GameObject
    [SerializeField] private List<string> textContents;
    [SerializeField] private float imageWidth = 1500;
    [SerializeField] private float imageHeight = 800;
    [SerializeField] private List<Sprite> guideImages;
    
    private RectTransform imageRect; // Image의 RectTransform
    private Image signPicture;
    private TextMeshProUGUI textComponent; // Text Component
    private bool isActive = false;
    private PlayerInput playerInput; // 동적으로 가져올 PlayerInput
    private int currentIndex = 0; // 현재 보여줄 인덱스

    private void Awake()
    {
        if (tutorialSignUI != null)
        {
            imageRect = tutorialSignUI.GetComponentInChildren<Image>().rectTransform;
            textComponent = tutorialSignUI.GetComponentInChildren<TextMeshProUGUI>();
            signPicture = tutorialSignUI.GetComponentsInChildren<Image>()[1];
        }
        else
        {
            Debug.LogError("TutorialSignUI is not assigned!");
        }
    }

    private void Start()
    {
        tutorialSignUI.SetActive(false);

        // PlayerMovement를 찾아 PlayerInput 참조 설정
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerInput = playerMovement.GetPlayerInput();
        }
        else
        {
            Debug.LogWarning("PlayerMovement not found! PlayerInput may not be set.");
        }
    }

    public void Inspect(GameObject floatingText)
    {
        if (GameManager.gm.isSheetOn){
            return;
        }
        if (isActive)
        {
            // E를 누르면 다음으로 넘어감
            currentIndex++;
            if (currentIndex >= textContents.Count)
            {
                // 모든 항목을 다 본 경우 UI 비활성화
                tutorialSignUI.SetActive(false);
                GameManager.sm.PlaySound("sign");
                isActive = false;
                currentIndex = 0;

                if (playerInput != null)
                    playerInput.active = true; // 플레이어 움직임 활성화
            }
            else
            {
                // 다음 텍스트와 이미지를 표시
                UpdateSignUI();
                GameManager.sm.PlaySound("sign");
            }
        }
        else
        {
            if (imageRect == null || textComponent == null)
            {
                Debug.LogError("Image or Text component is not found!");
                return;
            }

            // 처음 열 때 첫 번째 항목 표시
            currentIndex = 0;
            UpdateSignUI();

            tutorialSignUI.SetActive(true);
            GameManager.sm.PlaySound("sign");
            isActive = true;

            if (playerInput != null)
                playerInput.active = false; // 플레이어 움직임 비활성화
        }
    }
    
    private void UpdateSignUI()
    {
        // 텍스트 업데이트
        if (currentIndex < textContents.Count)
        {
            textComponent.text = textContents[currentIndex].Replace("\\n", "\n");
        }

        // 이미지 업데이트
        if (currentIndex < guideImages.Count && guideImages[currentIndex] != null)
        {
            signPicture.enabled = true;
            signPicture.sprite = guideImages[currentIndex];
        }
        else
        {
            signPicture.enabled = false;
        }

        // 이미지 크기 업데이트
        imageRect.sizeDelta = new Vector2(imageWidth, imageHeight);
    }
    
}

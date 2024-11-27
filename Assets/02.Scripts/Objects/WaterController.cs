using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    // Configurations
    public float waterLevelMax = 100.0f; // Maximum height of water
    public float waterLevelMin = 0.0f; // Minimum height of water
    public float waterLevelTargetUnit = 1.0f; // Target height of water
    public float timeToReachTarget = 2.0f; // 목표에 도달할 시간
    
    // Internal variables (But made public for debugging)
    public float initialPositionY;
    public bool isHeightChanging = false;
    public float waterLevelTarget; // Target height of water
    
    private float elapsedTime = 0.0f;
    private float startHeight; // 현재 높이를 저장할 변수
    
    void Start()
    {
        initialPositionY = transform.position.y;
        waterLevelTarget = initialPositionY;
    }

    void Update()
    {
        if (isHeightChanging)
        {
            // 경과 시간 갱신
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / timeToReachTarget); // 진행률 계산

            // 부드러운 가속/감속을 적용하여 높이 변경
            float smoothStepValue = Mathf.SmoothStep(0, 1, t);
            float newY = Mathf.LerpUnclamped(startHeight, waterLevelTarget, smoothStepValue);

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 목표에 도달했으면 높이 변경 중지 및 초기화
            if (t >= 1.0f)
            {
                transform.position = new Vector3(transform.position.x, waterLevelTarget, transform.position.z);
                isHeightChanging = false;
                elapsedTime = 0.0f; // 초기화
            }
        }
    }
    
    public bool TriggerStepIncreaseWaterLevel()
    {
        if (isHeightChanging) return false; // If already changing, return false

        if (waterLevelTarget+waterLevelTargetUnit > waterLevelMax)
        {
            Debug.Log("Water level is already at the maximum height.");
            waterLevelTarget = waterLevelMax; // Adjust to max if exceeded
            return false;
        }
        waterLevelTarget += waterLevelTargetUnit;
        TriggerChangeWaterLevel();
        return true; // Successfully initiated height change
    }
    
    public bool TriggerStepDecreaseWaterLevel()
    {
        if (isHeightChanging) return false; // If already changing, return false

        if (waterLevelTarget-waterLevelTargetUnit < waterLevelMin)
        {
            Debug.Log("Water level is already at the minimum height.");
            waterLevelTarget = waterLevelMin; // Adjust to min if exceeded
            return false;
        }
        waterLevelTarget -= waterLevelTargetUnit;
        TriggerChangeWaterLevel();
        return true; // Successfully initiated height change
    }
    
    private void TriggerChangeWaterLevel()
    {
        isHeightChanging = true;
        startHeight = transform.position.y; // 현재 높이를 시작 높이로 설정
        elapsedTime = 0.0f; // 새로운 변화가 시작되므로 경과 시간 초기화
    }
}
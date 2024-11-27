using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlateController : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float rayLength = 1f; // Ray 길이
    [SerializeField] private float stayDuration = 0.5f; // 감지 최소 시간
    [SerializeField] private LayerMask detectionLayer; // 감지 Layer
    [SerializeField] private int gridResolution = 5; // Ray 촘촘함 정도
    private float stayTime = 0f; // Plate 위에서 머문 시간
    private Bounds plateBounds; // Plate의 Bounds 정보

    [Header("Flag")]
    [SerializeField] private bool isPressed = false; // Press 상태 플래그
    private bool isLowering = false; // Plate가 낮아지고 있는지 여부
    private bool isRaising = false; // Plate가 원래 높이로 복귀 중인지 여부
    
    [Header("Fine-tuning")]
    [SerializeField] private ParticleSystem particleEffect; // 재생할 ParticleSystem
    [SerializeField] private float lowerHeight = 0.5f; // Plate가 낮아질 높이
    [SerializeField] private float lowerDuration = 1f; // Plate가 낮아지는 데 걸리는 시간
    [SerializeField] private float raiseDuration = 1f; // Plate가 원래 높이로 복귀하는 데 걸리는 시간
    
    // internal variables
    private Vector3 originalPosition; // Plate의 원래 위치
    private float raiseTimer = 0f; // RaisePlate 타이머
    
    // observer pattern    
    private List<IPlateObserver> observers = new List<IPlateObserver>();
    
    
    
    void Start()
    {
        if (TryGetComponent<Collider>(out Collider collider))
        {
            plateBounds = collider.bounds;
            originalPosition = transform.position;
        }
        else
        {
            Debug.LogError("No Collider found on Plate.");
        }
    }
    
    public void RegisterObserver(IPlateObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void UnregisterObserver(IPlateObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    private void NotifyObservers(bool isPressed)
    {
        foreach (var observer in observers)
        {
            observer.OnPlateStateChanged(isPressed);
        }
    }
    
    public float GetPressProgress()
    {
        Debug.Log(Mathf.Clamp01((originalPosition.y - transform.position.y) / lowerHeight));
        // Plate가 얼마나 눌렸는지를 반환 (0 = 완전히 올라감, 1 = 완전히 눌림)
        return Mathf.Clamp01((originalPosition.y - transform.position.y) / lowerHeight);
    }

    void Update()
    {
        DetectObjectsAbovePlate();
        
        if (isLowering)
        {
            LowerPlate();
        }

        // Plate가 원래 높이로 복귀하는 로직 처리
        if (isRaising)
        {
            RaisePlate();
        }
    }

    private void DetectObjectsAbovePlate()
    {
        bool isObjectDetected = false;

        // Plate 위의 범위를 격자로 나누어 Ray 발사
        for (int x = 0; x < gridResolution; x++)
        {
            for (int z = 0; z < gridResolution; z++)
            {
                // Plate의 XZ 평면 범위 내에서 격자 위치 계산
                float sampleX = Mathf.Lerp(plateBounds.min.x, plateBounds.max.x, x / (float)(gridResolution - 1));
                float sampleZ = Mathf.Lerp(plateBounds.min.z, plateBounds.max.z, z / (float)(gridResolution - 1));
                Vector3 samplePoint = new Vector3(sampleX, plateBounds.max.y, sampleZ); // Plate 위의 점

                Ray ray = new Ray(samplePoint, Vector3.up); // 위에서 아래로 Ray 발사
                if (Physics.Raycast(ray, out RaycastHit hit, rayLength, detectionLayer))
                {
                    isObjectDetected = true;
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green); // 디버깅용 Ray
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red); // 디버깅용 Ray
                }
            }
        }

        // 객체가 감지된 경우 타이머 증가
        if (isObjectDetected)
        {
            stayTime += Time.deltaTime;

            if (!isPressed && stayTime >= stayDuration)
            {
                isPressed = true; // 한 번만 실행
                NotifyObservers(true);
                TriggerPressEffect();
            }
        }
        else
        {
            // 객체가 감지되지 않으면 초기화
            stayTime = 0f;
            if (isPressed){
                isPressed = false;
                NotifyObservers(false);
                TriggerUnpressEffect(); // 높이 복구 시작
            }
        }
    }
    
    private void TriggerPressEffect()
    {
        if (particleEffect != null)
        {
            particleEffect.Play(); // Particle 재생
        }
        isLowering = true; // 낮추기 시작
        isRaising = false; // 복구 중지
    }

    private void TriggerUnpressEffect()
    {
        isRaising = true; // 복구 시작
        isLowering = false; // 낮추기 중지
    }

    private void LowerPlate()
    {
        stayTime += Time.deltaTime;
        float t = Mathf.Clamp01(stayTime / lowerDuration);
        float smoothStepT = Mathf.SmoothStep(0, 1, t);
        transform.position = Vector3.Lerp(originalPosition, originalPosition - new Vector3(0, lowerHeight, 0), smoothStepT);

        if (t >= 1f)
        {
            isLowering = false; // 낮추기 완료
        }
    }

    private void RaisePlate()
    {
        raiseTimer += Time.deltaTime;
        float t = Mathf.Clamp01(raiseTimer / raiseDuration);
        float smoothStepT = Mathf.SmoothStep(0, 1, t);
        transform.position = Vector3.Lerp(transform.position, originalPosition, smoothStepT);

        if (transform.position == originalPosition)
        {
            transform.position = originalPosition;
            isRaising = false; // 복구 완료
            raiseTimer = 0f;
        }
    }
}

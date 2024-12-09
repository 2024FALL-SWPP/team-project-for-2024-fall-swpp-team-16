using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Collections.Shaders.CircleTransition;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem ripplesEffectPrefab;
    [SerializeField] private float colorSwitchInterval = 0.5f;
    private float colorSwitchTimer;
    private float defaultSize = 7;
    
    public void TriggerRipples(Transform target, Color color, Vector3 targetScale, Vector3 positionOffset, bool isPlayer = false)
    {
        if (!GameManager.pm.activeRipplesEffects.ContainsKey(target))
        {
            ParticleSystem newEffect = Instantiate(ripplesEffectPrefab, target.position, Quaternion.identity);

            if (isPlayer)
            {
                // Collider를 사용하여 위치 계산
                Collider collider = target.GetComponent<Collider>();
                if (collider is not null)
                {
                    Bounds bounds = collider.bounds;

                    // 기본 위치는 Collider 중심 + 오프셋
                    Vector3 effectPosition = bounds.center + positionOffset;
                    newEffect.transform.position = effectPosition;
                }
            }

            newEffect.transform.SetParent(target, true);
            GameManager.pm.RegisterTarget(target, newEffect); // PlayManager에 등록
        }

        if (!GameManager.pm.activeRipplesColors[target].Contains(color))
        {
            GameManager.pm.activeRipplesColors[target].Add(color);
        }
        
        ParticleSystem activeEffect = GameManager.pm.activeRipplesEffects[target];

        if (isPlayer)
        {
            var mainModule = activeEffect.main;
            mainModule.startSize = Mathf.Max(targetScale.x, targetScale.y, targetScale.z) * defaultSize;
        }
        else { SetRippleSize(target, positionOffset : positionOffset); }
        
        if (!isPlayer || !activeEffect.isPlaying)
        {
            activeEffect.Play();
        }
    }
    
    public void SetRippleSize(Transform target, float multiplier = 3.0f, float minParticleSize = 0.1f, float maxParticleSize = 6f, Vector3 positionOffset = default)
    {
        if (!GameManager.pm.activeRipplesEffects.ContainsKey(target)) return;

        Collider collider = target.GetComponent<Collider>();
        if (collider is not null)
        {
            Bounds bounds = collider.bounds;

            // y축의 중간 위치 계산
            float yMidPosition = bounds.min.y + (bounds.size.y * 0.5f);

            // xz 평면의 크기 계산
            // 로컬 크기를 글로벌 크기로 변환
            Vector3 localSize = collider.bounds.size; // 이미 Global Space 크기
            Vector3 globalSize = new Vector3(
                localSize.x * target.lossyScale.x,
                localSize.y * target.lossyScale.y,
                localSize.z * target.lossyScale.z
            );

            // xz 크기 계산
            float xzAverage = (globalSize.x + globalSize.z) / 2;

            // 크기 제한 및 설정
            float particleSize = Mathf.Clamp(xzAverage * multiplier, minParticleSize, maxParticleSize);

            Debug.Log($"[SetRippleSize] Target: {target.name}, ParticleSize: {particleSize}, Y-Mid: {yMidPosition}");

            // 파티클 효과에 크기 적용
            ParticleSystem effect = GameManager.pm.activeRipplesEffects[target];
            var mainModule = effect.main;
            mainModule.startSize = particleSize; // 파티클 크기 설정

            // 파티클의 위치를 y축 중간 위치로 업데이트
            Vector3 newEffectPosition = new Vector3(bounds.center.x, yMidPosition, bounds.center.z);
            effect.transform.position = newEffectPosition + positionOffset;
        }
    }


    public void UpdateRipplePosition(Transform target, Vector3 newPosition)
    {
        if (GameManager.pm.activeRipplesEffects.ContainsKey(target))
        {
            ParticleSystem effect = GameManager.pm.activeRipplesEffects[target];
            effect.transform.position = newPosition;
        }
    }

    public void RemoveColorFromRipples(Transform target, Color color)
    {
        if (GameManager.pm.activeRipplesColors.ContainsKey(target))
        {
            GameManager.pm.activeRipplesColors[target].Remove(color);
        }
    }

    public void StopRipples(Transform target)
    {
        if (GameManager.pm.activeRipplesEffects.ContainsKey(target))
        {
            ParticleSystem effect = GameManager.pm.activeRipplesEffects[target];
            if (effect.isPlaying)
            {
                effect.Stop();
                GameManager.pm.UnregisterTarget(target); // PlayManager에서 타겟 제거
                StartCoroutine(DestroyAfterDuration(effect, target)); // 파티클 삭제 예약
            }
        }
    }
    
    private IEnumerator DestroyAfterDuration(ParticleSystem effect, Transform target)
    {
        yield return new WaitForSeconds(effect.main.duration/2); // duration만큼 대기
        if (effect)
        {
            Destroy(effect.gameObject); // 파티클 오브젝트 제거
        }
    }


    private void makeRipples()
    {
        colorSwitchTimer += Time.deltaTime;
        
        if (colorSwitchTimer >= colorSwitchInterval)
        {
            colorSwitchTimer = 0f;
            
            foreach (var entry in GameManager.pm.activeRipplesEffects)
            {
                Transform target = entry.Key;
                ParticleSystem effect = entry.Value;

                if (GameManager.pm.activeRipplesColors.ContainsKey(target) && GameManager.pm.activeRipplesColors[target].Count > 0)
                {
                    var mainModule = effect.main;
                    mainModule.startColor = GameManager.pm.activeRipplesColors[target][0];
                    
                    Color firstColor = GameManager.pm.activeRipplesColors[target][0];
                    GameManager.pm.activeRipplesColors[target].RemoveAt(0);
                    GameManager.pm.activeRipplesColors[target].Add(firstColor);
                }
            }
        }
    }

    private void Update()
    {
        if (GameManager.pm is not null)
        {
            makeRipples();
        }
    }

    public void NoEffectOnCt()
    {
        CircleTransition ct = FindObjectOfType<CircleTransition>();
        if (ct != null)
        {
            ct.FastFadeOut();
        }
        else
        {
            Debug.LogError("CircleTransition not found in the scene.");
        }
    }
    
    public void FadeOutCircleTransition()
    {
        CircleTransition ct = FindObjectOfType<CircleTransition>();
        if (ct != null)
        {
            ct.FadeOut();
        }
        else
        {
            Debug.LogError("CircleTransition not found in the scene.");
        }
    }

    public void FadeInCircleTransition()
    {
        CircleTransition ct = FindObjectOfType<CircleTransition>();
        if (ct != null)
        {
            ct.FadeIn();
        }
        else
        {
            Debug.LogError("CircleTransition not found in the scene.");
        }
    }
}

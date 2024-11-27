using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    private int durability = 1;
    [SerializeField] private ParticleSystem particleEffect; // 재생할 ParticleSystem
    [SerializeField] private GameObject models;
    private BoxCollider bc;

    private void Start()
    {
        bc = GetComponent<BoxCollider>();
        // Test: Invoke("Damage", 5f);
    }

    public void Damage()
    {
        if (--durability <= 0) Collapse();
    }

    private void Collapse()
    {
        if (particleEffect != null)
        {
            // 파티클 재생
            particleEffect.Play();
            // 파티클이 끝날 때 오브젝트를 삭제하는 코루틴 호출
            StartCoroutine(DestroyAfterParticle(particleEffect));
        }
        else
        {
            // 파티클이 없는 경우 즉시 삭제
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyAfterParticle(ParticleSystem particle)
    {
        float halfDuration = particle.main.duration / 2f;
        yield return new WaitForSeconds(halfDuration);
        
        models.SetActive(false);
        bc.enabled = false;
        
        // 파티클이 재생 중일 때까지 대기
        while (particle.isPlaying)
        {
            yield return null; // 다음 프레임까지 대기
        }
        // 오브젝트 삭제
        Destroy(gameObject);
    }
    
}

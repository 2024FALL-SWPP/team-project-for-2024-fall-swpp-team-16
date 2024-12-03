using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RipplesHandler : MonoBehaviour
{
    [SerializeField] private float ripplesHeightRatio = 0.5f;
    [SerializeField] private Vector3 ripplesPositionOffset = Vector3.zero;
    void Update()
    {
        // 각 pitch에 대해 파티클 처리
        foreach (PitchType pitch in Enum.GetValues(typeof(PitchType)))
        {
            Color color = GameParameters.PitchColors[(int)pitch];
            KeyCode key = GameParameters.PitchKeys[(int)pitch];
            // 각 키가 눌린 상태에서 파티클을 재생
            if (Input.GetKey(key)) TriggerParticleEffect(color);
            // 키가 떼어질 때 해당 색상을 제거
            if (Input.GetKeyUp(key)) GameManager.em.RemoveColorFromRipples(transform, color);
        }

        // 모든 키 입력이 없을 때 파티클을 멈춤
        bool noPitchInput = true;
        foreach (PitchType pitch in Enum.GetValues(typeof(PitchType)))
        {
            KeyCode key = GameParameters.PitchKeys[(int)pitch];
            if (Input.GetKey(key)) noPitchInput = false;
        }
        if (noPitchInput) GameManager.em.StopRipples(transform);
    }

    private void TriggerParticleEffect(Color color)
    {
        GameManager.em.TriggerRipples(transform, color, transform.localScale, ripplesPositionOffset, true);
    }
}

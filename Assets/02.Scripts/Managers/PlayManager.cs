using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayManager : MonoBehaviour
{
    public Transform playerTransform;

    // 관리할 파티클 효과와 색상 목록을 딕셔너리로 유지
    public Dictionary<Transform, ParticleSystem> activeRipplesEffects = new Dictionary<Transform, ParticleSystem>();
    public Dictionary<Transform, List<Color>> activeRipplesColors = new Dictionary<Transform, List<Color>>();
    public Transform currentTarget;

    private float defaultSize = 6;
    private float targetScaleMultiplier = 1.5f;

    private void Start()
    {
        currentTarget = null;
    }

    private void HandleTargetSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            List<Transform> targets = new List<Transform>(activeRipplesEffects.Keys);
            targets.Remove(playerTransform);

            if (targets.Count == 0) return;

            int currentIndex = currentTarget is not null ? targets.IndexOf(currentTarget) : -1;
            int nextIndex = (currentIndex + 1) % targets.Count;

            SetCurrentTarget(targets[nextIndex]);
        }
    }
    private void HandleResonance()
    {
        if (currentTarget is null) return;
        ResonatableObject resonatable = currentTarget.GetComponent<ResonatableObject>();
        if (resonatable is null) return;
        foreach (PitchType pitch in Enum.GetValues(typeof(PitchType)))
        {
            KeyCode key = GameParameters.PitchKeys[(int)pitch];
            if (Input.GetKey(key)) resonatable.resonate(pitch);
        }
    }

    private void SetCurrentTarget(Transform newTarget)
    {
        if (currentTarget is not null && activeRipplesEffects.ContainsKey(currentTarget))
        {
            var mainModule = activeRipplesEffects[currentTarget].main;
            mainModule.startSize = defaultSize;
        }

        currentTarget = newTarget;

        if (currentTarget is not null && activeRipplesEffects.ContainsKey(currentTarget))
        {
            var mainModule = activeRipplesEffects[currentTarget].main;
            mainModule.startSize = defaultSize * targetScaleMultiplier;
        }
    }


    private void Update()
    {
        HandleTargetSwitch();
        HandleResonance();
    }
}
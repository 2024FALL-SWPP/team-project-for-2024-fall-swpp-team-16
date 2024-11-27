using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    
    [SerializeField] private int durability = 3;
    // Prefabs must contain mesh component
    [SerializeField] private GameObject treePrefab;
    
    [SerializeField] private float minHeight = -3f;
    [SerializeField] private float maxHeight = 3f;
    
    private readonly float heightChangeSmoothTime = 0.6f;
    private float heightChangeSpeed = 1f;
    private float targetHeight;
    private Vector3 currentVelocity = Vector3.zero;
    
    private bool isCollapsed = false;

    private void Awake()
    {
        ResonatableObject resonatable = gameObject.AddComponent<ResonatableObject>();
        resonatable.properties = new[] { PitchType.So, PitchType.La };
        resonatable.resonate += TreeResonate;
    }

    private void TreeResonate(PitchType pitch)
    {
        switch (pitch)
        {
            case PitchType.So: { SmoothIncreaseHeight(); break; }
            case PitchType.La: { SmoothDecreaseHeight(); break; }
        }
    }

    private void Start()
    {
        targetHeight = Mathf.Clamp(treePrefab.transform.localPosition.y, minHeight, maxHeight);
        treePrefab.transform.localPosition = new Vector3(0f, targetHeight, 0f);
    }

    private void Update()
    {
        if (isCollapsed) return;
        Vector3 currentPosition = treePrefab.transform.localPosition;
        Vector3 targetPosition = new Vector3(0f, targetHeight, 0f);
        // Debug.Log((currentPosition, targetPosition));
        if (currentPosition != targetPosition)
        {
            treePrefab.transform.localPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref currentVelocity, heightChangeSmoothTime);
        }
    }

    public void SmoothIncreaseHeight()
    {
        targetHeight += heightChangeSpeed * Time.deltaTime;
        targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);
    }

    public void SmoothDecreaseHeight()
    {
        targetHeight -= heightChangeSpeed * Time.deltaTime;
        targetHeight = Mathf.Clamp(targetHeight, minHeight, maxHeight);
    }
    public void Damage()
    {
        if (isCollapsed) return;
        if (--durability <= 0) Collapse();
    }
    
    public void Collapse()
    {
        if (isCollapsed) return;
        Cutter.Cut(treePrefab, transform.position + Vector3.up * 0.1f, Vector3.up);
        isCollapsed = true;
    }
}

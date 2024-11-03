using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{
    
    [SerializeField] private int durability = 3;
    [SerializeField] private int currentLevel = 0;
    // Prefabs must contain mesh component
    [SerializeField] private GameObject treePrefab;
    
    private readonly float[] heightByLevel = { -1f, -0.5f, -0.1f };
    private readonly float growSmoothTime = 0.5f;
    private IEnumerator movingCoroutine = null;
    private bool isCollapsed = false;

    private void Start()
    {
        if (treePrefab == null) Debug.LogError("TreePrefab is null");
        treePrefab.transform.position = new Vector3(0f, heightByLevel[currentLevel], 0f);
    }

    public void Grow()
    {
        if (isCollapsed || currentLevel >= heightByLevel.Length - 1) return;
        SetLevel(currentLevel + 1);
    }

    public void Shrink()
    {
        if (isCollapsed || currentLevel <= 0) return;
        SetLevel(currentLevel - 1);
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

    private void SetLevel(int level)
    {
        currentLevel = level;
        if(movingCoroutine != null) StopCoroutine(movingCoroutine);
        movingCoroutine = MoveOverTime(treePrefab, new Vector3(0f, heightByLevel[currentLevel], 0f), growSmoothTime);
        StartCoroutine(movingCoroutine);
    }
    
    private IEnumerator MoveOverTime (GameObject objectToMove, Vector3 end, float smoothTime)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < smoothTime)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / smoothTime));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }
}

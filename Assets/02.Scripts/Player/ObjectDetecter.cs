using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDetecter : MonoBehaviour
{
    public float detectionRadius = 5f;

    void Start()
    {
        // Sphere Collider 설정
        SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = detectionRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ResonatableObject가 들어올 때
        ResonatableObject resonatable = other.GetComponent<ResonatableObject>();
        if (resonatable != null)
        {
            resonatable.OnEnterRadius();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ResonatableObject가 나갈 때
        ResonatableObject resonatable = other.GetComponent<ResonatableObject>();
        if (resonatable != null)
        {
            resonatable.OnExitRadius();
        }
    }
}
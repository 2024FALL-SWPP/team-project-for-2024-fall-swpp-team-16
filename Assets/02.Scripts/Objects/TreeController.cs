using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(CapsuleCollider))]
public class TreeController : Interactable
{
    [SerializeField] private int durability = 1;
    [SerializeField] private GameObject treePrefab;
    
    [SerializeField, Range(3f, 15f)] private float prefabHeight = 13.5f;
    [SerializeField, Range(3f, 15f)] private float minHeight = 3f;
    [SerializeField, Range(3f, 15f)] private float maxHeight = 13f;
    [SerializeField, Range(3f, 15f)] private float currentHeight = 8f;
    [SerializeField, Range(0f, 1f)] private float radius = 0.7f;
    
    private readonly float heightChangeSmoothTime = 0.6f;
    private float heightChangeSpeed = 1f;
    private Vector3 currentVelocity = Vector3.zero;
    
    private HingeJoint hinge;
    private CapsuleCollider capsuleCollider;
    private bool isCollapsed = false;
    
    private bool isPlayingSound = false;
    private GameObject _movingSound;

    private void Start()
    {
        Assert.IsNotNull(treePrefab, "TreePrefab cannot be null.");
        Assert.IsTrue(treePrefab.TryGetComponent<MeshFilter>(out _), "TreePrefab must have MeshFilter component.");
        maxHeight = Mathf.Clamp(maxHeight, 0f, prefabHeight);
        minHeight = Mathf.Clamp(minHeight, 0f, maxHeight);
        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);
        treePrefab.transform.localPosition = new Vector3(0f, currentHeight - prefabHeight, 0f);
        treePrefab.transform.localScale = Vector3.one;
        capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
        capsuleCollider.height = 2 * currentHeight;
        capsuleCollider.radius = radius;
    }

    private void Awake()
    {
        hinge = gameObject.GetComponent<HingeJoint>();
        ResonatableObject resonatable = gameObject.AddComponent<ResonatableObject>();
        resonatable.properties = new[] { PitchType.So, PitchType.La };
        resonatable.resonate += TreeResonate;
        resonatable.ripplesPositionOffset = Vector3.up * 0.5f;
    }

    private void TreeResonate(PitchType pitch)
    {
        switch (pitch)
        {
            case PitchType.So: { SmoothIncreaseHeight(); break; }
            case PitchType.La: { SmoothDecreaseHeight(); break; }
        }
    }

    private void Update()
    {
        if (isCollapsed) return;
        Vector3 currentPosition = treePrefab.transform.localPosition;
        Vector3 targetPosition = new Vector3(0f, currentHeight - prefabHeight, 0f);
        capsuleCollider.height = 2 * currentHeight;
        
        if (currentPosition != targetPosition)
        {
            treePrefab.transform.localPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref currentVelocity, heightChangeSmoothTime);
            if (Vector3.Distance(currentPosition, targetPosition) < 0.02f)
            {
                if (isPlayingSound)
                {
                    StopTreeSound();
                    isPlayingSound = false;
                }
            }
        }
        else
        {
            if (isPlayingSound)
            {
                StopTreeSound();
                isPlayingSound = false;
            }
        }
    }

    private void SmoothIncreaseHeight()
    {
        currentHeight += heightChangeSpeed * Time.deltaTime;
        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);
        
        // Play tree sound
        if (!isPlayingSound)
        {
            PlayTreeSound("increase");
            isPlayingSound = true;
        }
    }

    private void SmoothDecreaseHeight()
    {
        currentHeight -= heightChangeSpeed * Time.deltaTime;
        currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);

        // Play tree sound
        if (!isPlayingSound)
        {
            PlayTreeSound("decrease");
            isPlayingSound = true;
        }
    }
    
    private void PlayTreeSound(string action)
    {
        _movingSound = GameManager.sm.PlayLoopSound("tree-" + action);
        AudioSource source = _movingSound.GetComponent<AudioSource>();
        if (source != null)
        {
            source.volume *= 1f; // Reduce volume by half
        }
        Debug.Log("Playing tree sound");
    }
    
    private void StopTreeSound()
    {
        // Implement sound stopping logic here
        Destroy(_movingSound);
        Debug.Log("Stopping tree sound");
    }
    
    public void Damage(Transform damager)
    {
        if (isCollapsed) return;
        if (--durability <= 0)
        {
            var direction = transform.position - damager.position;
            direction.y = 0f;
            direction.Normalize();
            Collapse(direction);
        }
    }
    
    public void Collapse(Vector3 direction)
    {
        const float initialAngularVelocity = 1f;
        var cutOffset = radius + 0.1f;
        
        if (isCollapsed) return;
        var cutPoint = transform.position + Vector3.up * cutOffset;
        var cutTree = Cutter.Cut(treePrefab, cutPoint, Vector3.up);
        var cutCollider = cutTree.AddComponent<CapsuleCollider>();

        var remainHeight = cutPoint.y - treePrefab.transform.position.y;
        var cutHeight = prefabHeight - remainHeight;
        cutCollider.center = new Vector3(0f, (prefabHeight + remainHeight) / 2f, 0f);
        cutCollider.height = cutHeight;
        cutCollider.radius = radius;
        
        cutTree.AddComponent<PhysicsImmunity>();
        
        var cutRigidbody = cutTree.AddComponent<Rigidbody>();
        var axis = Vector3.Cross(Vector3.up, direction).normalized;
        var torque = axis * initialAngularVelocity;
        
        cutRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        cutRigidbody.centerOfMass = Vector3.up * prefabHeight;
        
        hinge.connectedBody = cutRigidbody;
        hinge.anchor = Vector3.up * cutOffset;
        hinge.axis = axis;
        
        cutRigidbody.AddTorque(torque, ForceMode.VelocityChange);
        cutRigidbody.mass = 100;
        
        isCollapsed = true;
        capsuleCollider.enabled = false;
        GameManager.em.StopRipples(transform);
        GameManager.pm.UnregisterTarget(transform);
        PlayCollapseSound();
        
        this.enabled = false;
        Destroy(gameObject.GetComponent<ResonatableObject>());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, new Vector3(3f, 0.01f, 3f));
        Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
        Gizmos.DrawCube(transform.position + minHeight * Vector3.up, new Vector3(3f, 0.01f, 3f));
        Gizmos.DrawCube(transform.position + maxHeight * Vector3.up, new Vector3(3f, 0.01f, 3f));
    }
    
    private void PlayCollapseSound()
    {
        GameManager.sm.PlaySound("collapse", 2f);
    }
}


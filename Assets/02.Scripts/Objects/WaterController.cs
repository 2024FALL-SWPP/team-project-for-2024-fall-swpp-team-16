using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(BoxCollider))]
public class WaterController : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] private float minWaterLevel = 0.1f; // Minimum height of water
    [SerializeField, Range(0f, 10f)] private float maxWaterLevel = 1f; // Maximum height of water
    [SerializeField, Range(0f, 10f)] private float initialWaterLevel = 0.5f; // Original height of water
    
    [SerializeField, Range(0f, 10f)] private float waterSurfaceSizeX = 1f; // X-Size of water surface
    [SerializeField, Range(0f, 10f)] private float waterSurfaceSizeZ = 1f; // Z-Size of water surface
    [Tooltip("Water Surface Object (Must be child of this GameObject)")]
    [SerializeField] private Transform waterSurface;
    [Tooltip("Ratio of exposed water surface area to the top surface area of the bounds.\n" +
             "The smaller the value, the more sensitive the water level.")]
    [SerializeField, Range(0.01f, 1f)] private float exposedSurfaceRatio = 0.2f;
    [SerializeField] private bool interactable = true;
    [SerializeField] private List<Transform> flotages = new();
    
    [SerializeField, Range(0f, 3f)] private float smoothTime = 0.5f; // Approximately the time it will take to reach the target water level
    private Vector3 _currentVelocity = Vector3.zero;
    
    private BoxCollider _waterCollider;
    private PlayerMovement _playerMovement;
    private WaterSinkHandler _waterSinkHandler;
    
    [HideInInspector] public float currentWaterLevel;
    
    public Bounds Bounds => _waterCollider.bounds;
    public float InitialWaterLevel => initialWaterLevel;
    public float ExposedSurfaceRatio => exposedSurfaceRatio;

    public void AdjustWaterLevel(float heightDelta)
    {
        initialWaterLevel += heightDelta;
        initialWaterLevel = Mathf.Clamp(initialWaterLevel, minWaterLevel, maxWaterLevel);
    }
    
    // Reflect changes to inspector values directly in the Editor Scene
    private void OnValidate()
    {
        Assert.IsNotNull(waterSurface, "Water Surface Object must be assigned");
        Assert.IsTrue(waterSurface.IsChildOf(transform), "Water Surface Object must be child of this GameObject");
        Assert.IsNotNull(waterSurface.GetComponent<Renderer>(), "Water Surface must have renderer component");
        minWaterLevel = Mathf.Clamp(minWaterLevel, 0f, maxWaterLevel);
        initialWaterLevel = Mathf.Clamp(initialWaterLevel, minWaterLevel, maxWaterLevel);
        InspectorDependentInit();
    }

    private void Awake()
    {
        _waterSinkHandler = new WaterSinkHandler(this);
        InspectorDependentInit();
    }
    
    private void InspectorDependentInit()
    {
        _waterCollider = gameObject.GetComponent<BoxCollider>();
        currentWaterLevel = initialWaterLevel;
        SetWaterSurfacePosition();
        SetWaterSurfaceSize();
        SetWaterCollider();
        SetFlotagePositions();
    }

    private void Start()
    {
        _playerMovement = GameManager.gm.controller.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckPlayerCollision();
        SetFlotagePositions();
        if (interactable) _waterSinkHandler.Handle();
        SmoothApplyWaterSurfacePosition();
    }

    private void SetFlotagePositions()
    {
        foreach (var flotage in flotages)
        {
            var flattenPosition = flotage.position;
            flattenPosition.y = waterSurface.position.y;
            flotage.position = flattenPosition;
        }
    }

    // Check for collisions with player
    // Respawn if they fall below a certain depth
    private void CheckPlayerCollision()
    {
        const int maxDetectCounts = 10;
        var playerPosition = GameManager.gm.controller.position;
        var sinkHeight = playerPosition.y - waterSurface.position.y;
        var waterLayer = 1 << LayerMask.NameToLayer("Water");
        var results = new Collider[maxDetectCounts];
        Physics.OverlapSphereNonAlloc(playerPosition, 3f, results, waterLayer);
        if (Array.IndexOf(results, _waterCollider) > -1 && sinkHeight < -0.5f)
        {
            _playerMovement.Drown();
        }
    }

    private void SmoothApplyWaterSurfacePosition()
    {
        currentWaterLevel = Mathf.Clamp(currentWaterLevel, minWaterLevel, maxWaterLevel);
        var waterSurfacePosition = Vector3.zero;
        waterSurfacePosition.y = currentWaterLevel;
        waterSurface.localPosition = Vector3.SmoothDamp(
            waterSurface.localPosition,
            waterSurfacePosition,
            ref _currentVelocity,
            smoothTime);
    }
    
    private void SetWaterSurfacePosition()
    {
        currentWaterLevel = Mathf.Clamp(currentWaterLevel, minWaterLevel, maxWaterLevel);
        var waterSurfacePosition = Vector3.zero;
        waterSurfacePosition.y = currentWaterLevel;
        waterSurface.localPosition = waterSurfacePosition;
    }

    private void SetWaterSurfaceSize()
    {
        var waterSurfaceSize = Vector3.one;
        waterSurfaceSize.x = waterSurfaceSizeX;
        waterSurfaceSize.z = waterSurfaceSizeZ;
        waterSurface.localScale = waterSurfaceSize;
    }
    
    private void SetWaterCollider()
    {
        var bounds = waterSurface.GetComponent<Renderer>().bounds;
        var center = Vector3.up * maxWaterLevel / 2;
        var size = new Vector3(bounds.size.x, maxWaterLevel, bounds.size.z);
        _waterCollider.center = center;
        _waterCollider.size = size;
        _waterCollider.isTrigger = true;
    }

    // Rock 오브젝트만 물에 빠졌을 때 상호작용함
    // 임시로 사용, 리팩토링 시 필요할 경우 Interactable에서 Strategy 패턴 적용 
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent<RockController>(out _))
            _waterSinkHandler.AddSinkingTarget(other.transform);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.TryGetComponent<RockController>(out _))
            _waterSinkHandler.RemoveSinkingTarget(other.transform);
    }
    
    // Debugging, Visualize the range of movement of a water surface
    private void OnDrawGizmos()
    {
        var bounds = waterSurface.GetComponent<Renderer>().bounds;
        var center = Vector3.up * (maxWaterLevel + minWaterLevel) / 2;
        var size = new Vector3(bounds.size.x, maxWaterLevel - minWaterLevel, bounds.size.z);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + center, size);
    }
}


using System;
using UnityEngine;
using UnityEngine.Serialization;

public class RockController : MonoBehaviour
{
    public bool isRolling; // Flag to check if the rock is rolling

    private const string PlayerTag = "Player"; // Tag of the player to follow
    private GameObject _player; // Reference to the player object
    
    [Range(0f, 2f)]
    [SerializeField] private float groundToCenterHeight = 1f; // Height above ground to maintain
    [Range(0f, 0.1f)]
    [SerializeField] private float approachDistance = 3f; // The distance at which the object stops approaching the player
    [SerializeField] private LayerMask groundLayer; // Layer mask to specify ground layer
    
    [Range(0f, 10f)]
    public float moveSpeed = 3f;
    private readonly float _moveSmoothTime = 0.6f;
    private Vector3 _currentVelocity = Vector3.zero;
    private Vector3 _targetPosition;
    
    private void Awake()
    {
        ResonatableObject resonatable = gameObject.AddComponent<ResonatableObject>();
        resonatable.properties = new[] { PitchType.Mi, PitchType.Fa };
        resonatable.resonate += RockResonate;
    }

    private void RockResonate(PitchType pitch)
    {
        switch (pitch)
        {
            case PitchType.Mi: { MoveToPlayer(); break; }
            case PitchType.Fa: { MoveAwayFromPlayer(); break; }
        }
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag(PlayerTag);
        _targetPosition = transform.position;
        StickToGround();
    }

    // Update is called once per frame
    void Update()
    {
        HandleCollision();

        Vector3 currentPosition = transform.position;
        transform.position = Vector3.SmoothDamp(currentPosition, _targetPosition, ref _currentVelocity, _moveSmoothTime);
        
        if (isRolling) // rotate rock if it is rolling
        {
            RollRock(_currentVelocity);
        }

        StickToGround();
    }

    private void RollRock(Vector3 velocity)
    {
        // Move and rotate the rock
        Vector3 direction = velocity.normalized;
        float angle = (velocity.magnitude / groundToCenterHeight) * Time.deltaTime * Mathf.Rad2Deg;
        transform.Rotate(Vector3.Cross(Vector3.up, direction), angle, Space.World);
    }

    private void StickToGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            float groundHeight = hit.point.y;
            if (!Mathf.Approximately(transform.position.y, groundHeight + groundToCenterHeight))
            {
                transform.position = new Vector3(transform.position.x, groundHeight + groundToCenterHeight, transform.position.z);
                _targetPosition.y = transform.position.y;
            }
        }
    }

    private void MoveToPlayer()
    {
        if (_player is null) return;
        if (Vector3.Distance(_player.transform.position, _targetPosition) < approachDistance) return;
        var directionToPlayer = (_player.transform.position - transform.position).normalized;
        _targetPosition += directionToPlayer * (moveSpeed * Time.deltaTime);
    }

    private void MoveAwayFromPlayer()
    {
        if (_player is null) return;
        var directionAwayFromPlayer = (transform.position - _player.transform.position).normalized;
        _targetPosition += directionAwayFromPlayer * (moveSpeed * Time.deltaTime);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.contacts);
    }
    
    private void HandleCollision()
    {
        Vector3 moveDirection = _currentVelocity.normalized;
        float checkDistance = 10f;
        // Inspection resolution
        int numStep = 10;

        for (int i = -numStep; i <= numStep; i++)
        {
            float checkAngle = 90f * i / numStep;
            Vector3 checkDirection = Quaternion.AngleAxis(checkAngle, Vector3.up) * moveDirection;
            Ray outRay = new Ray(transform.position, checkDirection);
            if (Physics.Raycast(outRay, out RaycastHit hit, checkDistance, groundLayer))
            {
                Ray inRay = new Ray(hit.point, -checkDirection);
                float hitDistance = hit.distance;
                Physics.Raycast(inRay, out hit, hitDistance, groundLayer);
                
                if (hit.transform != transform)
                {
                    _currentVelocity = Vector3.zero;
                    _targetPosition = transform.position;
                }
            }
        }
    }
}

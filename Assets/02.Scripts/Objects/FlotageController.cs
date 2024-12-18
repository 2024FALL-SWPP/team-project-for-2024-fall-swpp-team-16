using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions;

public class FlotageController : MonoBehaviour
{
    private const string PlayerTag = "Player"; // Tag of the player to follow
    
    [Range(0f, 10f)]
    [Tooltip("movement speed at resonance")]
    public float moveSpeed = 3f;
    
    private GameObject _player; // Reference to the player object
    private Rigidbody _rigidbody; // Rigidbody component
    private Vector3 _velocityInput;
    private Vector3 _currentVelocity;
    
    private void Awake()
    {
        gameObject.AddComponent<PhysicsImmunity>();
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        Assert.IsNotNull(_rigidbody);
        ResonatableObject resonatable = gameObject.AddComponent<ResonatableObject>();
        resonatable.properties = new[] { PitchType.Mi, PitchType.Fa };
        resonatable.resonate += FlotageResonate;
    }

    private void FlotageResonate(PitchType pitch)
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
    }

    void Update()
    {
        // FlattenPosition();
        _currentVelocity = _velocityInput;
        _velocityInput = Vector3.zero;
    }

    private void FixedUpdate()
    {
        _rigidbody.velocity = _currentVelocity;
    }

    // Anchor to the water surface
    private void FlattenPosition()
    {
        if (Mathf.Approximately(transform.localPosition.y, 0f)) return;
        Vector3 flattenPosition = transform.localPosition;
        flattenPosition.y = 0f;
        transform.localPosition = flattenPosition;
    }
    
    private void MoveToPlayer()
    {
        if (_player is null) return;
        var directionToPlayer = _player.transform.position - transform.position;
        directionToPlayer.y = 0;
        directionToPlayer.Normalize();
        // _rigidbody.velocity = directionToPlayer * moveSpeed;
        _velocityInput = directionToPlayer * moveSpeed;
    }

    private void MoveAwayFromPlayer()
    {
        if (_player is null) return;
        var directionAwayFromPlayer = transform.position - _player.transform.position;
        directionAwayFromPlayer.y = 0;
        directionAwayFromPlayer.Normalize();
        // _rigidbody.velocity = directionAwayFromPlayer * moveSpeed;
        _velocityInput = directionAwayFromPlayer * moveSpeed;
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput
{
    public float moveX;
    public float moveZ;
    public bool jump;
    public bool run;
    public bool active = true;
    
    public void Clear()
    {
        moveX = 0f;
        moveZ = 0f;
        jump = false;
        run = false;
    }
}

public class PlayerMovement : MonoBehaviour
{
    [Range(1f, 20f)]
    [SerializeField] private float _movementSpeed;
    
    [Tooltip("additional speed multiplier when running")]
    [Range(1f, 20f)]
    [SerializeField] private float _runMultiplier;

    [Tooltip("time taken to rotate when the direction of movement is changed")]
    [Range(0f, 0.1f)]
    [SerializeField] private float _smoothTime = 0.1f;

    private SurfaceContactController playerController;
    private Rigidbody playerRigidBody;
    private Collider playerCollider;
    private Transform playerTransform;
    private Animator playerAnimator;
    private Renderer playerMeshRenderer;
    private Vector3 _controllerVelocity;
    private Vector3 _lastStablePosition;
    
    private PlayerInput input;
    
    private bool immune = false;
    private float respawnDelayTime = 0.5f;
    private float respawnPositionRestoreTime = 1f;
    private int respawnBlinkCount = 5;
    private float respawnBlinkTime = 0.1f;
    
    private int runLayer = 1;
    private float runLayerWeight = 0f;
    private float runTransitionSpeed = 3f;

    // Start is called before the first frame update
    private void Awake()
    {
        input = new PlayerInput();
        playerRigidBody = GetComponent<Rigidbody>();
        playerController = GetComponent<SurfaceContactController>();
        playerCollider = GetComponent<Collider>();
        playerTransform = transform.Find("Player");
        playerAnimator = playerTransform.GetComponent<Animator>();
        playerMeshRenderer = playerTransform.GetComponentInChildren<Renderer>();
        _lastStablePosition = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        GetInputs();
        ControlPlayer();
    }

    private void GetInputs()
    {
        input.Clear();
        if(!input.active) return;
        
        // get the movement input
        input.moveX = Input.GetAxis("Horizontal");
        input.moveZ = Input.GetAxis("Vertical");
        input.jump = Input.GetButton("Jump");
        input.run = Input.GetKey(KeyCode.LeftShift);
    }

    private void ControlPlayer()
    {
        if (immune) return;
        
        CheckStable();
        
        // moves the controller in the desired direction on the x- and z-axis
        Vector3 movement = transform.right * input.moveX + transform.forward * input.moveZ;
        movement *= _movementSpeed;

        // the controller is able to run
        if (input.run && playerController.Grounded)
        {
            movement *= _runMultiplier;

            if(playerAnimator.GetBool("Grounded"))
            {
                runLayerWeight = Mathf.MoveTowards(runLayerWeight, 1f, Time.deltaTime * runTransitionSpeed);
            }
        }
        else
        {
            runLayerWeight = Mathf.MoveTowards(runLayerWeight, 0f, Time.deltaTime * runTransitionSpeed);
        }
        playerAnimator.SetLayerWeight(runLayer, runLayerWeight);
        
        float currentVelocity = movement.magnitude;
        
        // player movement is only controllable while being grounded
        playerController.Velocity = movement;
        playerAnimator.SetBool("Moving", currentVelocity > 0);
        if (currentVelocity > 0)
        {
            // set player's forward same as moving direction
            float targetAngle = Mathf.Atan2(input.moveX, input.moveZ) * Mathf.Rad2Deg - 90;
            float angle = Mathf.SmoothDampAngle(playerTransform.eulerAngles.y, targetAngle, ref currentVelocity, _smoothTime);
            playerTransform.rotation = Quaternion.Euler(0, angle, 0);
        }

        playerAnimator.SetBool("Grounded", playerController.Grounded);
        
        // the controller is able to jump when on the ground
        if (input.jump && playerController.Grounded)
        {
            playerAnimator.SetBool("Jump", true);
            playerAnimator.SetBool("Grounded", false);
            playerController.Jump = true;
        }
        else
        {
            playerAnimator.SetBool("Jump", false);
        }
    }

    // Convert the direction of movement to avoid entering a impassible area
    private void CheckStable()
    {
        Debug.DrawRay(_lastStablePosition, Vector3.up, Color.blue);
        if (!playerController.Grounded) return;
        
        // Restricted distance for impassable areas
        float checkDistance = 2f;
        // Inspection resolution
        int numStep = 20;
        
        for (int i = 0; i < numStep; i++)
        {
            float checkAngle = 360f * i / numStep;
            Vector3 checkDirection = Quaternion.AngleAxis(checkAngle, Vector3.up) * Vector3.forward;
            if (!CheckPassable(transform.position + checkDirection * checkDistance)) return;
        }

        _lastStablePosition = transform.position;
    }

    // Check if a location is passable
    private bool CheckPassable(Vector3 position)
    {
        // Subject to modification based on development
        Ray ray = new Ray(position + Vector3.up * 1.5f, Vector3.down);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return false;
        return !hit.transform.CompareTag("Water");
    }

    public void Drown()
    {
        // 플레이어가 물에 빠지는 이펙트 발생 필요
        if (immune) return;
        immune = true;
        StartCoroutine(RespawnPlayer());
    }
    
    private IEnumerator RespawnPlayer()
    {
        input.active = false;
        playerMeshRenderer.enabled = false;
        playerRigidBody.isKinematic = true;
        playerRigidBody.velocity = Vector3.zero;
        playerCollider.enabled = false;
        yield return new WaitForSeconds(respawnDelayTime);
        playerAnimator.SetBool("Grounded", true);
        playerAnimator.SetBool("Moving", false);
        yield return MoveSmooth(transform.position, _lastStablePosition, respawnPositionRestoreTime);
        yield return BlinkPlayer(respawnBlinkCount, respawnBlinkTime);
        Input.ResetInputAxes();
        input.active = true;
        playerMeshRenderer.enabled = true;
        playerRigidBody.isKinematic = false;
        playerCollider.enabled = true;
        immune = false;
    }
    
    // Smoothly move from startPos to endPos over finishTime
    private IEnumerator MoveSmooth(Vector3 startPos, Vector3 endPos, float finishTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < finishTime)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / finishTime;
            transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, normalizedTime));
            yield return null;
        }
    }

    public IEnumerator WalkToPoint(Vector3 targetPosition, float duration)
    {
        yield return StartCoroutine(MoveSmooth(transform.position, targetPosition, duration));
    }
    
    // Repeat blinking for the specified time count times
    private IEnumerator BlinkPlayer(int count, float time)
    {
        for (int i=0; i<count; i++)
        {
            playerMeshRenderer.enabled = false;
            yield return new WaitForSeconds(time);
            playerMeshRenderer.enabled = true;
            yield return new WaitForSeconds(time);
        }
    }
}

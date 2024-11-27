using System;
using System.Collections;
using UnityEngine;

namespace _12.Tests.PlayerDev
{
    public class PlayerInput
    {
        public float moveX;
        public float moveZ;
        public bool jump;
        public bool run;
        
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
        
        [Tooltip("run multiplier of the movement speed")]
        [Range(1f, 20f)]
        [SerializeField] private float _runMultiplier;
        
        [SerializeField] private float _gravity = -9.81f;
        [Range(0f, 3f)]
        [SerializeField] private float _jumpHeight;
        
        [Tooltip("time taken to rotate when the direction of movement is changed")]
        [Range(0f, 0.1f)]
        [SerializeField] private float _smoothTime = 0.1f;

        private CharacterController characterController;
        private Transform playerTransform;
        private Animator playerAnimator;
        private Vector3 _controllerVelocity;
        
        private PlayerInput input;
        
        private IEnumerator jumpCheckGroundAvoider; 
        private IEnumerator JumpCheckGroundAvoider()
        {
            yield return new WaitForFixedUpdate();
            playerAnimator.SetBool("Jump", false);
        }
        
        private int runLayer = 1;
        private float runLayerWeight = 0f;
        private float runTransitionSpeed = 3f;

        // Start is called before the first frame update
        private void Start()
        {
            input = new PlayerInput();
            characterController = GetComponent<CharacterController>();
            playerTransform = transform.Find("Player");
            playerAnimator = playerTransform.GetComponent<Animator>();
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
            
            // get the movement input
            input.moveX = Input.GetAxis("Horizontal");
            input.moveZ = Input.GetAxis("Vertical");
            input.jump = Input.GetButton("Jump");
            input.run = Input.GetKey(KeyCode.LeftShift);
        }

        private void ControlPlayer()
        {
            // stops the y velocity when player is on the ground and the velocity has reached 0
            if (characterController.isGrounded && _controllerVelocity.y < 0)
            {
                _controllerVelocity.y = 0;
            }

            // moves the controller in the desired direction on the x- and z-axis
            Vector3 movement = transform.right * input.moveX + transform.forward * input.moveZ;
            movement = PassableComponent(movement);
            characterController.Move(movement * (_movementSpeed * Time.deltaTime));

            // set player's forward same as moving direction
            float currentVelocity = movement.magnitude;
            playerAnimator.SetBool("Moving", currentVelocity > 0);
            if (currentVelocity > 0)
            {
                float targetAngle = Mathf.Atan2(input.moveX, input.moveZ) * Mathf.Rad2Deg - 90;
                float angle = Mathf.SmoothDampAngle(playerTransform.eulerAngles.y, targetAngle, ref currentVelocity, _smoothTime);
                playerTransform.rotation = Quaternion.Euler(0, angle, 0);
            }
                
            // gravity affects the controller on the y-axis
            _controllerVelocity.y += _gravity * Time.deltaTime;

            // moves the controller on the y-axis
            characterController.Move(_controllerVelocity * Time.deltaTime);
            
            if(characterController.isGrounded) playerAnimator.SetBool("Grounded", true);

            // the controller is able to jump when on the ground
            if (input.jump && characterController.isGrounded)
            {
                playerAnimator.SetBool("Jump", true);
                playerAnimator.SetBool("Grounded", false);
                _controllerVelocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                
                if(jumpCheckGroundAvoider != null)
                {
                    StopCoroutine(jumpCheckGroundAvoider);
                }
                jumpCheckGroundAvoider = JumpCheckGroundAvoider();
                StartCoroutine(jumpCheckGroundAvoider);
            }

            // the controller is able to run
            if (input.run)
            {
                characterController.Move(movement * (Time.deltaTime * _runMultiplier));

                if(playerAnimator.GetBool("Grounded"))
                {
                    runLayerWeight = Mathf.MoveTowards(runLayerWeight, 1f, Time.deltaTime * runTransitionSpeed);
                }
            }else{
                runLayerWeight = Mathf.MoveTowards(runLayerWeight, 0f, Time.deltaTime * runTransitionSpeed);
            }
            playerAnimator.SetLayerWeight(runLayer, runLayerWeight);
        }

        // Convert the direction of movement to avoid entering a impassible area
        private Vector3 PassableComponent(Vector3 movement)
        {
            if (movement.magnitude == 0) return movement;
            Vector3 moveDirection = movement.normalized;
            // Restricted distance for impassable areas
            float checkDistance = characterController.radius * 2f;
            // Inspection resolution
            int numStep = 10;

            // Stop if impassable to the move direction
            if (!CheckPassable(transform.position + moveDirection * checkDistance)) return Vector3.zero;
            
            // Scans a semicircular area in front of the player
            Vector3 leftPassibleDirection = movement;
            Vector3 rightPassibleDirection = movement;
            
            for (int i = 1; i < numStep; i++)
            {
                float checkAngle = 90f * i / numStep;
                Vector3 checkDirection = Quaternion.AngleAxis(checkAngle, Vector3.up) * moveDirection;
                if (!CheckPassable(transform.position + checkDirection * checkDistance)) break;
                leftPassibleDirection = checkDirection;
            }
            for (int i = 1; i < numStep; i++)
            {
                float checkAngle = -90f * i / numStep;
                Vector3 checkDirection = Quaternion.AngleAxis(checkAngle, Vector3.up) * moveDirection;
                if (!CheckPassable(transform.position + checkDirection * checkDistance)) break;
                rightPassibleDirection = checkDirection;
            }

            // Adjust the direction to bias it toward passable areas
            Vector3 newDirection = (leftPassibleDirection + rightPassibleDirection).normalized;
            return Vector3.Project(movement, newDirection);
        }

        // Check if a location is passable
        private bool CheckPassable(Vector3 position)
        {
            // Subject to modification based on development
            // Can also be used for slope limiting
            Ray ray = new Ray(position + Vector3.up * 0.5f, Vector3.down);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return false;
            return !hit.transform.CompareTag("Water");
        }
    }
}

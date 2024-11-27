using UnityEngine;
using System.Collections;

public enum AnimalState
{
    Idle,
    Rush,
    FollowPath
}

public class AnimalController : MonoBehaviour
{
    // Public variables for settings
    public AnimalState currentState = AnimalState.Idle;
    public float idleSpeed = 2f;
    public float rushSpeed = 15f;
    public float rushDuration = 3f;
    public ParticleSystem rushParticleEffect;
    public AnimalState baseState = AnimalState.Idle;
    public Transform[] pathPoints;

    // Private variables
    private Animator animator;
    private int currentPathIndex = 0;
    private bool isRushing = false;
    private float rushTimer = 0f;
    
    private void Awake()
    {
        ResonatableObject resonatable = gameObject.AddComponent<ResonatableObject>();
        resonatable.properties = new[] { PitchType.Do };
        resonatable.resonate += AnimalResonate;
    }

    private void AnimalResonate(PitchType pitch)
    {
        switch (pitch)
        {
            case PitchType.Do: { TriggerRush(); break; }
        }
    }

    void Start()
    {
        // Get the Animator component
        animator = GetComponent<Animator>();

        // Start the initial behavior based on the current state
        if (currentState == AnimalState.Idle)
        {
            StartCoroutine(IdleBehavior());
        }
        else if (currentState == AnimalState.FollowPath)
        {
            StartCoroutine(FollowPathBehavior());
        }
    }

    /*
    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha4))
        {
            if (currentState != AnimalState.Rush)
            {
                TriggerRush();
            }
    
            // Move forward while rushing
            transform.Translate(Vector3.forward * rushSpeed * Time.deltaTime);
        }
        else if (isRushing)
        {
            // Maintain the Rush state without resetting until the duration ends
            return;
        }
        else
        {
            // Reset to base state when not rushing
            animator.SetBool("isRushing", false);
            currentState = baseState;
    
            if (rushParticleEffect != null)
            {
                rushParticleEffect.Stop();
            }
        }
    }
    
    public void TriggerRush()
    {
        if (currentState != AnimalState.Rush)
        {
            Debug.Log("TriggerRush called.");
            
            // Change state to Rush
            currentState = AnimalState.Rush;
            isRushing = true;
            rushTimer = 0f;
    
            // Stop other behaviors to prevent conflicts
            StopAllCoroutines();
    
            // Play particle effect if available
            if (rushParticleEffect != null)
            {
                rushParticleEffect.Play();
            }
    
            // Set animator parameter
            animator.SetBool("isRushing", true);
    
            // Start coroutine to manage rush duration
            StartCoroutine(RushCoroutine());
        }
    }

    void StopRush()
    {
        isRushing = false;

        // Stop particle effect if playing
        if (rushParticleEffect != null)
        {
            rushParticleEffect.Stop();
        }

        // Reset animator parameter
        animator.SetBool("isRushing", false);

        // Return to previous behavior
        if (pathPoints != null && pathPoints.Length > 0)
        {
            currentState = AnimalState.FollowPath;
            StartCoroutine(FollowPathBehavior());
        }
        else
        {
            currentState = AnimalState.Idle;
            StartCoroutine(IdleBehavior());
        }
    } */

    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha4)) // Change to KeyCode.Alpha1 if needed
        {
            if (currentState != AnimalState.Rush)
            {
                TriggerRush();
            }
    
            // Move forward while rushing
            transform.Translate(Vector3.forward * rushSpeed * Time.deltaTime);
        }
        else
        {
            if (isRushing)
            {
                StopRush(); // Stop rushing when the key is released
            }
            else
            {
                // Reset to base state when not rushing
                animator.SetBool("isRushing", false);
                currentState = baseState;
    
                if (rushParticleEffect != null)
                {
                    rushParticleEffect.Stop();
                }
            }
        }
    }
    
    public void TriggerRush()
    {
        if (currentState != AnimalState.Rush)
        {
            Debug.Log("TriggerRush called.");
    
            // Change state to Rush
            currentState = AnimalState.Rush;
            isRushing = true;
    
            // Stop other behaviors to prevent conflicts
            StopAllCoroutines();
    
            // Play particle effect if available
            if (rushParticleEffect != null)
            {
                rushParticleEffect.Play();
            }
    
            // Set animator parameter
            animator.SetBool("isRushing", true);
        }
    }
    
    void StopRush()
    {
        isRushing = false;
    
        // Stop particle effect if playing
        if (rushParticleEffect != null)
        {
            rushParticleEffect.Stop();
        }
    
        // Reset animator parameter
        animator.SetBool("isRushing", false);
    
        // Return to the previous behavior
        if (pathPoints != null && pathPoints.Length > 0)
        {
            currentState = AnimalState.FollowPath;
            StartCoroutine(FollowPathBehavior());
        }
        else
        {
            currentState = AnimalState.Idle;
            StartCoroutine(IdleBehavior());
        }
    }


    IEnumerator IdleBehavior()
    {
        while (currentState == AnimalState.Idle)
        {   animator.SetBool("isMoving", false);
            // Randomly select an idle animation
            int randomIndex = Random.Range(0, 3);

            switch (randomIndex)
            {
                case 0:
                    // Play turn left animation
                    animator.Play("turn_90_L");
                    yield return new WaitForSeconds(2f);
                    break;

                case 1:
                    // Play sit and stand animations
                    animator.Play("sit_to_stand");
                   // yield return new WaitForSeconds(2f);
                    animator.Play("stand_to_sit");
                    yield return new WaitForSeconds(8f);
                    break;

                case 2:
                    // Play turn right animation
                    animator.Play("turn_90_R");
                    yield return new WaitForSeconds(2f);
                    break;
            }

            // Wait before performing the next idle action
            yield return new WaitForSeconds(Random.Range(2f, 5f));
        }
    }

    IEnumerator FollowPathBehavior()
    {
        while (currentState == AnimalState.FollowPath)
        {
            if (pathPoints.Length == 0)
                yield break;

            // Get the next waypoint
            Vector3 targetPosition = pathPoints[currentPathIndex].position;
            targetPosition.y = transform.position.y; // Keep the same height

            // Rotate towards the target
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.5f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, idleSpeed * 100 * Time.deltaTime);
                yield return null;

                if (currentState != AnimalState.FollowPath)
                    yield break;
            }

            // Start walking animation
            animator.SetBool("isMoving", true);

            // Move towards the waypoint
            while (Vector3.Distance(transform.position, targetPosition) > 0.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, idleSpeed * Time.deltaTime);
                yield return null;

                if (currentState != AnimalState.FollowPath)
                    yield break;
            }

            // Stop walking animation
            // animator.SetBool("isMoving", false);

            // Move to the next waypoint
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;

            // Wait before moving to the next waypoint
            yield return new WaitForSeconds(Random.Range(0.01f, 0.5f));
        }
    }
    /*

    private void OnCollisionEnter(Collision collision)
    {
        if (currentState == AnimalState.Rush && isRushing && collision.gameObject.CompareTag("Trees") ) // 다른 옵젝트와 부딪힘 , 태그 수정 필요
        {
            // Stop rushing upon collision
            StopRush();
        }
    }*/
}

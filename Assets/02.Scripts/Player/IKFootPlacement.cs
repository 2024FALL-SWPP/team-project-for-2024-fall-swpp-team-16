using System;
using System.Collections;
using System.Collections.Generic;
using _12.Tests.PlayerDev;
using UnityEditor.IMGUI.Controls;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    // Distance from Foot (ankle) to floor
    [Range(0f, 1f)]
    [SerializeField] private float _distanceToGround = 0.1f;
    // Layers to cast IK to (including Walkables, but excluding the Player itself)
    [SerializeField] private LayerMask _raycastLayerMask;
    
    [SerializeField] private AnimationCurve _leftFootWalkIKCurve;
    [SerializeField] private AnimationCurve _rightFootWalkIKCurve;
    [SerializeField] private AnimationCurve _leftFootRunIKCurve;
    [SerializeField] private AnimationCurve _rightFootRunIKCurve;
    
    private Animator playerAnimator;
    private int runLayer = 1;
    
    private void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!playerAnimator) return;
        
        // IK only works when the foot touches the ground
        bool moving = playerAnimator.GetBool("Moving");
        bool grounded = playerAnimator.GetBool("Grounded");
        
        float nTime = playerAnimator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;
        float runWeight = playerAnimator.GetLayerWeight(runLayer);
        float leftMovingWeight = Mathf.Lerp(_leftFootWalkIKCurve.Evaluate(nTime), _leftFootRunIKCurve.Evaluate(nTime), runWeight);
        float rightMovingWeight = Mathf.Lerp(_rightFootWalkIKCurve.Evaluate(nTime), _rightFootRunIKCurve.Evaluate(nTime), runWeight);
        
        float leftFootIKWeight = grounded ? (moving ? leftMovingWeight : 1f) : 0f;
        float rightFootIKWeight = grounded ? (moving ? rightMovingWeight : 1f) : 0f;
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootIKWeight);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootIKWeight);
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightFootIKWeight);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightFootIKWeight);
        
        float detectDistance = 0.5f;
        
        // Raising the origin so that it can be detected even when the foot is buried
        transform.localPosition = Vector3.zero;
        Ray leftRay = new Ray(playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
        Ray rightRay = new Ray(playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
        bool leftIntersected = Physics.Raycast(leftRay, out RaycastHit leftHit, _distanceToGround + detectDistance + 1f,
            _raycastLayerMask);
        bool rightIntersected = Physics.Raycast(rightRay, out RaycastHit rightHit, _distanceToGround + detectDistance + 1f,
            _raycastLayerMask);
        
        if (leftIntersected && rightIntersected && grounded)
        {
            // Staying low on slopes where it's hard to reach your feet
            float lowestHeight = Mathf.Min(leftHit.point.y, rightHit.point.y);
            float originHeight = transform.parent.position.y;
            Vector3 loweredPosition = new Vector3(0f, lowestHeight - originHeight, 0f);
            transform.localPosition = loweredPosition;
            
            // if (leftHit.transform.tag == "Walkable")
            Vector3 leftFootPosition = leftHit.point;
            leftFootPosition.y += _distanceToGround;
            playerAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPosition);
            playerAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, leftHit.normal));
            Debug.DrawRay(leftFootPosition, leftHit.normal, Color.green);
            
            // if (rightHit.transform.tag == "Walkable")
            Vector3 rightFootPosition = rightHit.point;
            rightFootPosition.y += _distanceToGround;
            playerAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPosition);
            playerAnimator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, rightHit.normal));
            Debug.DrawRay(rightFootPosition, rightHit.normal, Color.green);
        }
    }
}

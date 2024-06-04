using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class SwingingArmMotion : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    [SerializeField] private GameObject leftHand, rightHand;

    [SerializeField] private InputActionReference leftHandTrigger,rightHandTrigger;

    [SerializeField] private float speed = 20;

    private Vector3 previousPosLeft, previousPosRight, direction;

    private void Start()
    {
        previousPosLeft = leftHand.transform.position;
        previousPosRight = rightHand.transform.position;
    }

    private void Update()
    {
        float leftTriggerPress = leftHandTrigger.action.ReadValue<float>();
        float rightTriggerPress = rightHandTrigger.action.ReadValue<float>(); ;
        if (leftTriggerPress == 1)
        {
            Vector3 leftHandVelocity = leftHand.transform.position - previousPosLeft;
            if (leftHandVelocity.magnitude >= 0.05f)
            {
                direction = -leftHandVelocity;
                characterController.Move(speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up));
            }
        }else if (rightTriggerPress == 1)
        {
            Vector3 rightHandVelocity = rightHand.transform.position - previousPosRight;
            if (rightHandVelocity.magnitude >= 0.05f)
            {
                direction = -rightHandVelocity;
                characterController.Move(speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up));
            }
        }


        previousPosLeft = leftHand.transform.position;
        previousPosRight = rightHand.transform.position;
    }
}

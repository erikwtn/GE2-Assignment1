using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

// TODO: Path following steering behaviour for enemy ai
public class SpaceshipController : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float downAndUpSpeed;
    [SerializeField] private float speed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Camera cam;

    private InputAction _rollAction;
    private InputAction _upDownAction;
    private InputAction _horizontalAction;
    private InputAction _verticalAction;
    private InputAction _leftGripAction;
    private InputAction _rightGripAction;
    private InputAction _fireAction;

    private void Start()
    {
        _rollAction = new InputAction(binding: "<XRController>/roll");
        _upDownAction = new InputAction(binding: "<XRController>/primary2DAxis/y");
        _horizontalAction = new InputAction(binding: "<XRController>/primary2DAxis/x");
        //_verticalAction = new InputAction(binding: "<XRController>/trigger");
        _leftGripAction = new InputAction(binding: "<XRController>/gripButton");
        _rightGripAction = new InputAction(binding: "<XRController>/secondaryButton");
        _fireAction = new InputAction(binding: "<XRController/triggerButton>");
        
        _rollAction.Enable();
        _upDownAction.Enable();
        _horizontalAction.Enable();
        //_verticalAction.Enable();
        _leftGripAction.Enable();
        _rightGripAction.Enable();
        _fireAction.Enable();
    }

    private void FixedUpdate()
    {
        var rollInput = _rollAction.ReadValue<float>();
        var upDownInput = _upDownAction.ReadValue<float>();
        var horizontalInput = _horizontalAction.ReadValue<float>();
        //var verticalInput = _verticalAction.ReadValue<float>();
        var leftGripInput = _leftGripAction.ReadValue<float>();
        var rightGripInput = _rightGripAction.ReadValue<float>();
        var fireInput = _fireAction.ReadValue<float>();
        
        transform.Rotate(new Vector3(0, 0, -1) * (rotateSpeed * rollInput * Time.deltaTime));
        transform.Rotate(new Vector3(1, 0, 0) * (downAndUpSpeed * upDownInput * Time.deltaTime));
        transform.Rotate(new Vector3(0, 1, 0) * (turnSpeed * horizontalInput * Time.deltaTime));

        var moveInput = leftGripInput - rightGripInput;
        transform.Translate(Vector3.forward * (speed * moveInput * Time.deltaTime));

        if (fireInput > 0)
        {
            Debug.Log("Attack made");
        }
    }

    private void OnDisable()
    {
        _rollAction.Disable();
        _upDownAction.Disable();
        _horizontalAction.Disable();
        //_verticalAction.Disable();
        _leftGripAction.Disable();
        _rightGripAction.Disable();
        _fireAction.Disable();
    }
}
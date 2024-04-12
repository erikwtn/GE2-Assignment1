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
    private InputAction _fireAction;

    private void Start()
    {
        // ref
        _rollAction = new InputAction(binding: "<XRController>/roll");
        _upDownAction = new InputAction(binding: "<XRController>/primary2DAxis/y");
        _horizontalAction = new InputAction(binding: "<XRController>/primary2DAxis/x");
        _verticalAction = new InputAction(binding: "<XRController>/trigger");
        _fireAction = new InputAction(binding: "<XRController>/triggerButton");
        
        // Enable inpout actions
        _rollAction.Enable();
        _upDownAction.Enable();
        _horizontalAction.Enable();
        _verticalAction.Enable();
        _fireAction.Enable();
    }

    private void FixedUpdate()
    {
        // get valyes for input
        var rollInput = _rollAction.ReadValue<float>();
        var upDownInput = _upDownAction.ReadValue<float>();
        var horizontalInput = _horizontalAction.ReadValue<float>();
        var verticalInput = _verticalAction.ReadValue<float>();

        // rotate and translate when input
        transform.Rotate(new Vector3(0, 0, -1) * (rotateSpeed * rollInput * Time.deltaTime));
        transform.Rotate(new Vector3(1, 0, 0) * (downAndUpSpeed * upDownInput * Time.deltaTime));
        transform.Rotate(new Vector3(0, 1, 0) * (turnSpeed * horizontalInput * Time.deltaTime));
        transform.Translate(Vector3.forward * (speed * verticalInput * Time.deltaTime));

        if (_fireAction.triggered)
        {
            // accelerate spaceship
            transform.position += cam.transform.forward * (moveSpeed * Time.deltaTime);
        }
    }

    private void OnDisable()
    {
        // disable actions
        _rollAction.Disable();
        _upDownAction.Disable();
        _horizontalAction.Disable();
        _verticalAction.Disable();
        _fireAction.Disable();
    }
}
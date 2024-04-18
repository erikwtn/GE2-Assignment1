using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SpaceshipController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float downAndUpSpeed;
    [SerializeField] private float speed;
    [SerializeField] private float maxViewDist;
    
    [Header("Combat")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletOrigin;
    [SerializeField] private Transform bulletParent;
    [SerializeField] private float fireCooldown = 0.5f;

    private bool _canFire = true;
    
    [SerializeField] private Camera cam;
    private LayerMask _enemyLayer;

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
        _leftGripAction = new InputAction(binding: "<XRController>/gripButton");
        _rightGripAction = new InputAction(binding: "<XRController>/secondaryButton");
        _fireAction = new InputAction(binding: "<XRController>/triggerButton");
        
        _rollAction.Enable();
        _upDownAction.Enable();
        _horizontalAction.Enable();
        _leftGripAction.Enable();
        _rightGripAction.Enable();
        _fireAction.Enable();
    }

    private void FixedUpdate()
    {
        var rollInput = _rollAction.ReadValue<float>();
        var upDownInput = _upDownAction.ReadValue<float>();
        var horizontalInput = _horizontalAction.ReadValue<float>();
        var leftGripInput = _leftGripAction.ReadValue<float>();
        var rightGripInput = _rightGripAction.ReadValue<float>();
        var fireInput = _fireAction.ReadValue<float>();
        
        transform.Rotate(new Vector3(0, 0, -1) * (rotateSpeed * rollInput * Time.deltaTime));
        transform.Rotate(new Vector3(1, 0, 0) * (downAndUpSpeed * upDownInput * Time.deltaTime));
        transform.Rotate(new Vector3(0, 1, 0) * (turnSpeed * horizontalInput * Time.deltaTime));

        var moveInput = leftGripInput - rightGripInput;
        transform.Translate(Vector3.forward * (speed * moveInput * Time.deltaTime));

        if (fireInput > 0 && _canFire)
        {
            StartCoroutine(Fire());
        }
    }

    private EnemyAI FindNearestEnemy()
    {
        _enemyLayer = LayerMask.GetMask("Enemy");
        var colliders = Physics.OverlapSphere(transform.position, maxViewDist, _enemyLayer);
        EnemyAI enemyAI = null;
        var nearestDist = Mathf.Infinity;
        
        foreach (var c in colliders)
        {
            var dir = c.transform.position - transform.position;

            if (!(Vector3.Dot(transform.forward, dir.normalized) > 0)) continue;
            var dist = Vector3.Distance(transform.position, c.transform.position);

            if (!(dist < nearestDist)) continue;
            enemyAI = c.GetComponent<EnemyAI>();
            nearestDist = dist;
        }

        return enemyAI;
    }
    
    private IEnumerator Fire()
    {
        _canFire = false;
        
        var target = FindNearestEnemy();
        var newBullet = Instantiate(bulletPrefab, bulletOrigin.position, transform.rotation, bulletParent);
        var bullet = newBullet.GetComponent<Bullet>();
        
        if (bullet != null)
        {
            bullet.GetTarget(target != null ? target.transform : null);
        }

        yield return new WaitForSecondsRealtime(fireCooldown);
        _canFire = true;
    }

    private void OnDisable()
    {
        _rollAction.Disable();
        _upDownAction.Disable();
        _horizontalAction.Disable();
        _leftGripAction.Disable();
        _rightGripAction.Disable();
        _fireAction.Disable();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * maxViewDist);
    }
}
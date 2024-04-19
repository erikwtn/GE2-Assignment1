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
    private InputAction _horizontalAction;
    private InputAction _verticalAction;
    private InputAction _upDownAction;
    private InputAction _gripAction;
    //private InputAction _fireAction;

    private void Start()
    {
        _rollAction = new InputAction(binding: "<XRController>/roll");
        _horizontalAction = new InputAction(binding: "<XRController>/primary2DAxis/x");
        _verticalAction = new InputAction(binding: "<XRController>/primary2DAxis/y");
        _upDownAction = new InputAction(binding: "<XRController>/triggerButton");
        _gripAction = new InputAction(binding: "<XRController>/gripButton");
        //_fireAction = new InputAction(binding: "<XRController>/triggerButton");
        
        _rollAction.Enable();
        _horizontalAction.Enable();
        _verticalAction.Enable();
        _upDownAction.Enable();
        _gripAction.Enable();
        //_fireAction.Enable();
    }

    private void FixedUpdate()
    {
        var rollInput = _rollAction.ReadValue<float>();
        var horizontalInput = _horizontalAction.ReadValue<float>();
        var verticalInput = _verticalAction.ReadValue<float>();
        var upDownInput = _upDownAction.ReadValue<float>();
        var gripInput = _gripAction.ReadValue<float>();
        //var fireInput = _fireAction.ReadValue<float>();
        
        transform.Rotate(Vector3.back * (rotateSpeed * rollInput * Time.deltaTime));
        transform.Rotate(Vector3.up * (turnSpeed * horizontalInput * Time.deltaTime));
        transform.Rotate(Vector3.right * (downAndUpSpeed * verticalInput * Time.deltaTime));
        
        var moveInput = new Vector3(horizontalInput, upDownInput - gripInput, verticalInput);
        transform.Translate(moveInput * (speed * Time.deltaTime));

        /*
        if (fireInput > 0 && _canFire)
        {
            StartCoroutine(Fire());
        }
        */
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
        _horizontalAction.Disable();
        _verticalAction.Disable();
        _upDownAction.Disable();
        _gripAction.Disable();
        //_fireAction.Disable();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * maxViewDist);
    }
}
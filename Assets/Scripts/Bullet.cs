using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform _target;
    [SerializeField] private float speed;
    [SerializeField] private float trackingDuration;
    private float _timer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Hit");
        }
    }

    private void Update()
    {
        if (_target != null)
        {
            var dir = (_target.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, _target.position, speed * Time.deltaTime);

            _timer += Time.deltaTime;
            if (_timer >= trackingDuration)
            {
                _target = null;
            }
        }
        else
        {
            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }
    }

    public void GetTarget(Transform newTarget)
    {
        _target = newTarget;
    }
}

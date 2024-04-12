using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    private Transform _player;
    
    [SerializeField] private List<Transform> waypoints;

    [Header("Stats")]
    [SerializeField] private float speed;
    [SerializeField] private float stopDist;
    [SerializeField] private int currentWaypoint;
    

    [Header("Tracking")] 
    [SerializeField] private float searchRange;
    [SerializeField] private float chaseRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float wanderRadius;
    

    private bool _plrInSearchRange;
    private bool _plrInRange;
    private bool _isAttackRange;
    
    private enum States
    {
        Patrol,
        Search,
        Chase,
        Attack
    }

    private States _states;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        SearchRange();
        
        if (!_plrInSearchRange && !_plrInRange && !_plrInSearchRange && !_isAttackRange)
        {
            _states = States.Attack;
        }
        if (_plrInSearchRange && !_plrInRange && !_isAttackRange)
        {
            _states = States.Search;
        }
        if (_plrInRange && !_isAttackRange)
        {
            _states = States.Chase;
        }
        if (_isAttackRange)
        {
            _states = States.Attack;
        }

        switch (_states)
        {
            case States.Search:
                Searching();
                break;
            case States.Patrol:
                Patrolling();
                break;
            case States.Chase:
                Chasing();
                break;
            case States.Attack:
                Attacking();
                break;
            default:
                Patrolling();
                break;
        }
    }

    private void SearchRange()
    {
        var dist = Vector3.Distance(transform.position, _player.position);
        _plrInSearchRange = dist <= searchRange;
    }

    private void ChaseRange()
    {
        var dist = Vector3.Distance(transform.position, _player.position);
        _plrInRange = dist <= chaseRange;
    }

    private void Searching()
    {
    }

    private void Patrolling()
    {
        var position = transform.position;
        var dist = Vector3.Distance(position, waypoints[currentWaypoint].position);
        position = Vector3.Lerp(position, waypoints[currentWaypoint].position, Time.deltaTime * speed);
        transform.position = position;

        if (dist <= stopDist)
        {
            currentWaypoint++;
        }

        if (currentWaypoint >= waypoints.Count)
        {
            currentWaypoint = 0;
        }
    }

    private void Chasing()
    {
        var randomDirection = Random.Range(0f, 360f);
        var randomPoint = RandomPoint(_player.position, wanderRadius, randomDirection);
        transform.position = Vector3.Lerp(transform.position, randomPoint, speed * Time.deltaTime);
    }
    
    private Vector3 RandomPoint(Vector3 center, float radius, float angle)
    {
        var direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        var randomPoint = center + direction * radius;
        return randomPoint;
    }

    private void Attacking()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (waypoints.Count <= 0) return;

        foreach (var t in waypoints.Where(t => t != null))
        {
            Gizmos.DrawWireSphere(t.position, stopDist);
        }
    }

    private void OnDrawGizmosSelected()
    {
        var position = transform.position;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(position, searchRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, attackRange);
    }
}

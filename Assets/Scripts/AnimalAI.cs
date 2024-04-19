using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// TODO: ADD HUNGER METER, FOR INTERFACE TO CHANGE VARIABLES
// TODO: ADD FEAR METER, VICIOUSNESS METER, GOOFINESS (bouncing, rolling), airtime, social?
public class AnimalAI : MonoBehaviour
{
    private Transform _player;
    
    [SerializeField] private List<Transform> waypoints;
    
    [Header("Physics")]
    [SerializeField] private Vector3 gravityDir = Vector3.down;
    [SerializeField] private float gravityForce = 9.86f;
    [SerializeField] private bool gravityEnabled;
    [SerializeField] private float groundCheckDist;
    private bool _isGrounded;

    [Header("Stats")]
    [SerializeField] private float speed;
    [SerializeField] private float stopDist;
    [SerializeField] private int currentWaypoint;
    [SerializeField] private float rotateSpeed;

    [Header("Tracking")] 
    [SerializeField] private float searchRange;
    [SerializeField] private float chaseRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float wanderRadius;
    //[SerializeField] private float loseSightCooldown = 3f;

    [Header("Emotions")] 
    [SerializeField] [Range(0, 10)]
    private int hunger; 
    [SerializeField] [Range(0, 10)]
    private int fear; 
    [SerializeField] [Range(0, 10)]
    private int viciousness; 
    [SerializeField] [Range(0, 10)]
    private int goofiness; 
    [SerializeField] [Range(0, 10)]
    private int airtime;
    [SerializeField] [Range(0, 10)]
    private int sociability; 
    
    // Thought States
    private bool _plrSensed;
    private bool _plrHeard;
    private bool _plrSeen;
    
    // Player Interactions
    private bool _plrAttacked;
    private bool _plrFed;
    

    private enum Behaviours
    {
        Idle, // Idle a,b,c, bounce, sit, spin, eat
        Walk,
        Run,
        Swim,
        Scared,
        Fly,
        Attack,
        Roll
    }
    
    [SerializeField] private Behaviours behaviours;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        GroundCheck();
        
        SearchRange();
        ChaseRange();
        AttackRange();
        
        StateTransition();

        switch (behaviours)
        {
            case Behaviours.Idle:
                break;
            case Behaviours.Walk:
                break;
            case Behaviours.Run:
                break;
            case Behaviours.Swim:
                break;
            case Behaviours.Scared:
                break;
            case Behaviours.Fly:
                break;
            case Behaviours.Attack:
                break;
            case Behaviours.Roll:
                break;
        }
    }

    private void StateTransition()
    {
    }

    private void TransitionToState(Behaviours newBehaviour)
    {
        behaviours = newBehaviour;
    }

    private void SearchRange()
    {
        var dist = Vector3.Distance(transform.position, _player.position);
        _plrSensed = dist <= searchRange;
    }

    private void ChaseRange()
    {
        var dist = Vector3.Distance(transform.position, _player.position);
        _plrHeard = dist <= chaseRange;
    }

    private void AttackRange()
    {
        var dist = Vector3.Distance(transform.position, _player.position);
        _plrSeen = dist <= attackRange;
    }

    private void Searching()
    {
        /*
        if (!_isSearching)
        {
            _searchPoint = RandomPoint();
            _isSearching = true;
        }
    
        var distanceToSearchPoint = Vector3.Distance(transform.position, _searchPoint);
        
        if (distanceToSearchPoint <= stopDist)
        {
            _isSearching = false;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _searchPoint, speed * Time.deltaTime);
        }
        */
    }

    private Vector3 RandomPoint()
    {
        var randomPoint = Vector3.zero;
        var maxDistance = 30f;

        foreach (var waypoint in waypoints)
        {
            var distance = Vector3.Distance(transform.position, waypoint.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                randomPoint = waypoint.position;
            }
        }
    
        randomPoint += Random.insideUnitSphere * wanderRadius;
        return randomPoint;
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
        var direction = (_player.position - transform.position).normalized;
        var lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
        var targetPoint = _player.position - direction * (wanderRadius * 0.9f);
        var distanceToTarget = Vector3.Distance(transform.position, targetPoint);
        var interpolationFactor = Mathf.Clamp01(distanceToTarget / wanderRadius);
        var adjustedSpeed = speed * interpolationFactor;
        transform.position = Vector3.Lerp(transform.position, targetPoint, adjustedSpeed * Time.deltaTime);
    }
    
    private Vector3 RandomPoint(Vector3 center, float radius, float angle)
    {
        var direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        var randomPoint = center + direction * radius;
        return randomPoint;
    }

    private void Attacking()
    {
        /*
        if (!_isAttackRange)
        {
            // If player is out of range, start the sight timer
            if (SightTimer())
            {
                // Rotate towards the player
                var direction = (_player.position - transform.position).normalized;
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotateSpeed);
            }
            else
            {
                TransitionToState(States.Chase);
            }
        }
        else
        {
            Fire();
        }
        */
    }
    
    private void Fire()
    {
        /*
        var newBullet = Instantiate(bulletPrefab, bulletOrigin.position, Quaternion.identity, bulletParent);
        var bullet = newBullet.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.GetTarget(_player);
        }
        */
    }

    private bool SightTimer()
    {
        /*
            if (_loseSightTimer > 0f)
            {
                _loseSightTimer -= Time.deltaTime;
                return true;
            }
            else
            {
                return false;
            }
            */
        return false;
    }

    private void ApplyGravity()
    {
        if (!gravityEnabled) return;
        if (_isGrounded) return;
        var gravity = gravityDir.normalized * gravityForce;
        transform.position += gravity * Time.deltaTime;
    }

    private void GroundCheck()
    {
        var col = Physics.Raycast(transform.position, gravityDir.normalized, out var hit, groundCheckDist);
        if (hit.collider)
        {
            _isGrounded = hit.collider.CompareTag($"Ground") && col;
        }
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

        Gizmos.color = Color.white;
        var newDist = new Vector3(position.x, position.y - groundCheckDist, position.z);
        Gizmos.DrawLine(position, newDist);
    }
}
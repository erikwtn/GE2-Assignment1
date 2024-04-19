using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// TODO: ADD HUNGER METER, FOR INTERFACE TO CHANGE VARIABLES
// TODO: ADD FEAR METER, VICIOUSNESS METER, GOOFINESS (bouncing, rolling), airtime, social?
public class EnemyAI : MonoBehaviour
{
    private Transform _player;
    
    [SerializeField] private List<Transform> waypoints;
    
    [Header("Physics")]
    [SerializeField] private Vector3 gravityDir = Vector3.down;
    [SerializeField] private float gravityForce = 9.86f;
    [SerializeField] private bool gravityEnabled;
    [SerializeField] private float groundCheckDist;
    [SerializeField] private bool _isGrounded;

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
    [SerializeField] private float loseSightCooldown = 3f;

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
    
    //[Header("Combat")]
    //[SerializeField] private GameObject bulletPrefab;
    //[SerializeField] private Transform bulletOrigin;
    //[SerializeField] private Transform bulletParent;
    //[SerializeField] private float fireCooldown = 0.5f;
    
    private float _loseSightTimer = 0f;
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

    [SerializeField] private States states;
    [SerializeField] private Behaviours behaviours;
    
    private Vector3 _searchPoint;
    private bool _isSearching = false;
    
    private float _searchTimer = 0f;
    private float _chaseTimer = 0f;


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
        UpdateTimers();

        switch (states)
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

    private void StateTransition()
    {
        if (!_isAttackRange && !_plrInRange && !_plrInSearchRange)
        {
            TransitionToState(States.Patrol);
        }
        else if (_plrInSearchRange && !_plrInRange && !_isAttackRange)
        {
            if (_searchTimer <= 0f)
            {
                TransitionToState(States.Search);
                _searchTimer = 0f;
            }
            else
            {
                _searchTimer -= Time.deltaTime;
            }
        }
        else if (_plrInRange && !_isAttackRange)
        {
            if (_chaseTimer <= 0f)
            {
                TransitionToState(States.Chase);
                _chaseTimer = 0f;
            }
            else
            {
                _chaseTimer -= Time.deltaTime; 
            }
        }
        else if (_isAttackRange)
        {
            TransitionToState(States.Attack);
        }
    }
    
    private void TransitionToState(States newState)
    {
        states = newState;
    }
    
    private void UpdateTimers()
    {
        switch (states)
        {
            // Update timers for search and chase states
            case States.Search when _searchTimer <= 0f:
                _searchTimer = loseSightCooldown;
                break;
            case States.Chase when _chaseTimer <= 0f:
                _chaseTimer = loseSightCooldown;
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

    private void AttackRange()
    {
        var dist = Vector3.Distance(transform.position, _player.position);
        _isAttackRange = dist <= attackRange;
    }

    private void Searching()
    {
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
            if (_loseSightTimer > 0f)
            {
                _loseSightTimer -= Time.deltaTime;
                return true;
            }
            else
            {
                return false;
            }
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
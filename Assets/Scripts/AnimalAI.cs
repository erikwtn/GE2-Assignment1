using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AnimalAI : MonoBehaviour
{
    private Transform _player;
    private Vector3 _targetPos;
    private Animator _animator;
    
    [SerializeField] private List<Transform> waypoints;
    
    [Header("Physics")]
    [SerializeField] private Vector3 gravityDir = Vector3.down;
    [SerializeField] private float gravityForce = 9.86f;
    [SerializeField] private bool gravityEnabled;
    [SerializeField] private float groundCheckDist;
    private bool _isGrounded;

    [Header("Stats")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float stopDist;
    [SerializeField] private float flySpeed;
    [SerializeField] private int currentWaypoint;
    [SerializeField] private float rotateSpeed;

    [Header("Tracking")] 
    [SerializeField] private float senseRange;
    [SerializeField] private float heardRange;
    [SerializeField] private float seenRange;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float flyRange;
    [SerializeField] private Transform ground;
    //[SerializeField] private float loseSightCooldown = 3f;

    [Header("Emotions")] 
    [SerializeField] [Range(0, 10)]
    private int fear; 
    [SerializeField] [Range(0, 10)]
    private int viciousness; 
    [SerializeField] [Range(0, 10)]
    private float goofiness; 
    [SerializeField] [Range(0, 10)]
    private int airtime;
    [SerializeField] [Range(0, 10)]
    private int sociability; 
    [SerializeField] [Range(0, 10)]
    private int energy; 
    
    // Thought States
    private bool _plrSensed;
    private bool _plrHeard;
    private bool _plrSeen;
    
    // Player Interactions
    private bool _plrAttacked;
    private bool _plrFed;
    
    // Doing States
    private bool _wandering;
    private bool _flying;
    

    private enum Behaviours
    {
        Idle, // Idle a,b,c, bounce, sit, spin
        Walk,
        Run,
        Scared,
        Fly,
        Attack,
        Roll,
        Eat
    }

    [SerializeField] private Behaviours behaviours;
    
    // Animations
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsEating = Animator.StringToHash("isEating");
    private static readonly int IsFlying = Animator.StringToHash("isFlying");

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        GroundCheck();
        
        _plrSensed = CheckRange(senseRange);
        _plrHeard = CheckRange(heardRange);
        _plrSeen = CheckRange(seenRange);
        
        CalcEmotions();

        switch (behaviours)
        {
            case Behaviours.Idle:
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsRunning, false);
                _animator.SetBool(IsEating, false);
                _animator.SetBool(IsFlying, false);
                break;
            case Behaviours.Walk:
                Wander(walkSpeed);
                _animator.SetBool(IsWalking, true);
                _animator.SetBool(IsRunning, false);
                _animator.SetBool(IsEating, false);
                _animator.SetBool(IsFlying, false);
                break;
            case Behaviours.Run:
                Wander(runSpeed);
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsRunning, true);
                _animator.SetBool(IsEating, false);
                _animator.SetBool(IsFlying, false);
                break;
            case Behaviours.Scared:
                break;
            case Behaviours.Fly:
                _animator.SetBool(IsFlying, true);
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsRunning, false);
                _animator.SetBool(IsEating, false);
                if (!_flying)
                {
                    FlyToRandomPoint();
                }
                else
                {
                    FlyToTarget();
                }
                break;
            case Behaviours.Attack:
                break;
            case Behaviours.Roll:
                break;
            case Behaviours.Eat:
                _animator.SetBool(IsWalking, false);
                _animator.SetBool(IsRunning, false);
                _animator.SetBool(IsEating, true);
                _animator.SetBool(IsFlying, false);
                break;
        }
    }

    private void CalcEmotions()
    {
        // hunger, fear, viciousness, goofiness, airtime, sociability, energy
        // energy > airtime > sociability > goofiness
        // viciousness > fear
        
        if (!_plrSensed && !_plrHeard && !_plrSeen)
        {
            if (energy < 2)
            {
                TransitionToState(Behaviours.Eat);
            }
            else if (energy is 2 or 3)
            {
                TransitionToState(Behaviours.Idle);
            }
            else if (energy is > 3 and < 7)
            {
                TransitionToState(Behaviours.Walk);
            }
            else if (energy is 7 or 8)
            {
                TransitionToState(Behaviours.Run);
            }
            else if (energy > 8)
            {
                TransitionToState(Behaviours.Fly);
            }
        }
    }

    private void Wander(float moveSpeed)
    {
        if (!_wandering)
        {
            var randomDir = Random.insideUnitCircle.normalized * wanderRadius;
            _targetPos = transform.position + new Vector3(randomDir.x, 0f, randomDir.y);
            _wandering = true;
        }

        transform.position = Vector3.MoveTowards(transform.position, _targetPos, moveSpeed * Time.deltaTime);
        
        var direction = _targetPos - transform.position;

        if (direction != Vector3.zero)
        {
            var lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        }

        if (!(Vector3.Distance(transform.position, _targetPos) < 0.1f)) return;
        _wandering = false;
    }
    
    private void FlyToRandomPoint()
    {
        var gY = ground.position.y + (ground.localScale.y) / 2f + 1f;
        var y = Mathf.Clamp(Random.Range(gY, gY + 20f), gY, gY + 20f);
        _targetPos = GetRandomPoint(transform.position, y, flyRange);
        _flying = true;
    }

    private void FlyToTarget()
    {
        var dir = (_targetPos - transform.position).normalized;
        var targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed);
        transform.position = Vector3.MoveTowards(transform.position, _targetPos, flySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetPos) < 0.1f)
        {
            _flying = false;
        }
    }

    private Vector3 GetRandomPoint(Vector3 center, float y, float range)
    {
        var x = Random.Range(center.x - range, center.x + range);
        var z = Random.Range(center.z - range, center.z + range);

        return new Vector3(x, y, z);
    }

    private void TransitionToState(Behaviours newBehaviour)
    {
        behaviours = newBehaviour;
    }

    private bool CheckRange(float range)
    {
        var dist = Vector3.Distance(transform.position, _player.position);
        return dist <= range;
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
        Gizmos.DrawWireSphere(position, senseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, heardRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, seenRange);

        Gizmos.color = Color.white;
        var newDist = new Vector3(position.x, position.y - groundCheckDist, position.z);
        Gizmos.DrawLine(position, newDist);
    }
}
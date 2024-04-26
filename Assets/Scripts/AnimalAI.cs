using System;
using System.Collections;
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
    private Coroutine _updateEn;
    
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
    [SerializeField] private Transform groundTop;
    
    [Header("Emotions")] 
    [SerializeField] [Range(0, 10)]
    private int fear; 
    [SerializeField]
    private bool isGoofy; 
    [SerializeField] [Range(0, 10)]
    private int sociability; 
    [SerializeField] [Range(0, 10)]
    private int energy;
    private int _previousEnergy = 11;
    [SerializeField] private float energyUpdateDel;
    
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
    private bool _isReset;
    private bool _calc;
    

    private enum Behaviours
    {
        Idle, // Idle a,b,c, bounce, sit, spin
        Walk,
        Run,
        Scared,
        Fly,
        Roll,
        Eat,
    }

    [SerializeField] private Behaviours behaviours;
    
    // Animations
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsEating = Animator.StringToHash("isEating");
    private static readonly int IsFlying = Animator.StringToHash("isFlying");
    private static readonly int IsRolling = Animator.StringToHash("isRolling");

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _animator = GetComponent<Animator>();
        _updateEn = StartCoroutine(UpdateEnergy());
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        GroundCheck();
        
        _plrSensed = CheckRange(senseRange);
        _plrHeard = CheckRange(heardRange);
        _plrSeen = CheckRange(seenRange);

        // CalcEmotions();

        if (_previousEnergy != energy)
        {
            _calc = true;
            _previousEnergy = energy;
        }
        
        CalcEmotions();

        switch (behaviours)
        {
            case Behaviours.Idle:
                SwitchAnimations(null);
                break;
            case Behaviours.Walk:
                Wander(walkSpeed);
                SwitchAnimations(IsWalking);
                break;
            case Behaviours.Run:
                Wander(runSpeed);
                SwitchAnimations(IsRunning);
                break;
            case Behaviours.Scared:
                break;
            case Behaviours.Fly:
                SwitchAnimations(IsFlying);
                if (!_flying)
                {
                    FlyToRandomPoint();
                }
                else
                {
                    FlyToTarget();
                }
                break;
            case Behaviours.Roll:
                SwitchAnimations(IsRolling);
                Wander(walkSpeed);
                break;
            case Behaviours.Eat:
                SwitchAnimations(IsEating);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SwitchAnimations(int? anim)
    {
        var parameters = _animator.parameters;

        foreach (var parameter in parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Bool) return;
            var hash = parameter.nameHash;
            _animator.SetBool(hash, hash == anim);
        }
    }

    private void CalcEmotions()
    {
        // hunger, fear, viciousness, goofiness, airtime, sociability, energy
        // energy > airtime > sociability > goofiness
        // viciousness > fear
        
        /*
                   while eating, energy increases fast
                   While idle energy increases slow
                   while walking energy decreases 
                   while running energy decreases faster
                   while flying energy decreases faster
                   energy will randomly increase at any stage
                  */
        if (!_plrSensed && !_plrHeard && !_plrSeen)
        {
            if (!_calc) return;
            switch (energy)
            {
                case < 2:
                    TransitionToState(Behaviours.Eat);
                    _calc = false;
                    break;
                case 2 or 3:
                    TransitionToState(Behaviours.Idle);
                    _calc = false;
                    break;
                case > 3 and < 7:
                    TransitionToState(Behaviours.Walk);
                    _calc = false;
                    break;
                case 7 or 8:
                    TransitionToState(isGoofy ? Behaviours.Roll : Behaviours.Run);
                    _calc = false;
                    break;
                case > 8:
                    TransitionToState(Behaviours.Fly);
                    _calc = false;
                    break;
            }
        }
    }

    private void ManageEnergy()
    {
        if (Random.Range(0, 100) < 30)
        {
            energy += Random.Range(4, 8);
        }
        
        switch (behaviours)
        {
            case Behaviours.Eat:
                energy += 1;
                break;
            case Behaviours.Idle:
                energy += 1;
                break;
            case Behaviours.Walk:
                energy -= 1;
                break;
            case Behaviours.Run:
                energy -= 1;
                break;
            case Behaviours.Fly:
                energy -= 1;
                break;
        }
        
        energy = Mathf.Clamp(energy, 0, 10);
    }
    
    private IEnumerator UpdateEnergy()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(energyUpdateDel);
            ManageEnergy();
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
        if (behaviours == Behaviours.Fly)
        {
            Returning(newBehaviour);
        }
        else
        {
            behaviours = newBehaviour;
        }
    }

    private void Returning(Behaviours newBehaviour)
    {
        _targetPos = new Vector3(transform.position.x, groundTop.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;
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
    
    private void OnDestroy()
    {
        if (UpdateEnergy() == null) return;
        StopCoroutine(UpdateEnergy());
    }
}
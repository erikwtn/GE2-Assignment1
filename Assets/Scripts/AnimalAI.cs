using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AnimalAI : MonoBehaviour
{
    private Transform _player;
    private Vector3 _targetPos;
    private Animator _animator;
    
    [SerializeField] private List<Transform> otherBirds;
    private Coroutine _updateEn;
    private AudioSource _audioSource;
    
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
    [SerializeField] private float rotateSpeed;

    [Header("Tracking")] 
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

    [Header("Parameters")] 
    [SerializeField] private Toggle goofyToggle;
    [SerializeField] private Slider socialSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Slider fearSlider;
    
    // Thought States
    private bool _plrSeen;
    private bool _otherBirdSeen;
    
    // Doing States
    private bool _wandering;
    private bool _flying;
    private bool _isReset;
    private bool _calc;
    
    // Timing
    [SerializeField] private float squawkDur;
    private bool _isTimer = false;
    private bool _isUpdating;

    private enum Behaviours
    {
        Idle,
        Walk,
        Run,
        Scared,
        Fly,
        Roll,
        Eat,
        Squawk,
        RunAway,
        Socialise
    }

    [SerializeField] private Behaviours behaviours;
    
    // Animations
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsRunning = Animator.StringToHash("isRunning");
    private static readonly int IsEating = Animator.StringToHash("isEating");
    private static readonly int IsFlying = Animator.StringToHash("isFlying");
    private static readonly int IsRolling = Animator.StringToHash("isRolling");
    private static readonly int IsBouncing = Animator.StringToHash("isBouncing");
    private static readonly int IsScared = Animator.StringToHash("isScared");

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _animator = GetComponent<Animator>();
        _updateEn = StartCoroutine(UpdateEnergy());
        _audioSource = GetComponent<AudioSource>();
        UpdateUI();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        GroundCheck();

        if (!_isUpdating)
        {
            UpdateUI();
        }

        _plrSeen = CheckRange(seenRange, true);
        _otherBirdSeen = CheckRange(seenRange, false);

        if (_previousEnergy != energy)
        {
            _calc = true;
            _previousEnergy = energy;
        }
        
        CalcEmotions();

        switch (behaviours)
        {
            case Behaviours.Idle:
                if (isGoofy)
                {
                    SwitchAnimations(IsBouncing);
                }
                else
                {
                    SwitchAnimations(null);
                }
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
                SwitchAnimations(IsScared);
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
            case Behaviours.Squawk:
                SwitchAnimations(IsScared);
                Squawking();
                break;
            case Behaviours.RunAway:
                Flee();
                SwitchAnimations(IsRunning);
                break;
            case Behaviours.Socialise:
                Squawking();
                SwitchAnimations(IsBouncing);
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
        if (!_calc) return;
        if (!_plrSeen)
        {
            if (sociability < 5 || !_otherBirdSeen)
            {
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
            else if (sociability >= 5 && _otherBirdSeen)
            { 
                TransitionToState(Behaviours.Socialise);
                _calc = false;
            }
        }
        else if (_plrSeen)
        {
            if (fear > 7)
            {
                TransitionToState(energy > 5 ? Behaviours.RunAway : Behaviours.Squawk);
                _calc = false;
            }
            else
            {
                TransitionToState(Behaviours.Scared);
                _calc = false;
            }
        }
    }

    private void ManageEnergy()
    {
        if (Random.Range(0, 100) < 30 && behaviours != Behaviours.Fly)
        {
            energy += Random.Range(4, 8);
        }
        
        switch (behaviours)
        {
            case Behaviours.Eat:
                energy += 1;
                fear -= 1;
                sociability -= 1;
                break;
            case Behaviours.Idle:
                energy += 1;
                fear -= 1;
                sociability += 1;
                break;
            case Behaviours.Walk:
                energy -= 1;
                fear -= 1;
                sociability += 1;
                break;
            case Behaviours.Run:
                energy -= 1;
                fear -= 1;
                break;
            case Behaviours.Fly:
                energy -= 1;
                fear -= 1;
                sociability -= 1;
                break;
            case Behaviours.Scared:
                energy += 1;
                fear += 1;
                sociability += 1;
                break;
            case Behaviours.Roll:
                energy -= 1;
                sociability += 1;
                break;
            case Behaviours.RunAway:
                fear -= 1;
                energy -= 1;
                break;
            case Behaviours.Squawk:
                fear += 1;
                energy += 1;
                break;
            case Behaviours.Socialise:
                fear -= 1;
                sociability -= 2;
                break;
            default:
                throw new ArgumentOutOfRangeException();
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

    private void Squawking()
    {
        if (!_isTimer)
        {
            StartCoroutine(SquawkTimer());
        }
    }

    private IEnumerator SquawkTimer()
    {
        _isTimer = true;

        var timer = squawkDur;

        while (timer > 0f)
        {
            yield return new WaitForSecondsRealtime(1f);
            timer -= 1f;
        }

        if (!_audioSource.isPlaying)
        {
            _audioSource.Play();
        }

        _isTimer = false;
    }
    
    private void Flee()
    {
        var dir = transform.position - _player.position;
        dir.y = 0;
        dir.Normalize();
        var targetPosition = transform.position + dir * (runSpeed * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, runSpeed * Time.deltaTime);
        
        var targetRotation = Quaternion.LookRotation(dir);
        targetRotation.x = 0;
        targetRotation.z = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
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
        if (behaviours == Behaviours.Fly || Math.Abs(transform.position.y - groundTop.position.y) > 0.1f)
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

    private bool CheckRange(float range, bool isPlayer)
    {
        var dist = 0f;
        if (isPlayer)
        {
            dist = Vector3.Distance(transform.position, _player.position);
        }
        else
        {
            foreach (var bird in otherBirds)
            {
                var distance = Vector3.Distance(transform.position, bird.position);
                if (distance < dist || dist == 0f)
                {
                    dist = distance;
                }
            }
        }
        
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

    private void UpdateUI()
    {
        goofyToggle.isOn = isGoofy;
        socialSlider.value = sociability;
        energySlider.value = energy;
        fearSlider.value = fear;
    }

    public void UpdateParams(GameObject param)
    {
        _isUpdating = true;
        if (param.name == "Toggle")
        {
            isGoofy = param.GetComponent<Toggle>().isOn;
            _isUpdating = false;
        }
        else
        {
            var s = param.GetComponent<Slider>();

            if (s == energySlider)
            {
                energy = (int)energySlider.value;
                _isUpdating = false;
            }
            else if (s == fearSlider)
            {
                fear = (int)fearSlider.value;
                _isUpdating = false;
            }
            else if (s == socialSlider)
            {
                sociability = (int)socialSlider.value;
                _isUpdating = false;
            }
            else if (!string.IsNullOrEmpty(s.gameObject.name = "Slider"))
            {
                seenRange = s.value;
                _isUpdating = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        var position = transform.position;
        
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
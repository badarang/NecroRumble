using System;
using System.Collections;
using LOONACIA.Unity;
using LOONACIA.Unity.Managers;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{

	private SpriteRenderer spriteRenderer;
	private Rigidbody2D rb;
	private PlayerData _data;
	private InputManager _input;
	private Animator _animator;
	private Player _player;
	public bool CanMove { get; set; } = true;

	[Header("Movement Stats")]
	[SerializeField, Range(0f, 20f)] private float _maxSpeed = 20f;
	[SerializeField, Range(0f, 100f)] private float _maxAcceleration = 60f;
	[SerializeField, Range(0f, 100f)] private float _maxDecceleration = 60f;
	[SerializeField, Range(0f, 100f)] private float _maxTurnSpeed = 80f;
	[SerializeField] private float _friction = 1f;

	[Header("Calculations")]
	[ReadOnly] private float _directionX;
	[ReadOnly] private float _directionY;
	[ReadOnly] private Vector2 _direction;
	[ReadOnly] private Vector2 _desiredVelocity;
	[ReadOnly] private Vector2 _velocity;
	[ReadOnly] private float _maxSpeedChangeX;
	[ReadOnly] private float _maxSpeedChangeY;
	[ReadOnly] private float _acceleration;
	[ReadOnly] private float _deceleration;
	[ReadOnly] private float _turnSpeed;

	private Transform _flipable;

	private void Awake()
	{
		_data = ManagerRoot.Resource.Load<PlayerData>("Data/PlayerData");
		_input = ManagerRoot.Input;
		_player = FindObjectOfType<Player>();
		_flipable = transform.Find("@Flipable");
	}

	void Start()
	{
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();
		_animator = GetComponentInChildren<Animator>();
	}
	
	void Update()
	{
		if ((_directionX != 0 || _directionY != 0) && CanMove && _player.CanAction)
		{
			_animator.SetTrigger("Walk");
			ShakeRotate();
		}
		else
		{
			if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("NecromancerSpecialAttack"))
			{
				_animator.SetTrigger("Idle");
			}
			BackToOriginalRotation();
		}
		
		if (_directionX != 0)
		{
			_flipable.localScale = new Vector3(_directionX > 0 ? 1 : -1, 1, 1) * transform.localScale.y ;
		}

		//DetectCollision(direction);

		_desiredVelocity = _direction * Mathf.Max(_maxSpeed - _friction, 0f);
		
		//마지막 스프라이트 유지
		
		//transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(transform.rotation.eulerAngles.z, 0, 20f * Time.deltaTime));
	}
	private void FixedUpdate()
	{
		if (CanMove && _player.CanAction)
		{
			_velocity = rb.velocity;
			MoveWithAcceleration();
		}
		else
		{
			rb.velocity = Vector2.zero;
		}
	}

	public void IncreaseMaxSpeed(float maxSpeed_)
	{
		_maxSpeed += maxSpeed_;
	}
	public void IncreaseMaxAccel(float maxAccel_)
	{
		_maxAcceleration += maxAccel_;
	}
	public void IncreaseMaxDeccel(float maxDeccel_)
	{
		_maxDecceleration += maxDeccel_;
	}

	public bool IsWalking()
	{
		return _animator.GetCurrentAnimatorStateInfo(0).IsName("NecromancerWalk");
	}
	
	private Vector2 SetDirection()
	{
		_directionX = _input.MoveHorizontal;
		_directionY = _input.MoveVertical;

		return new Vector2(_directionX, _directionY).normalized;
	}

	public void SetAnimator(UnitManager.CharacterType characterType_)
	{
		_animator = GetComponentInChildren<Animator>();
		if (_animator == null)
		{
			Debug.Log("Animator is null");
			return;
		}
		if (characterType_ == UnitManager.CharacterType.Necromancer)
		{
			_animator.runtimeAnimatorController = ManagerRoot.Resource.Load<RuntimeAnimatorController>($"Animations/Player/Player");
			Debug.Log("SetAnimator into Necromancer");
		}
		else if (characterType_ == UnitManager.CharacterType.CutePlayer)
		{
			_animator.runtimeAnimatorController = ManagerRoot.Resource.Load<RuntimeAnimatorController>($"Animations/Player2/Player2");
			Debug.Log("SetAnimator into CutePlayer");
		}
	}
	
	public Vector2 GetMoveDirection()
	{
		return _direction;
	}
	
	public Animator GetAnimator()
	{
		return _animator;
	}
	
	public bool CheckCurrentPlayerAnimation(string animationName_)
	{
		return _animator.GetCurrentAnimatorStateInfo(0).IsName(animationName_);
	}
	
	public void TriggerAnimation(string animationName_)
	{
		_animator.SetTrigger(animationName_);
	}
	
	public void PlayAnimation(string animationName_)
	{
		_animator.Play(animationName_);
	}
	
	private void ShakeRotate()
	{
		spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 10f) * 5f);
	}
	
	private void BackToOriginalRotation()
	{
		spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(transform.rotation.eulerAngles.z, 0, 20f * Time.deltaTime));
	}

	private void MoveWithAcceleration()
	{
		_acceleration = _maxAcceleration;
		_deceleration = _maxDecceleration;
		_turnSpeed = _maxTurnSpeed;

		_direction = SetDirection();

		if (_input.MoveButton)
		{
			if (Mathf.Sign(_directionX) != Mathf.Sign(_velocity.x))
			{
				_maxSpeedChangeX = _turnSpeed * Time.deltaTime;
			}
			else
			{
				_maxSpeedChangeX = _acceleration * Time.deltaTime;
			}

			if (Mathf.Sign(_directionY) != Mathf.Sign(_velocity.y))
			{
				_maxSpeedChangeY = _turnSpeed * Time.deltaTime;
			}
			else
			{
				_maxSpeedChangeY = _acceleration * Time.deltaTime;
			}
		}
		else
		{
			_maxSpeedChangeX = _deceleration * Time.deltaTime;
			_maxSpeedChangeY = _deceleration * Time.deltaTime;
		}

		_velocity.x = Mathf.MoveTowards(_velocity.x, _desiredVelocity.x, _maxSpeedChangeX);
		_velocity.y = Mathf.MoveTowards(_velocity.y, _desiredVelocity.y, _maxSpeedChangeX);

		rb.velocity = VelocityClamp(_velocity);
	}
	
	private Vector2 VelocityClamp(Vector2 velocity_)
	{
		var velocityClamped = velocity_;

		if (_input.LastUsedInputType == DeviceChangeListener.ControlDeviceType.KeyboardAndMouse)
		{
			if (MathF.Abs(_input.MoveHorizontal) > 0.7 && Mathf.Abs(_input.MoveVertical) > 0.7)
			{
				velocityClamped *= 0.9f;
			}
		}
		else
		{
			if (MathF.Abs(_input.MoveHorizontal) > 0.6 && Mathf.Abs(_input.MoveVertical) > 0.6)
			{
				velocityClamped *= 0.9f;
			}
			else if(MathF.Abs(_input.MoveHorizontal) > 0.8 && Mathf.Abs(_input.MoveVertical) > 0.4)
			{
				velocityClamped *= 0.9f;
			}
			else if(MathF.Abs(_input.MoveHorizontal) > 0.4 && Mathf.Abs(_input.MoveVertical) > 0.8)
			{
				velocityClamped *= 0.9f;
			}
			else if(MathF.Abs(_input.MoveHorizontal) > 0.9 && Mathf.Abs(_input.MoveVertical) > 0.1)
			{
				velocityClamped *= 0.9f;
			}
			else if(MathF.Abs(_input.MoveHorizontal) > 0.1 && Mathf.Abs(_input.MoveVertical) > 0.9)
			{
				velocityClamped *= 0.9f;
			}
		}

		return velocityClamped;
	}
	
	public float GetMass() { return rb.mass; }
	#region Not Use
	//float _up = 0f;
	//float _down = 0f;
	//float _right = 0f;
	//float _left = 0f;
	//[SerializeField] float _accel = 2.5f;
	//[SerializeField] float _deccel = 3f;

	//private void Move4()
	//{
	//    if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
	//    {
	//        if (Input.GetKey(KeyCode.W))
	//        {
	//            _up += Time.deltaTime * _accel;
	//        }
	//        else
	//        {
	//            _up -= Time.deltaTime * _deccel;
	//        }
	//        if (Input.GetKey(KeyCode.S))
	//        {
	//            _down += Time.deltaTime * _accel;
	//        }
	//        else
	//        {
	//            _down -= Time.deltaTime * _deccel;
	//        }
	//        if (Input.GetKey(KeyCode.A))
	//        {
	//            _left += Time.deltaTime * _accel;
	//        }
	//        else
	//        {
	//            _left -= Time.deltaTime * _deccel;
	//        }
	//        if (Input.GetKey(KeyCode.D))
	//        {
	//            _right += Time.deltaTime * _accel;
	//        }
	//        else
	//        {
	//            _right -= Time.deltaTime * _deccel;
	//        }
	//    }
	//    else
	//    {
	//        _up = 0;
	//        _down = 0;
	//        _left = 0;
	//        _right = 0;
	//    }

	//    _up = Mathf.Clamp01(_up);
	//    _down = Mathf.Clamp01(_down);
	//    _left = Mathf.Clamp01(_left);
	//    _right = Mathf.Clamp01(_right);

	//    Vector2 direction = new Vector2(_right - _left, _up - _down).normalized;

	//    //_acceleration = _maxAcceleration;
	//    //_deceleration = _maxDecceleration;
	//    //_turnSpeed = _maxTurnSpeed;

	//    //if (_input.MoveButton)
	//    //{
	//    //    if (Mathf.Sign(direction.x) != Mathf.Sign(_velocity.x))
	//    //    {
	//    //        _maxSpeedChangeX = _turnSpeed * Time.deltaTime;
	//    //    }
	//    //    else
	//    //    {
	//    //        _maxSpeedChangeX = _acceleration * Time.deltaTime;
	//    //    }

	//    //    if (Mathf.Sign(direction.y) != Mathf.Sign(_velocity.y))
	//    //    {
	//    //        _maxSpeedChangeY = _turnSpeed * Time.deltaTime;
	//    //    }
	//    //    else
	//    //    {
	//    //        _maxSpeedChangeY = _acceleration * Time.deltaTime;
	//    //    }
	//    //}
	//    //else
	//    //{
	//    //    _maxSpeedChangeX = _deceleration * Time.deltaTime;
	//    //    _maxSpeedChangeY = _deceleration * Time.deltaTime;
	//    //}

	//    _velocity.x = direction.x;
	//    _velocity.y = direction.y;

	//    rb.velocity = _velocity * _maxSpeed;

	//    //transform.Translate(direction * _maxSpeed * Time.deltaTime, Space.World);
	//}
#endregion
    private void DetectCollision(Vector2 direction_)
    {
        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, direction_, 3f);
        
        if (hit.collider.IsTouchingLayers(Layers.HumanUnit.ToMask() + Layers.Obstacle.ToMask()))
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    public void TakeSlow(float slowPercent_, float slowDuration_)
    {
        StartCoroutine(SlowCoroutine(slowPercent_, slowDuration_));
    }
    
    IEnumerator SlowCoroutine(float slowPercent_, float slowDuration_)
    {
        float originalSpeed = _maxSpeed;
        _maxSpeed *= (1 - slowPercent_ / 100);
        yield return new WaitForSeconds(slowDuration_);
        _maxSpeed = originalSpeed;
    }
}

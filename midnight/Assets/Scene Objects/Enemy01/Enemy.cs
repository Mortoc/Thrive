using UnityEngine;

using System;
using System.Collections.Generic;

public class Enemy : MonoBehaviour 
{
	public ParallaxObject AttachedLayer;
	public event Action OnDeath;
	
	public float AccelerationNoise = 0.1f;
	public float Acceleration = 20.0f;
	
	public float MaxSpeedVariety = 0.1f;
	public float MaxSpeed = 50.0f;
	
	public float ScaleVariety = 0.1f;
	
	public int HitPoints = 1000;
	
	public GameObject AttackProjectile;
	public GameObject DeadPrefab;
	
	private Vector3 _velocity = Vector3.zero;
	
	public float AttackDistance = 350.0f;
	public float AttackDistanceVariety = 0.5f;
	public float AttackRate = 0.5f;
	public int AttackDamage = 100;
	public Transform AttackEmitter;
	private float _attackDistSqr;
	
	private ITask _attackShipTask;
	
	public enum State { Walking, Attacking, LevelOver }
	public State CurrentState = State.Walking;
	
	private CharacterController _controller;
	private Spaceship _spaceship;
	
	private static float RandomVarietyFactor(float variety)
	{
		return 1.0f + ((2.0f * UnityEngine.Random.value * variety) - variety);
	}
	
	void Start()
	{
		_controller = GetComponent<CharacterController>();
		_spaceship = Find.ObjectInScene<Spaceship>();
		AttackDistance *= RandomVarietyFactor( AttackDistanceVariety );
		transform.localScale *= RandomVarietyFactor( ScaleVariety );
		_attackDistSqr = AttackDistance * AttackDistance;
		
		MaxSpeed *= RandomVarietyFactor( MaxSpeedVariety );
		Acceleration *= RandomVarietyFactor( MaxSpeedVariety );
	}
	
	public void TakeDamage(int damage)
	{
		HitPoints -= damage;
		if( HitPoints <= 0 )
		{
			HitPoints = 0;
			DeathTime();
		}
	}
	
	private void DeathTime()
	{
		GameObject deadMe = (GameObject)Instantiate(DeadPrefab);
		deadMe.transform.position = transform.position;
		
		
		RaycastHit rh;
		if( Physics.Raycast(new Ray(transform.position + (Vector3.up * 25.0f), Vector3.down), out rh, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")) )
		{
			deadMe.transform.position = rh.point;
			deadMe.transform.up = rh.normal;
		}		
		
		if( _attackShipTask != null )
			_attackShipTask.Exit();
		
		if( OnDeath != null )
			OnDeath();
		
		Destroy(gameObject);
	}
	
	private IEnumerator<IYieldInstruction> AttackShip()
	{
		while( gameObject && ShouldBeAttacking() )
		{
			GameObject projectile = (GameObject)Instantiate(AttackProjectile);
			projectile.transform.position = AttackEmitter.position;
			projectile.GetComponent<EnemyProjectile>().Damage = AttackDamage;
			
			yield return new YieldForSeconds(AttackRate);
		}
		
		if( _spaceship.IsDestroyed() )
			CurrentState = State.LevelOver;
		else
			CurrentState = State.Walking;
	}
			
	private bool ShouldBeAttacking()
	{
		return _spaceship && !_spaceship.IsDestroyed() && (transform.position - _spaceship.transform.position).sqrMagnitude < _attackDistSqr;
	}
	
	void FixedUpdate()
	{		
		// Close enough to attack the ship
		if( CurrentState != State.Attacking && ShouldBeAttacking() )
		{
			CurrentState = State.Attacking;
			_attackShipTask = Scheduler.Run( AttackShip() );
		}
		
		if( CurrentState == State.Walking )
		{
			// Move towards target
			float thisFrameAccel = RandomVarietyFactor(AccelerationNoise) * Acceleration;
			_velocity += thisFrameAccel * Time.fixedDeltaTime * Vector3.right;
			
			// Max Speed (on X)
			if( Mathf.Abs(_velocity.x) > MaxSpeed )
			{
				if( _velocity.x < 0.0f )
					_velocity.x = -1.0f * MaxSpeed;
				else
					_velocity.x = MaxSpeed;
			}
			
			// Gravity
			_velocity += Physics.gravity * Time.fixedDeltaTime;
			_controller.Move(_velocity * Time.fixedDeltaTime);
		}
	}
}

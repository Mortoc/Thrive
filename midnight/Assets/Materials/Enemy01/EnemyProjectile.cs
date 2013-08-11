using UnityEngine;
using System.Collections.Generic;

public class EnemyProjectile : MonoBehaviour 
{
	public int Damage;
	
	public float Acceleration;
	public float TopSpeed;
	private float _speed;
	
	private Spaceship _target;
	
	void Start()
	{
		_target = Find.ObjectInScene<Spaceship>();
	}
	
	void Update()
	{
		_speed += Acceleration * Time.deltaTime;
		
		if( _speed > TopSpeed )
			_speed = TopSpeed;
		
		float translation = _speed * Time.deltaTime;
		transform.position += translation * (_target.transform.position - transform.position).normalized;
		
		if( (_target.transform.position - transform.position).sqrMagnitude < (translation * translation) )
		{
			_target.TakeDamage(Damage);
			Destroy(gameObject);
		}
	}
}

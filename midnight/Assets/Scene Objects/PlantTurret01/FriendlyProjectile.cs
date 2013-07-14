using UnityEngine;
using System.Collections.Generic;

public class FriendlyProjectile : MonoBehaviour 
{
	public int Damage;
	
	public float Acceleration = 1500.0f;
	public float TopSpeed = 1500.0f;
	private float _speed = 0.0f;
	
	public Enemy Target;
	public Vector3 _targetOffset;
	
	
	void Update()
	{
		Vector3 targetPosition = _targetOffset + Target.transform.position;
		_speed += Acceleration * Time.deltaTime;
		
		if( _speed > TopSpeed )
			_speed = TopSpeed;
		
		float translation = _speed * Time.deltaTime;
		transform.position += translation * (targetPosition - transform.position).normalized;
		
		if( (targetPosition - transform.position).sqrMagnitude < (translation * translation) )
		{
			Target.TakeDamage(Damage);
			Destroy(gameObject);
		}
	}
}

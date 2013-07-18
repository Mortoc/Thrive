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
	
	public float _initialZ;
	void Start()
	{
		_initialZ = transform.position.z;
	}
	void Update()
	{
		if( !Target )
		{
			Destroy(gameObject);
			return;
		}
		
		Vector3 targetPosition = _targetOffset + Target.transform.position;
		_speed += Acceleration * Time.deltaTime;
		
		if( _speed > TopSpeed )
			_speed = TopSpeed;
		
		float translation = _speed * Time.deltaTime;
		transform.position += translation * (targetPosition - transform.position).normalized;
		transform.position = new Vector3(transform.position.x, transform.position.y, _initialZ);
		
		if( (targetPosition - transform.position).sqrMagnitude < (translation * translation) )
		{
			Target.TakeDamage(Damage);
			Destroy(gameObject);
		}
	}
}

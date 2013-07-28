using UnityEngine;
using System.Collections.Generic;

public class EnemySpawn : MonoBehaviour 
{
	public ParallaxObject ParallaxLayer;
	public GameObject EnemyPrefab;
	public int GroupSizeJitter = 2;
	public int GroupSize = 4;
	
	public float InitialWait = 10.0f;
	public float GroupTime = 3.0f;
	public float GroupRate = 12.0f;
	
	
	void Start()
	{
		if (ParallaxLayer == null)
		{
			throw new System.Exception("Need to specify layer on EnemySpawn");	
		}
		
		Scheduler.Run(Spawner());
	}
	
	private IEnumerator<IYieldInstruction> Spawner()
	{
		yield return new YieldForSeconds(InitialWait);
		
		while( gameObject )
		{
			int thisGroupSize = Random.Range(GroupSize - GroupSizeJitter, GroupSize + GroupSizeJitter);		
			float timePerSpawn = GroupTime / (float)thisGroupSize;
			
			for( int i = 0; i < thisGroupSize; ++i )
			{
				GameObject enemy = (GameObject)Instantiate(EnemyPrefab);
				enemy.transform.position = transform.position;
				
				Enemy en = enemy.GetComponent<Enemy>();
				en.AttachedLayer = ParallaxLayer;
				en.transform.parent = ParallaxLayer.transform.parent;
				
				ParallaxLayer.AddGameObjectToLayer(enemy);
				
				yield return new YieldForSeconds(timePerSpawn);
			}
			
			yield return new YieldForSeconds(GroupRate);
		}
	}
	
}

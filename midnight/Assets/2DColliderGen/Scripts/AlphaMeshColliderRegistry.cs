using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

//-------------------------------------------------------------------------
/// <summary>
/// A data storage class to hold the intermediate results of the collider-generation algorithm
/// and the resulting colliders of all AlphaMeshCollider instances.
/// 
/// Implemented as a singleton.
/// </summary>
[ExecuteInEditMode]
public class AlphaMeshColliderRegistry : MonoBehaviour {

	//-------------------------------------------------------------------------
	/// <summary>
	/// Nested class to hold informations of one group of equal
	/// AlphaMeshColliders that all share the same targetPath,
	/// intermediate results, etc.
	/// </summary>
	[System.Serializable]
	public class ColliderGroup {
		public string mFullColliderMeshPath;
		public PolygonOutlineFromImageFrontend mOutlineAlgorithm = null;
		public List<Vector2> mIntermediateOutlineVertices = null; // TODO: get rid of this type of intermediate result.
		public Mesh mColliderMesh = null;
		public float mAlphaOpaqueThreshold = 0.1f;
		public float mCustomRotation = 0.0f;
		public Vector2 mCustomScale = Vector2.one;
		public Vector3 mCustomOffset = Vector3.zero;
		public List<WeakReference> mAlphaMeshColliderObjects = null;
	}
	//-------------------------------------------------------------------------
	
	
	/// <summary> The singleton instance. </summary>
    private static AlphaMeshColliderRegistry mInstance = null;
	public List<ColliderGroup> mColliderGroups = new List<ColliderGroup>();
	
    
	//-------------------------------------------------------------------------
	public static AlphaMeshColliderRegistry Instance { 
        get {
            if (mInstance == null) {
			
				AlphaMeshColliderRegistry existingSingleton = (AlphaMeshColliderRegistry) GameObject.FindObjectOfType(typeof(AlphaMeshColliderRegistry));
				if (existingSingleton != null) {
					mInstance = existingSingleton;
				}
				else {
		            GameObject gameObject = new GameObject();
		            mInstance = gameObject.AddComponent<AlphaMeshColliderRegistry>();
		            gameObject.name = "AlphaMeshColliderRegistry";
				}
            }
            return mInstance; 
        }
    }
	
	//-------------------------------------------------------------------------
	public void ReInitColliderGroups() {
		
		mColliderGroups.Clear();
		
		
		object[] alphaMeshColliders = GameObject.FindSceneObjectsOfType(typeof(AlphaMeshCollider));
		Array.Sort(alphaMeshColliders, delegate(object first, object second) {
                    							return ((AlphaMeshCollider)first).mColliderMeshFilename.CompareTo(
													   ((AlphaMeshCollider)second).mColliderMeshFilename);
                  						});
		
		foreach (AlphaMeshCollider collider in alphaMeshColliders) {
			
			string colliderMeshPath = collider.FullColliderMeshPath();
			ColliderGroup colliderGroup = FindColliderGroup(colliderMeshPath);
			if (colliderGroup == null) {
				// first AlphaMeshCollider with path colliderMeshPath.
				colliderGroup = new ColliderGroup();
				colliderGroup.mFullColliderMeshPath = colliderMeshPath;
				mColliderGroups.Add(colliderGroup);
			
				AssignValuesFromInstanceToGroup(collider, colliderGroup);
				
				colliderGroup.mAlphaMeshColliderObjects = new List<WeakReference>();				
				colliderGroup.mAlphaMeshColliderObjects.Add(new WeakReference(collider));
			}
			else {
				// not the first one - add it to the list of weak references.
				colliderGroup.mAlphaMeshColliderObjects.Add(new WeakReference(collider));
			}
		}
	}
	
	//-------------------------------------------------------------------------
	void Update() {
        if (!Application.isEditor || Application.isPlaying)
            return;
		
		ReInitColliderGroups();
	}
	
	//-------------------------------------------------------------------------
	public void RecalculateColliderAndUpdateSimilar(AlphaMeshCollider target) {
		if (!target.CanRecalculateCollider)
			return;
		
		target.RecalculateCollider();
		
		UpdateSimilarCollidersAndGroupToTarget(target);
	}
	
	//-------------------------------------------------------------------------
	public void RecalculateColliderFromPreviousResultAndUpdateSimilar(AlphaMeshCollider target) {
		if (!target.CanRecalculateCollider)
			return;
		
		target.RecalculateColliderFromPreviousResult();
		
		UpdateSimilarCollidersAndGroupToTarget(target);
	}
	
	//-------------------------------------------------------------------------
	public void RewriteColliderToFileAndUpdateSimilar(AlphaMeshCollider target) {
		if (!target.CanRewriteCollider)
			return;
		
		target.RewriteColliderToFile();
		
		UpdateSimilarCollidersAndGroupToTarget(target);
	}
	
	//-------------------------------------------------------------------------
	public void ReloadOrRecalculateColliderAndUpdateSimilar(AlphaMeshCollider target) {
		if (!target.CanRecalculateCollider)
			return;
		
		string colliderMeshPath = target.FullColliderMeshPath();
		
		ColliderGroup colliderGroup = FindColliderGroup(colliderMeshPath);
		if (colliderGroup == null || !IsColliderGroupValid(colliderGroup)) {
			target.RecalculateCollider();
			UpdateSimilarCollidersAndGroupToTarget(target);
		}
		else {
			UpdateSimilarCollidersToGroup(colliderGroup);
		}
	}
	
	//-------------------------------------------------------------------------
	public void ReloadOrRecalculateSingleCollider(AlphaMeshCollider target) {
		if (!target.CanReloadCollider)
			return;
		
		string colliderMeshPath = target.FullColliderMeshPath();
		ColliderGroup colliderGroup = FindColliderGroup(colliderMeshPath);
		if (colliderGroup == null || !IsColliderGroupValid(colliderGroup)) {
			if (!target.CanRecalculateCollider)
				return;
			
			target.RecalculateCollider();
			UpdateSimilarCollidersAndGroupToTarget(target);
		}
		else {
			AssignValuesFromColliderGroup(target, colliderGroup);
		}
	}
	
	//-------------------------------------------------------------------------
	protected void UpdateSimilarCollidersToGroup(ColliderGroup colliderGroup) {
		
		string colliderMeshPath = colliderGroup.mFullColliderMeshPath;
		object[] alphaMeshColliders = GameObject.FindSceneObjectsOfType(typeof(AlphaMeshCollider));
		
		colliderGroup.mAlphaMeshColliderObjects = new List<WeakReference>();
		foreach (AlphaMeshCollider collider in alphaMeshColliders)
		{
			if (collider.FullColliderMeshPath().Equals(colliderMeshPath)) {
				
				colliderGroup.mAlphaMeshColliderObjects.Add(new WeakReference(collider));
				
				// reassign previously calculated values.
				AssignValuesFromColliderGroup(collider, colliderGroup);
			}
		}
	}
	
	//-------------------------------------------------------------------------
	protected void UpdateSimilarCollidersAndGroupToTarget(AlphaMeshCollider target) {
		
		string colliderMeshPath = target.FullColliderMeshPath();
		object[] alphaMeshColliders = GameObject.FindSceneObjectsOfType(typeof(AlphaMeshCollider));

		ColliderGroup colliderGroup = FindColliderGroup(colliderMeshPath);
		if (colliderGroup == null) {
			// add new group.
			colliderGroup = new ColliderGroup();
			colliderGroup.mFullColliderMeshPath = colliderMeshPath;
			mColliderGroups.Add(colliderGroup);
		}
		
		AssignValuesFromInstanceToGroup(target, colliderGroup);
		
		colliderGroup.mAlphaMeshColliderObjects = new List<WeakReference>();
		foreach (AlphaMeshCollider collider in alphaMeshColliders)
		{
			if (collider.FullColliderMeshPath().Equals(colliderMeshPath)) {
				
				colliderGroup.mAlphaMeshColliderObjects.Add(new WeakReference(collider));
				
				// reassign previously calculated values.
				AssignValuesFromColliderGroup(collider, colliderGroup);
			}
		}
	}
	
	//-------------------------------------------------------------------------
	protected void AssignValuesFromInstanceToGroup(AlphaMeshCollider target, ColliderGroup colliderGroup) {
		
		colliderGroup.mColliderMesh = target.ColliderMesh;
		colliderGroup.mIntermediateOutlineVertices = target.IntermediateOutlineVertices;
		colliderGroup.mOutlineAlgorithm = target.OutlineAlgorithm;
		colliderGroup.mAlphaOpaqueThreshold = target.mAlphaOpaqueThreshold;
		colliderGroup.mCustomRotation = target.mCustomRotation;
		colliderGroup.mCustomScale = target.mCustomScale;
		colliderGroup.mCustomOffset = target.mCustomOffset;
	}
	
	//-------------------------------------------------------------------------
	public bool AssignValuesFromColliderGroup(AlphaMeshCollider target, string fullMeshPath) {
		
		ColliderGroup colliderGroup = this.FindColliderGroup(fullMeshPath);
		if (colliderGroup == null) {
			return false;
		}
		else {
			AssignValuesFromColliderGroup(target, colliderGroup);
			return true;
		}
	}
	
	//-------------------------------------------------------------------------
	public void AssignValuesFromColliderGroup(AlphaMeshCollider target, ColliderGroup colliderGroup) {
		
		target.ColliderMesh = colliderGroup.mColliderMesh; // sets the sharedMesh to null first, so no need to set it here.
		target.IntermediateOutlineVertices = colliderGroup.mIntermediateOutlineVertices;
		target.OutlineAlgorithm = colliderGroup.mOutlineAlgorithm;
		
		if (colliderGroup.mOutlineAlgorithm != null) {
			target.mVertexReductionDistanceTolerance = colliderGroup.mOutlineAlgorithm.Backend.mVertexReductionDistanceTolerance;
			target.mMaxPointCount = colliderGroup.mOutlineAlgorithm.Backend.mMaxPointCount;
			target.mThickness = colliderGroup.mOutlineAlgorithm.Backend.mThickness;
		}
		target.mAlphaOpaqueThreshold = colliderGroup.mAlphaOpaqueThreshold;
		target.mCustomRotation = colliderGroup.mCustomRotation;
		target.mCustomScale = colliderGroup.mCustomScale;
		target.mCustomOffset = colliderGroup.mCustomOffset;
	}
	
	//-------------------------------------------------------------------------
	/// <summary>
	/// Returns a collider group within mColliderGroups if one with
	/// mFullColliderMeshPath equal to <c>fullMeshPath</c> is found,
	/// null otherwise.
	/// </summary>
	protected ColliderGroup FindColliderGroup(string fullMeshPath) {
		foreach (ColliderGroup colliderGroup in mColliderGroups) {
			if (colliderGroup.mFullColliderMeshPath.Equals(fullMeshPath))
				return colliderGroup;
		}
		return null;
	}
	
	//-------------------------------------------------------------------------
	protected bool IsColliderGroupValid(ColliderGroup colliderGroup) {
		if (colliderGroup.mOutlineAlgorithm == null ||
			colliderGroup.mIntermediateOutlineVertices == null ||
			colliderGroup.mColliderMesh == null) {
			
			return false;
		}
		else {
			return true;
		}
	}
	
	//-------------------------------------------------------------------------
	protected void EnsureColliderGroupIsValid(ColliderGroup colliderGroup) {
		if (IsColliderGroupValid(colliderGroup))
			return;
		
		string colliderMeshPath = colliderGroup.mFullColliderMeshPath;
		object[] alphaMeshColliders = GameObject.FindSceneObjectsOfType(typeof(AlphaMeshCollider));
		
		colliderGroup.mAlphaMeshColliderObjects = new List<WeakReference>();
		AlphaMeshCollider target = null;
		foreach (AlphaMeshCollider collider in alphaMeshColliders)
		{
			if (collider.FullColliderMeshPath().Equals(colliderMeshPath)) {
				
				target = collider;
				colliderGroup.mAlphaMeshColliderObjects.Add(new WeakReference(collider));
			}
		}
		
		if (target != null) {
			AssignValuesFromInstanceToGroup(target, colliderGroup);
		}
	}
	
	//-------------------------------------------------------------------------
	protected void CheckForOutdatedColliderMeshPaths() {
		
		List<ColliderGroup> groupsToRemove = new List<ColliderGroup>();
		
		foreach (ColliderGroup colliderGroup in mColliderGroups) {
			
			if (colliderGroup.mAlphaMeshColliderObjects != null) {
			
				colliderGroup.mAlphaMeshColliderObjects.RemoveAll(
					delegate(WeakReference colliderInstanceRef) {
						bool isOutdated = !((AlphaMeshCollider) colliderInstanceRef.Target).FullColliderMeshPath().Equals(colliderGroup.mFullColliderMeshPath);
						return isOutdated;
				});
				if (colliderGroup.mAlphaMeshColliderObjects.Count == 0) {
					groupsToRemove.Add(colliderGroup);
				}
			}
		}
		
		for (int removeIndex = groupsToRemove.Count - 1; removeIndex >= 0; --removeIndex) {
			mColliderGroups.Remove(groupsToRemove[removeIndex]);
		}
	}
	
	//-------------------------------------------------------------------------
	protected void RemoveEmptyColliderGroups() {
		
		mColliderGroups.RemoveAll(
			delegate(ColliderGroup colliderGroup) {
				bool hasNoInstances = (colliderGroup.mAlphaMeshColliderObjects != null &&
									   colliderGroup.mAlphaMeshColliderObjects.Count == 0);
				if (hasNoInstances)
					Debug.Log("removing group: " + colliderGroup.mFullColliderMeshPath);
				return hasNoInstances;
			});
	}
	
}

#endif // #if UNITY_EDITOR


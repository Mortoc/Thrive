using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

//-------------------------------------------------------------------------
/// <summary>
/// Class that provides the collider mesh generation functionality for
/// 2D Toolkit (TK2D) sprites.
/// </summary>
public class GenerateColliderTK2DHelper {
	
	protected const int POLYGON_COLLIDER_TYPE_INT_VALUE = 4; // NOTE: keep up to date with the ColliderType enum in tk2dSpriteCollection.cs.
	
	protected PolygonOutlineFromImageFrontend mOutlineAlgorithm = null;
	protected IslandDetector mIslandDetector = null;
	protected List<Vector2> mIntermediateOutlineVertices = null; // temporary result, stored to run the algorithm from a common intermediate point instead of from the start.
	protected string mLastError = null;
	
	public string LastError {
		get {
			return mLastError;
		}
	}
	
	// Parameters passed to the PolygonOutlineFromImageFrontend class. See class description for further info.
	Texture2D mMainTex = null;
	bool [,]mBinaryImage = null;
	
	public float mNormalizedAlphaOpaqueThreshold = 0.1f;
	public float mVertexReductionDistanceTolerance = 0.0f;
	public int mMaxPointCount = 30;
	public bool mConvex = false;
	public Texture2D mCustomTex = null;
	public Vector2 mCustomScale = Vector2.one;
	public Vector2 mCustomOffset = Vector2.zero;
	
	public float mXScale = 1.0f;
	public float mYScale = 1.0f;
	public float mXOffsetNormalized = 0.0f; // with [0..1] mapping to [0..width] of the image.
	public float mYOffsetNormalized = 0.0f; // with [0..1] mapping to [0..height] of the image.	
	public bool mFlipInsideOutside = false;
	
	protected Texture2D UsedTexture {
		get {
			return mCustomTex != null ? mCustomTex : mMainTex;
		}
	}
	
	
	//-------------------------------------------------------------------------
	public int GetSpriteID(Component tk2dSpriteComponent) {
		Type componentType = tk2dSpriteComponent.GetType();
		FieldInfo fieldSpriteId = componentType.GetField("_spriteId", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteId == null) {
			Debug.LogError("Detected a missing '_spriteId' member variable at the tk2dSpriteComponent class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return 0;
		}
		return (int) fieldSpriteId.GetValue(tk2dSpriteComponent);
	}
	
	//-------------------------------------------------------------------------
	public bool EnsureColliderTypePolyCollider(object spriteCollectionProxy, int[] spriteIDs) {
		bool wereAllSuccessful = true;
		
		foreach (int spriteID in spriteIDs) {
			object spriteCollectionDefinition = GetTK2DSpriteCollectionDefinition(spriteCollectionProxy, spriteID);
			if (spriteCollectionDefinition == null) {
				// last error is already set in GetTK2DSpriteCollectionDefinition() above.
				wereAllSuccessful = false;
				continue;
			}
			Type spriteCollectionDefinitionType = spriteCollectionDefinition.GetType();
			FieldInfo fieldColliderType = spriteCollectionDefinitionType.GetField("colliderType");
			if (fieldColliderType == null) {
				Debug.LogError("Detected a missing 'colliderType' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return false;
			}
			object enumValue = fieldColliderType.GetValue(spriteCollectionDefinition);
			Type enumType = enumValue.GetType();
			object newEnumValue = Enum.ToObject(enumType, POLYGON_COLLIDER_TYPE_INT_VALUE);
			fieldColliderType.SetValue(spriteCollectionDefinition, newEnumValue);
		}
				
		return wereAllSuccessful;
	}
	
	//-------------------------------------------------------------------------
	public bool GenerateColliderVertices(object spriteCollection, int[] spriteIDs) {
		bool wereAllSuccessful = true;
		
		foreach (int spriteID in spriteIDs) {
			
			mMainTex = GetTextureRef(spriteCollection, spriteID);
			if (mMainTex == null) {
				
				//SetLastError("No sprite texture found at sprite '" + tk2dSpriteComponent.name + "' with spriteID " + spriteID + ".");
				SetLastError("No sprite texture found at sprite with spriteID " + spriteID + ".");
				wereAllSuccessful = false;
				continue;
			}
			
			object spriteCollectionDefinition = GetTK2DSpriteCollectionDefinition(spriteCollection, spriteID);
			if (spriteCollectionDefinition == null) {
				// last error is already set in GetTK2DSpriteCollectionDefinition() above.
				wereAllSuccessful = false;
				continue;
			}
			
			int regionXOffset = 0;
			int regionYOffset = 0;
			int regionWidth = UsedTexture.width;
			int regionHeight = UsedTexture.height;
			bool isRegionUsed = false;
			if (mCustomTex == null) {
				isRegionUsed = ReadRegionParameters(out regionXOffset, out regionYOffset, out regionWidth, out regionHeight, spriteCollectionDefinition);
				regionYOffset = mMainTex.height - regionYOffset - regionHeight;		
			}
			
			if (mOutlineAlgorithm == null) {
				mOutlineAlgorithm = new PolygonOutlineFromImageFrontend();
			}
			mOutlineAlgorithm.BinaryAlphaThresholdImageFromTexture(out mBinaryImage, UsedTexture, mNormalizedAlphaOpaqueThreshold,
																   isRegionUsed, regionXOffset, regionYOffset, regionWidth, regionHeight);
			
			IntVector2[] islandStartingPoints = CalculateIslandStartingPoints(mBinaryImage);
			
			mOutlineAlgorithm.Backend.mVertexReductionDistanceTolerance = this.mVertexReductionDistanceTolerance;
            mOutlineAlgorithm.Backend.mMaxPointCount = this.mMaxPointCount;
            mOutlineAlgorithm.Backend.mConvex = this.mConvex;
            mOutlineAlgorithm.Backend.mXOffsetNormalized = 0.0f + mCustomOffset.x + (0.5f - (0.5f * mCustomScale.x));
            mOutlineAlgorithm.Backend.mYOffsetNormalized = 1.0f - mCustomOffset.y - (0.5f - (0.5f * mCustomScale.y));
            mOutlineAlgorithm.Backend.mXScale = 1.0f * mCustomScale.x;
            mOutlineAlgorithm.Backend.mYScale = -1.0f * mCustomScale.y;
			
			bool reverseVertexOrder = !mFlipInsideOutside; // reverse vertex order is the outside-order because of the -1 y-scale.
            if (mOutlineAlgorithm.Backend.mXScale * mOutlineAlgorithm.Backend.mYScale > 0)
            {
				reverseVertexOrder = !reverseVertexOrder;
			}
			
			bool outputVerticesInNormalizedSpace = false;

            mOutlineAlgorithm.Backend.UnreducedOutlineFromBinaryImage(out mIntermediateOutlineVertices, mBinaryImage, islandStartingPoints, outputVerticesInNormalizedSpace, false);
            List<Vector2> reducedOutline = mOutlineAlgorithm.Backend.ReduceOutline(mIntermediateOutlineVertices);

			if (reverseVertexOrder) {
				reducedOutline.Reverse();
			}
			
			int numIslands = 1; // for now we have only one.
			IEnumerable colliderIslandsArray = PrepareColliderIslands(spriteCollectionDefinition, numIslands);
			
			foreach (object island in colliderIslandsArray) {
				
				Type spriteColliderIslandType = island.GetType();
				FieldInfo fieldPoints = spriteColliderIslandType.GetField("points");
				if (fieldPoints == null) {
					Debug.LogError("Detected a missing 'colliderType' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
					return false;
				}
				fieldPoints.SetValue(island, reducedOutline.ToArray()); // note: by now we only support one island.
			}
		}
		
		return wereAllSuccessful;
	}
	
	//-------------------------------------------------------------------------
	protected IntVector2[] CalculateIslandStartingPoints(bool [,] binaryImage) {
		IntVector2[] islandStartingPoints = null;
		int[,] islandClassificationImage = null;
		IslandDetector.Region[] islands = null;
		IslandDetector.Region[] seaRegions = null;
		
		mIslandDetector = new IslandDetector();
		mIslandDetector.DetectIslandsFromBinaryImage(binaryImage, out islandClassificationImage, out islands, out seaRegions);
		if (islands != null && islands.Length > 0 && islands[0] != null) {
			islandStartingPoints = new IntVector2[1];
			islandStartingPoints[0] = islands[0].mPointAtBorder;
		}
		return islandStartingPoints;
	}
	
	//-------------------------------------------------------------------------
	protected bool ReadRegionParameters(out int regionXOffset, out int regionYOffset, out int regionWidth, out int regionHeight,
										object spriteCollectionDefinition) {
		regionXOffset = 0;
		regionYOffset = 0;
		regionWidth = 0;
		regionHeight = 0;
		
		Type spriteCollectionDefinitionType = spriteCollectionDefinition.GetType();
		FieldInfo fieldExtractRegion = spriteCollectionDefinitionType.GetField("extractRegion");
		if (fieldExtractRegion == null) {
			Debug.LogError("Detected a missing 'extractRegion' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		bool extractRegion = (bool) fieldExtractRegion.GetValue(spriteCollectionDefinition);
		
		FieldInfo fieldRegionX = spriteCollectionDefinitionType.GetField("regionX");
		FieldInfo fieldRegionY = spriteCollectionDefinitionType.GetField("regionY");
		FieldInfo fieldRegionH = spriteCollectionDefinitionType.GetField("regionH");
		FieldInfo fieldRegionW = spriteCollectionDefinitionType.GetField("regionW");
		if (fieldRegionX == null || fieldRegionY == null || fieldRegionH == null || fieldRegionW == null) {
			Debug.LogError("Detected a missing 'fieldRegionX/Y' or 'fieldRegionW/H' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		
		regionXOffset = (int) fieldRegionX.GetValue(spriteCollectionDefinition);
		regionYOffset = (int) fieldRegionY.GetValue(spriteCollectionDefinition);
		regionWidth   = (int) fieldRegionW.GetValue(spriteCollectionDefinition);
		regionHeight  = (int) fieldRegionH.GetValue(spriteCollectionDefinition);
		return extractRegion;
	}
	
	//-------------------------------------------------------------------------
	protected IList PrepareColliderIslands(object spriteCollectionDefinition, int targetNumIslands) {
		
		Type spriteCollectionDefinitionType = spriteCollectionDefinition.GetType();
		FieldInfo fieldPolyColliderIslands = spriteCollectionDefinitionType.GetField("polyColliderIslands");
		if (fieldPolyColliderIslands == null) {
			Debug.LogError("Detected a missing 'polyColliderIslands' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		IList colliderIslandsArray = (IList) fieldPolyColliderIslands.GetValue(spriteCollectionDefinition);
		
		int currentNumIslands = 0;
		Type colliderIslandsArrayType = null;
		Type colliderIslandType = null;
		if (colliderIslandsArray == null) {
			colliderIslandType = Type.GetType("tk2dSpriteColliderIsland");
			IList tempArray = Array.CreateInstance(colliderIslandType, 0);
			colliderIslandsArrayType = tempArray.GetType();
			currentNumIslands = 0;
		}
		else {
			colliderIslandsArrayType = colliderIslandsArray.GetType();
			colliderIslandType = colliderIslandsArrayType.GetElementType();
			currentNumIslands = colliderIslandsArray.Count;
		}
		
		
		if (currentNumIslands != targetNumIslands) {
			colliderIslandsArray = Array.CreateInstance(colliderIslandType, targetNumIslands);
			for (int index = 0; index < targetNumIslands; ++index) {
				colliderIslandsArray[index] = Activator.CreateInstance(colliderIslandType);
			}
			fieldPolyColliderIslands.SetValue(spriteCollectionDefinition, colliderIslandsArray);
		}
		
		foreach (object island in colliderIslandsArray) {
			FieldInfo fieldConnected = colliderIslandType.GetField("connected");
			if (fieldConnected == null) {
				Debug.LogError("Detected a missing 'connected' member variable at TK2D's collider island class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				continue;
			}
			bool trueValue = true;
			fieldConnected.SetValue(island, trueValue);
			// island.points is set later anyway.
		}
		return colliderIslandsArray;
	}
	
	//-------------------------------------------------------------------------
	public object GetTK2DSpriteCollection(Component tk2dSpriteComponent) {
		
		string spriteCollectionGUID = GetSpriteCollectionGUID(tk2dSpriteComponent);
		if (spriteCollectionGUID == null)
			return null;
		
		string path = AssetDatabase.GUIDToAssetPath(spriteCollectionGUID);
		object spriteCollection = AssetDatabase.LoadAssetAtPath(path, typeof(MonoBehaviour));
		if (spriteCollection == null) {
			SetLastError("Failed to load sprite collection at path " + path + ".");
			return null;
		}		
		return spriteCollection;
	}
	
	//-------------------------------------------------------------------------
	object GetTK2DSpriteCollectionDefinition(object spriteCollection, int spriteID) {
		
		// Actually does this:
		// tk2dSpriteCollectionDefinition[] collectionDefArray = spriteCollection.textureParams;
		// tk2dSpriteCollectionDefinition collectionDef = collectionDefArray[spriteID];
		
		Type spriteCollectionType = spriteCollection.GetType();
		FieldInfo fieldTextureParams = spriteCollectionType.GetField("textureParams"); // NOTE: the name textureParams is a bit misleading.
		if (fieldTextureParams == null) {
			Debug.LogError("Detected a missing 'textureParams' member variable at TK2D's sprite collection class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		
		IEnumerable textureParams = (IEnumerable) fieldTextureParams.GetValue(spriteCollection);
		object spriteCollectionDef = null;
		int index = 0;
		foreach (object currentSpriteCollectionDef in textureParams) {
			if (index++ == spriteID) {
				spriteCollectionDef = currentSpriteCollectionDef;
				break;
			}
		}
		
		if (spriteCollectionDef == null) {
			SetLastError("No sprite collection definition found at spriteID " + spriteID + ".");
		}
		
		return spriteCollectionDef;
	}
	
	//-------------------------------------------------------------------------
	string GetSpriteCollectionGUID(Component tk2dSpriteComponent) {
		// Actually does this:
		// tk2dSpriteCollectionData spriteCollectionData = tk2dSpriteComponent.collection
		Type componentType = tk2dSpriteComponent.GetType().BaseType;
        FieldInfo fieldCollection = componentType.GetField("collection", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldCollection == null) {
			Debug.LogError("Detected a missing 'collection' member variable at the tk2dSpriteComponent class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}		
		object spriteCollectionData = fieldCollection.GetValue(tk2dSpriteComponent);
		if (spriteCollectionData == null) {
			SetLastError("No sprite collection data found at sprite '" + tk2dSpriteComponent.name + "'.");
			return null;
		}
		// Actually does this:
		// tk2dSpriteDefinition[] spriteDefinitions = spriteCollection.spriteDefinitions
		Type spriteCollectionDataType = spriteCollectionData.GetType();
		FieldInfo fieldSpriteCollectionGUID = spriteCollectionDataType.GetField("spriteCollectionGUID");
		if (fieldSpriteCollectionGUID == null) {
			Debug.LogError("Detected a missing 'spriteCollectionGUID' member variable at the spriteCollectionData class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		
		string spriteCollectionGUID = (string) fieldSpriteCollectionGUID.GetValue(spriteCollectionData);
		return spriteCollectionGUID;
	}
	
	//-------------------------------------------------------------------------
	Texture2D GetTextureRef(object spriteCollection, int spriteID) {
		
		Texture2D texture = null;
		Type spriteCollectionType = spriteCollection.GetType();
		// first we test for the textureRefs (older version of TK2D), simpler to access.
		FieldInfo fieldTextureRefs = spriteCollectionType.GetField("textureRefs");
		if (fieldTextureRefs != null) {
			IEnumerable textureRefs = (IEnumerable) fieldTextureRefs.GetValue(spriteCollection);
			
			int index = 0;
			foreach (Texture2D currentTexture in textureRefs) {
				if (index++ == spriteID) {
					texture = currentTexture;
					break;
				}
			}
			return texture;
		}
		else {
			// now we test for the sprite collection definition (new version of TK2D, 'textureRefs' member is gone).
			object spriteCollectionDefinition = GetTK2DSpriteCollectionDefinition(spriteCollection, spriteID);
			if (spriteCollectionDefinition == null) {
				// last error is already set in GetTK2DSpriteCollectionDefinition() above.
				return null;
			}
			
			Type spriteCollectionDefinitionType = spriteCollectionDefinition.GetType();
			FieldInfo fieldTexture = spriteCollectionDefinitionType.GetField("texture");
			if (fieldTexture == null) {
				Debug.LogError("Detected a missing 'texture' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return null;
			}
			texture = (Texture2D) fieldTexture.GetValue(spriteCollectionDefinition);
		}
		return texture;
	}
	
	//-------------------------------------------------------------------------
	void SetLastError(string description) {
		mLastError = description;
	}
}

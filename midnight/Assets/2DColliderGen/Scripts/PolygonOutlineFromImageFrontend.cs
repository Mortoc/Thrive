#if UNITY_EDITOR	

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//-------------------------------------------------------------------------
/// <summary>
/// Frontend class to deal with UnityEditor specific tasks which the
/// Runtime-dll is not allowed to access, but the script can.
/// The backend part is directly exposed via the .Backend member. So
/// methods and members other than BinaryAlphaThresholdImageFromTexture()
/// can be accessed by calling PolygonOutlineFromImageFrontend.Backend.method().
/// </summary>
public class PolygonOutlineFromImageFrontend {

    private Texture2D mLastSubImageTexture = null;
    private bool mLastUseRegion = false;
    private int mLastSubImageX = 0;
    private int mLastSubImageY = 0;
    private int mLastSubImageWidth = 0;
    private int mLastSubImageHeight = 0;
    private Color32[] mLastSubImage = null;

    private PolygonOutlineFromImage mBackend = new PolygonOutlineFromImage();
    public PolygonOutlineFromImage Backend {
        get {
            return mBackend;
        }
    }

    public string LastError
    {
        get
        {
            return mBackend.GetLastError();
        }
    }

	//-------------------------------------------------------------------------
	/// <param name='binaryImage'>
	/// A bool array representing the resuling threshold image. In row-major order, thus accessed binaryImage[y,x].
	/// </param>
	public bool BinaryAlphaThresholdImageFromTexture(out bool[,] binaryImage, Texture2D texture, float normalizedAlphaThreshold,
													 bool useRegion, int regionX, int regionYFromTop, int regionWidth, int regionHeight) {
		
		bool isCachedSubImageUpToDate = IsCachedImageUpToDate(texture, useRegion, regionX, regionYFromTop, regionWidth, regionHeight);
		if (!isCachedSubImageUpToDate) {
			if (!ReadAndCacheSubImage(texture, useRegion, regionX, regionYFromTop, regionWidth, regionHeight)) { // this method calls SetLastError with a precise message.
				binaryImage = null;
				return false;
			}
		}
		
		
		byte alphaThreshold8Bit = (byte)(normalizedAlphaThreshold * 255.0f);
		
		int width = mLastSubImageWidth;
		int height = mLastSubImageHeight;
		binaryImage = new bool[height, width];
		
		// NOTE: mainTexPixels is read from bottom left origin upwards.
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				
				byte alpha = mLastSubImage[y * width + x].a;
				if (alpha >= alphaThreshold8Bit)
					binaryImage[y,x] = true;
				else
					binaryImage[y,x] = false;
			}
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	private bool IsCachedImageUpToDate(Texture2D texture, bool useRegion,
										 int regionX, int regionYFromTop, int regionWidth, int regionHeight) {
		
		return (mLastSubImageTexture == texture &&
			    mLastUseRegion == useRegion &&
			    mLastSubImageX == regionX &&
			    mLastSubImageY == regionYFromTop &&
			    mLastSubImageWidth == regionWidth &&
			    mLastSubImageHeight == regionHeight &&
			    mLastSubImage != null);
	}
	
	//-------------------------------------------------------------------------
	private bool ReadAndCacheSubImage(Texture2D texture, bool useRegion,
										int regionX, int regionYFromTop, int regionWidth, int regionHeight) {
		
		Color32[] texturePixels = null;
		try {
			texturePixels = texture.GetPixels32();
		}
		catch (System.Exception ) {
			// expected behaviour, if the texture is read-only.
		}
			
		bool wasTextureReadOnly = (texturePixels == null);
		if (wasTextureReadOnly) {
			if (!SetTextureReadable(texture, true)) {
				SetLastError("Unable to set the texture '" + texture.name + "' to readable state. Aborting collider mesh generation. Please set the texture's readable flag manually via the editor.");
				return false;
			}
			
			texturePixels = texture.GetPixels32();
		}
		
		int destWidth = texture.width;
		int destHeight = texture.height;
		int srcRegionOffsetX = 0;
		int srcRegionOffsetY = 0;  // = bottom left origin
		
		if (!useRegion) {
			mLastSubImage = texturePixels;
		}
		else {
			destWidth = regionWidth;
			destHeight = regionHeight;
			srcRegionOffsetX = regionX;
			srcRegionOffsetY = texture.height - destHeight - regionYFromTop; // regionYFromTop is measured from top(-left) origin to the top of the region.
			
			mLastSubImage = new Color32[destHeight * destWidth];
		
			// NOTE: mainTexPixels is read from bottom left origin upwards.
			for (int destY = 0; destY < destHeight; ++destY) {
				for (int destX = 0; destX < destWidth; ++destX) {
					int srcX = destX + srcRegionOffsetX;
					int srcY = destY + srcRegionOffsetY;
					int destIndex = destY * destWidth + destX;
					int srcIndex = srcY * texture.width + srcX;
					mLastSubImage[destIndex] = texturePixels[srcIndex];
				}
			}
		}
		
		mLastSubImageTexture = texture;
		mLastUseRegion = useRegion;
		mLastSubImageX = regionX;
		mLastSubImageY = regionYFromTop;
		mLastSubImageWidth = destWidth;
		mLastSubImageHeight = destHeight;
		
		if (wasTextureReadOnly) {
			if (!SetTextureReadable(texture, false)) {
				SetLastError("Unable to set the texture '" + texture.name + "' back to read-only state. Continuing anyway, please set the texture's readable flag manually via the editor.");
				return false;
			}
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	public bool SetTextureReadable(Texture2D texture, bool readable) {
		string texturePath = UnityEditor.AssetDatabase.GetAssetPath(texture); 
		if (!System.IO.File.Exists(texturePath)) {
			SetLastError("Aborting Generation: Texture at path " + texturePath + " not found while changing import settings - did you delete it?");
			return false;
		}
		
		UnityEditor.TextureImporter textureImporter = (UnityEditor.TextureImporter) UnityEditor.AssetImporter.GetAtPath(texturePath);
        textureImporter.isReadable = readable;
        UnityEditor.AssetDatabase.ImportAsset(texturePath);

		return true;
	}

    //-------------------------------------------------------------------------
	private void SetLastError(string description) {
        mBackend.SetLastError(description);
	}
	
}

#endif // UNITY_EDITOR

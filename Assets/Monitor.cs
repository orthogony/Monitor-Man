using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MonitorMan
{
	public class Monitor : MonoBehaviour
	{
		[SerializeField]
		MeshRenderer screen;

		Material screenMat;
		
		Texture2D screenTexture;

		int xstart, screenWidth, ystart, screenHeight; // all in pixels

		// Use this for initialization
		void Start()
		{
			Assert.IsNotNull(screen);
			screenMat = screen.material;
		}

		//internal void SetParameters(float videoWidth, float videoHeight, float startXPct, float endXPct, float startYPct, float endYPct)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pixelsToUnits">the number of video pixels per engine unit (eg "100" means that a 1920x1080p video is 19.2 by 10.8 units [usually meters] in the game world)</param>
		/// <param name="videoWidth">in pixels</param>
		/// <param name="videoHeight">in pixels</param>
		/// <param name="xPos">center of the monitor as a fraction of width of the array</param>
		/// <param name="yPos"></param>
		/// <param name="xFrac">size of the monitor as a fraction of the width of the array</param>
		/// <param name="yFrac"></param>
		internal void SetParameters(float pixelsToUnits, float videoWidth, float videoHeight, float xPos, float yPos, float xFrac, float yFrac)
		{
			screenWidth = Mathf.RoundToInt(xFrac * videoWidth);
			screenHeight = Mathf.RoundToInt(yFrac * videoHeight);
			/*xstart = Mathf.RoundToInt((float)videoWidth * startXPct);
			screenWidth = Mathf.RoundToInt((float)videoWidth * endXPct) - xstart;
			ystart = Mathf.RoundToInt((float)videoHeight * startYPct);
			screenHeight = Mathf.RoundToInt((float)videoHeight * endYPct) - ystart;*/

			//screenTexture = new Texture2D((int)videoPlayer.clip.width, (int)videoPlayer.clip.height, TextureFormat.ARGB32, false);
			screenTexture = new Texture2D(screenWidth, screenHeight, TextureFormat.ARGB32, false);
		}

		internal void Display(Texture fullFrameTedxture)
		{
			Graphics.CopyTexture(fullFrameTedxture, 0, 0, xstart, ystart, screenWidth, screenHeight, screenTexture, 0, 0, 0, 0);

			screenMat.mainTexture = screenTexture;
		}
	}
}

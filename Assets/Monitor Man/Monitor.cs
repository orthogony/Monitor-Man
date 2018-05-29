﻿using System;
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

		// These two are the size of the screen relative to the overall monitor (eg .9 means a monitor that is 90% screen, 10% border)
		float screenXscale;
		float screenYscale;
		
		void Initialize()
		{
			Assert.IsNotNull(screen);
			screenMat = screen.material;

			screenXscale = screen.transform.localScale.x;
			screenYscale = screen.transform.localScale.y;
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
			Initialize();
			//Debug.Log("Parametersa re " + pixelsToUnits + ", " + xPos + ", " + xFrac);

			var monitorWidth = xFrac * videoWidth;
			var monitorHeight = yFrac * videoHeight;

			var centerX = videoWidth * xPos; // in unrounded pixels
			var centerY = videoHeight * yPos; // in unrounded pixels

			//Debug.Log("Center x and y are " + centerX + ", " + centerY);

			transform.localPosition = new Vector3((centerX - videoWidth / 2) / pixelsToUnits, (centerY - videoHeight / 2) / pixelsToUnits, 0);
			transform.rotation = Quaternion.identity;

			//Debug.Log("x scale is shaping up 
			transform.localScale = new Vector3(monitorWidth / pixelsToUnits, monitorHeight / pixelsToUnits, 1);
			
			screenWidth = Mathf.RoundToInt(monitorWidth * screenXscale);
			screenHeight = Mathf.RoundToInt(monitorHeight * screenYscale);

			xstart = Mathf.RoundToInt(centerX - screenWidth / 2);
			ystart = Mathf.RoundToInt(centerY - screenHeight / 2);

			if (xstart < 0)
			{
				Debug.LogWarning("it's " + xstart);
			}
			if (xstart + screenWidth > videoWidth)
			{
				Debug.LogWarning("it's " + (xstart + screenWidth));
			}
			if (ystart < 0)
			{
				Debug.LogWarning("it's " + ystart);
			}
			if (ystart + screenHeight > videoHeight)
			{
				Debug.LogWarning("it's " + (ystart + screenHeight));
			}
			
			screenTexture = new Texture2D(screenWidth, screenHeight, TextureFormat.ARGB32, false);
		}

		internal void Display(Texture fullFrameTedxture)
		{
			Graphics.CopyTexture(fullFrameTedxture, 0, 0, xstart, ystart, screenWidth, screenHeight, screenTexture, 0, 0, 0, 0);

			screenMat.mainTexture = screenTexture;
			screenMat.SetTexture("_EmissionMap", screenTexture);
			//screenMat
		}
	}
}
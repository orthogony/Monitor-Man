﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;

namespace MonitorMan
{
	[RequireComponent(typeof(VideoPlayer))]
	public class MonitorArray : MonoBehaviour
	{
		[SerializeField]
		protected Monitor monitorPrefab;

		VideoPlayer videoPlayer;

		List<Monitor> monitors = new List<Monitor>();

		public float widthInUnits = 5f;

		public float monitorSizeFactor = 0.9f;

		public enum ArrayShapes
		{
			GRID,
			CLUMPED_GRID
		}

		public ArrayShapes arrayShape = ArrayShapes.GRID;

		public int arrayWidth = 3;
		public int arrayHeight = 3;

		public float clumpingFactor = 0.2f;

		public float squareMonitorBias = 0.1f;

		// Use this for initialization
		void Start()
		{
			videoPlayer = GetComponent<VideoPlayer>();

			Assert.AreEqual(VideoRenderMode.APIOnly, videoPlayer.renderMode, "Video player must be set to API render mode to be used with monitor array");

			switch (arrayShape)
			{
				case ArrayShapes.GRID:
					CreateMonitorArray();
					break;
				case ArrayShapes.CLUMPED_GRID:
					CreateClumpedMonitorArray();
					break;
			}
		}

		private void OnDrawGizmos()
		{
			videoPlayer = GetComponent<VideoPlayer>();

			Gizmos.color = Color.yellow;

			if (videoPlayer.clip != null)
			{
				Gizmos.DrawWireCube(transform.position, new Vector3(widthInUnits, widthInUnits * videoPlayer.clip.height / videoPlayer.clip.width, .3f));
			}
			else
			{
				Gizmos.DrawWireCube(transform.position, new Vector3(widthInUnits, widthInUnits * 10 / 16f, .3f));
			}
		}

		private void CreateClumpedMonitorArray()
		{
			var scale = videoPlayer.clip.width / widthInUnits;

			var fullXFrac = 1 / (float)arrayWidth;
			var fullYFrac = 1 / (float)arrayHeight;

			int[,] occupied = new int[arrayWidth, arrayHeight];

			for (int i = 0; i < arrayWidth; i++)
			{
				for (int j = 0; j < arrayHeight; j++)
				{
					if (occupied[i, j] != 1)
					{
						var m = Instantiate<Monitor>(monitorPrefab, transform);

						monitors.Add(m);

						//m.SetParameters(videoPlayer.clip.width, videoPlayer.clip.height, 0, 0, 1, 1);
						var xPos = (i) / (float)(arrayWidth) + fullXFrac / 2f;
						var yPos = (j) / (float)(arrayHeight) + fullYFrac / 2f;

						var xFrac = fullXFrac;
						var yFrac = fullYFrac;

						if (UnityEngine.Random.value < clumpingFactor)
						{
							// double hit; make this a 2x2 monitor
							if (UnityEngine.Random.value < squareMonitorBias)
							{ // double hit; make this a 3x3 monitor
								if (UnityEngine.Random.value < squareMonitorBias)
								{
									if (i < arrayWidth - 2 && j < arrayHeight - 2 &&
									(occupied[i + 1, j] == 0 && occupied[i + 2, j] == 0 &&
									occupied[i, j + 1] == 0 && occupied[i + 1, j + 1] == 0 && occupied[i + 2, j + 1] == 0 &&
									occupied[i, j + 2] == 0 && occupied[i + 1, j + 2] == 0 && occupied[i + 2, j + 2] == 0))
									{
										occupied[i + 1, j] = 1;
										occupied[i + 2, j] = 1;

										occupied[i, j + 1] = 1;
										occupied[i + 1, j + 1] = 1;
										occupied[i + 2, j + 1] = 1;

										occupied[i, j + 2] = 1;
										occupied[i + 1, j + 2] = 1;
										occupied[i + 2, j + 2] = 1;

										xPos += xFrac;
										yPos += yFrac;

										xFrac *= 3;
										yFrac *= 3;
									}
								}
								else
								{
									if (i < arrayWidth - 1 && j < arrayHeight - 1 &&
									(occupied[i + 1, j] == 0 && occupied[i, j + 1] == 0 && occupied[i + 1, j + 1] == 0))
									{
										occupied[i + 1, j] = 1;
										occupied[i, j + 1] = 1;
										occupied[i + 1, j + 1] = 1;

										xPos += xFrac / 2f;
										yPos += yFrac / 2f;

										xFrac *= 2;
										yFrac *= 2;
									}
								}
							}
							else
							{
								if (UnityEngine.Random.value < 0.5f)
								{
									if (i < arrayWidth - 1 && occupied[i + 1, j] == 0)
									{
										occupied[i + 1, j] = 1;
										xPos += xFrac / 2f;
										xFrac *= 2;
									}
								}
								else
								{
									if (j < arrayHeight - 1 && occupied[i, j + 1] == 0)
									{
										occupied[i, j + 1] = 1;
										yPos += yFrac / 2f;
										yFrac *= 2;
									}
								}
							}
						}

						m.SetParameters(scale, videoPlayer.clip.width, videoPlayer.clip.height, xPos, yPos, xFrac * monitorSizeFactor, yFrac * monitorSizeFactor);

						occupied[i, j] = 1;
					}
				}
			}
		}

		private void CreateMonitorArray()
		{
			var scale = videoPlayer.clip.width / widthInUnits;

			var xFrac = 1 / (float)arrayWidth;
			var yFrac = 1 / (float)arrayHeight;

			for (int i = 0; i < arrayWidth; i++)
			{
				for (int j = 0; j < arrayHeight; j++)
				{
					var m = Instantiate<Monitor>(monitorPrefab, transform);

					monitors.Add(m);
					//m.SetParameters(videoPlayer.clip.width, videoPlayer.clip.height, 0, 0, 1, 1);
					var xPos = (i) / (float)(arrayWidth) + xFrac / 2f;
					var yPos = (j) / (float)(arrayHeight) + yFrac / 2f;
					m.SetParameters(scale, videoPlayer.clip.width, videoPlayer.clip.height, xPos, yPos, xFrac * monitorSizeFactor, yFrac * monitorSizeFactor);
				}
			}
		}

#if false
		private void CreateMonitorSpiral()
		{
			var scale = videoPlayer.clip.width / widthInUnits;

			var numMonitorsInSpiral = arrayWidth * arrayHeight; // for now...

			float rectX = 0; // where our "outer" rectangle is
			float rectY = 0;

			float rectYFrac = 1; // what percentage of the whole rectangle our "outer" rectangle occupies
			float rectXFrac = 1;

			for (int i = 0; i < numMonitorsInSpiral; i++)
			{
				var m = Instantiate<Monitor>(monitorPrefab, transform);

				monitors.Add(m);

				if (rectXFrac * videoPlayer.clip.width > rectYFrac * videoPlayer.clip.height)
				{
					Debug.Log("Wider than tall; rect is " + rectX + ", " + rectY + " and dims " + rectXFrac + ", " + rectYFrac);
					var innerXFrac = (videoPlayer.clip.height * rectYFrac) / (videoPlayer.clip.width * rectXFrac);
					Debug.Log("x fraction is " + innerXFrac);
					m.SetParameters(scale, videoPlayer.clip.width, videoPlayer.clip.height, rectX + innerXFrac / 2f, rectY + rectYFrac / 2f, innerXFrac * monitorSizeFactor, rectYFrac * monitorSizeFactor);
					rectX += innerXFrac;
					rectXFrac -= innerXFrac;
				}
				else
				{
					var innerYFrac = (videoPlayer.clip.width * rectXFrac) / (videoPlayer.clip.height * rectYFrac);
					m.SetParameters(scale, videoPlayer.clip.width, videoPlayer.clip.height, rectX + rectXFrac / 2f, rectY + innerYFrac / 2f, rectXFrac * monitorSizeFactor, innerYFrac * monitorSizeFactor);
					rectY += innerYFrac;
					rectYFrac -= innerYFrac;
				}

			}
		}
#endif

		// Update is called once per frame
		void Update()
		{
			if (videoPlayer.texture != null)
			{
				foreach (var m in monitors)
				{
					m.Display(videoPlayer.texture);
				}
			}
		}
	}
}

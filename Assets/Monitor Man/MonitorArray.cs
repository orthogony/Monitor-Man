using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;

namespace MonitorMan
{
	[RequireComponent(typeof(VideoPlayer))]
	[ExecuteInEditMode]
	public class MonitorArray : MonoBehaviour
	{
		[SerializeField]
		[Range(0, 10)]
		private float m_borderSize = 0.1f;

		[SerializeField]
		protected Monitor m_monitorPrefab;

		VideoPlayer videoPlayer;

		[SerializeField]
		List<Monitor> monitors = new List<Monitor>();

		[Range(0.1f, 50f)]
		[SerializeField]
		float m_widthInUnits = 5f;

		[SerializeField]
		[Range(0.5f, 0.99f)]
		float m_monitorSizeFactor = 0.9f;

		public enum ArrayShapes
		{
			GRID,
			CLUMPED_GRID
		}

		[SerializeField]
		private ArrayShapes m_arrayShape = ArrayShapes.GRID;
		
		[SerializeField]
		[Range(1, 20)]
		int m_arrayWidth = 3;
		[SerializeField]
		[Range(1, 20)]
		int m_arrayHeight = 3;

		private float squareMonitorBias = 0.2f;
		
		float clumpingFactor = 4f;

		RenderTexture videoRenderTexture;

		//float squareMonitorBias = 0.4f;

		// Use this for initialization
		public void Start()
		{
			DestroyMonitors();
			videoPlayer = GetComponent<VideoPlayer>();

			videoRenderTexture = new RenderTexture((int)videoPlayer.clip.width, (int)videoPlayer.clip.height, 0);
			videoPlayer.targetTexture = videoRenderTexture;
			//Assert.AreEqual(VideoRenderMode.APIOnly, videoPlayer.renderMode, "Video player must be set to API render mode to be used with monitor array");

			switch (m_arrayShape)
			{
				case ArrayShapes.GRID:
					CreateMonitorArray();
					break;
				case ArrayShapes.CLUMPED_GRID:
					CreateClumpedMonitorArray();
					break;
			}
		}
		
		// NB used to be the OnValidate method
		public void ResizeMonitors()
		{
			// Just to make sure we have no empties for whatever reason
			monitors = monitors.Where(m => m != null).ToList();
			
			monitors.ForEach(m => m.SetBorderSize(m_borderSize));
		}
		
		private void DestroyMonitors()
		{
			foreach (var m in monitors)
			{
				if (m != null)
				{
					if (Application.isPlaying)
					{
						// Because they won't be destroyed immediately, i do this so they don't cause any trouble
						m.gameObject.SetActive(false);
						Destroy(m.gameObject);
					}
					else
					{
						DestroyImmediate(m.gameObject);
					}
				}
			}

			// TODO ?? for some reason my monitors list is getting reset to 0 every time.  it serialized fine when i play the game but it's broke in the eidtor fer some reason
			if (!Application.isPlaying)
			{
				foreach (Transform t in transform)
				{
					DestroyImmediate(t.gameObject);
				}
			}

			monitors.Clear();
		}

		private void OnDrawGizmos()
		{
			videoPlayer = GetComponent<VideoPlayer>();

			Gizmos.color = Color.yellow;

			if (videoPlayer.clip != null)
			{
				Gizmos.DrawWireCube(transform.position, new Vector3(m_widthInUnits, m_widthInUnits * videoPlayer.clip.height / videoPlayer.clip.width, .3f));
			}
			else
			{
				Gizmos.DrawWireCube(transform.position, new Vector3(m_widthInUnits, m_widthInUnits * 10 / 16f, .3f));
			}
		}

		private void CreateClumpedMonitorArray()
		{
			var fullXFrac = 1 / (float)m_arrayWidth;
			var fullYFrac = 1 / (float)m_arrayHeight;

			bool[,] occupied = new bool[m_arrayWidth, m_arrayHeight];

			for (int i = 0; i < m_arrayWidth; i++)
			{
				for (int j = 0; j < m_arrayHeight; j++)
				{
					if (occupied[i, j] == false)
					{
						var m = Instantiate<Monitor>(m_monitorPrefab, transform);

						monitors.Add(m);

						//m.SetParameters(videoPlayer.clip.width, videoPlayer.clip.height, 0, 0, 1, 1);
						var xPos = (i) / (float)(m_arrayWidth) + fullXFrac / 2f;
						var yPos = (j) / (float)(m_arrayHeight) + fullYFrac / 2f;

						var xFrac = fullXFrac;
						var yFrac = fullYFrac;

						Clump(i, j, occupied, ref xPos, ref yPos, ref xFrac, ref yFrac);

						m.SetParameters(videoRenderTexture, m_widthInUnits, m_widthInUnits * videoPlayer.clip.height / videoPlayer.clip.width, m_borderSize, xPos, yPos, xFrac * m_monitorSizeFactor, yFrac * m_monitorSizeFactor);

						occupied[i, j] = true;
					}
				}
			}
		}

		private bool IsAreaUnoccupied(bool[,] occupied, int x, int y, int xdim, int ydim)
		{
			for (int i = x; i < x + xdim; i++)
			{
				for (int j = y; j < y + ydim; j++)
				{
					if (occupied[i, j])
						return false;
				}
			}
			return true;
		}

		private void ReserveArea(bool[,] occupied, int x, int y, int xdim, int ydim)
		{
			for (int i = x; i < x + xdim; i++)
			{
				for (int j = y; j < y + ydim; j++)
				{
					occupied[i, j] = true;
				}
			}
		}

		private void Clump(int i, int j, bool[,] occupied, ref float xPos, ref float yPos, ref float xFrac, ref float yFrac)
		{
			var xDim = Mathf.CeilToInt(UnityEngine.Random.value * clumpingFactor);
			int yDim = xDim;
			// If we roll a non-square monitor, we have to reroll for the ydim
			if (UnityEngine.Random.value > squareMonitorBias)
			{
				yDim = Mathf.CeilToInt(UnityEngine.Random.value * clumpingFactor);
			}
			if (i < m_arrayWidth - (xDim - 1) && j < m_arrayHeight - (yDim - 1) && IsAreaUnoccupied(occupied, i, j, xDim, yDim))
			{
				ReserveArea(occupied, i, j, xDim, yDim);
				xPos += xFrac * (xDim - 1) / 2f;
				yPos += yFrac * (yDim - 1) / 2f;

				xFrac *= xDim;
				yFrac *= yDim;
			}
		}

		private void CreateMonitorArray()
		{
			var xFrac = 1 / (float)m_arrayWidth;
			var yFrac = 1 / (float)m_arrayHeight;

			for (int i = 0; i < m_arrayWidth; i++)
			{
				for (int j = 0; j < m_arrayHeight; j++)
				{
					var m = Instantiate<Monitor>(m_monitorPrefab, transform);
					
					monitors.Add(m);
					//m.SetParameters(videoPlayer.clip.width, videoPlayer.clip.height, 0, 0, 1, 1);
					var xPos = (i) / (float)(m_arrayWidth) + xFrac / 2f;
					var yPos = (j) / (float)(m_arrayHeight) + yFrac / 2f;
					m.SetParameters(videoRenderTexture, m_widthInUnits, m_widthInUnits * videoPlayer.clip.height / videoPlayer.clip.width, m_borderSize, xPos, yPos, xFrac * m_monitorSizeFactor, yFrac * m_monitorSizeFactor);
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
	}
}

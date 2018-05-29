using System.Collections;
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

		public int arrayWidth = 3;
		public int arrayHeight = 3;

		// Use this for initialization
		void Start()
		{
			videoPlayer = GetComponent<VideoPlayer>();

			Assert.AreEqual(VideoRenderMode.APIOnly, videoPlayer.renderMode, "Video player must be set to API render mode to be used with monitor array");

			CreateMonitorArray();
		}

		private void CreateMonitorArray()
		{
			for (int i = 0; i < arrayWidth; i++)
			{
				for (int j = 0; j < arrayHeight; j++)
				{
					var m = Instantiate<Monitor>(monitorPrefab, transform);

					monitors.Add(m);
					m.SetParameters(videoPlayer.clip.width, videoPlayer.clip.height, 0, 0, 1, 1);
				}
			}
		}

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

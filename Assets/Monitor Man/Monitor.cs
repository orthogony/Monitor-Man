using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace MonitorMan
{
	[ExecuteInEditMode]
	public class Monitor : MonoBehaviour
	{
		[SerializeField]
		MeshRenderer screen;

		new Rigidbody rigidbody;

		new Collider collider;

		Material screenMat;
		
		Texture2D screenTexture;

		int xstart, screenWidth, ystart, screenHeight; // all in pixels

		Vector3PID positionController = new Vector3PID();
		Vector3PID velocityController = new Vector3PID();

		QuaternionPID rotationController = new QuaternionPID();
		Vector3PID angVelocityController = new Vector3PID();

		// These two are the size of the screen relative to the overall monitor (eg .9 means a monitor that is 90% screen, 10% border)
		/*float screenXscale;
		float screenYscale;*/
		
		// the localposition it's supposed to be at
		private Vector3 rootPosition;

		private bool stuck = false;
		private Vector3 lastPosition = Vector3.zero;

		private HashSet<GameObject> ignoredCollisions = new HashSet<GameObject>();

		[SerializeField]
		[HideInInspector]
		private float monitorWidth;
		[SerializeField]
		[HideInInspector]
		private float monitorHeight;
		[SerializeField]
		[HideInInspector]
		private float centerX;
		[SerializeField]
		[HideInInspector]
		private float centerY;

		private float borderSizeInPixels = 7f;

		void Initialize()
		{
			rigidbody = GetComponent<Rigidbody>();

			collider = GetComponentInChildren<Collider>();
			Assert.IsNotNull(collider);

			Assert.IsNotNull(screen);
#if UNITY_EDITOR
			screenMat = screen.sharedMaterial;
#else
			screenMat = screen.material;
#endif

			/*screenXscale = screen.transform.localScale.x;
			screenYscale = screen.transform.localScale.y;*/
		}

		private void Start()
		{
			rigidbody = GetComponent<Rigidbody>();
		}

		// For some reason this will get called before initialize and after start, even though initialize is called after start?
		private void FixedUpdate()
		{
			DoStuckCheck();

			var posCorrection = positionController.GetOutput(rigidbody.position, rootPosition, Time.fixedDeltaTime);
			var velCorrection = velocityController.GetOutput(rigidbody.velocity, Vector3.zero, Time.fixedDeltaTime);

			var force = posCorrection + velCorrection;

			//Debug.Log("Position correction is " + force.ToString("N5"));

			rigidbody.AddForce(force);

			var rotCorrection = rotationController.GetOutput(rigidbody.rotation, Time.fixedDeltaTime);
			var angVelCorrection = angVelocityController.GetOutput(rigidbody.angularVelocity, Vector3.zero, Time.fixedDeltaTime);

			var torque = rotCorrection + angVelCorrection;

			// Debug.Log("Rot correction is " + rotCorrection + " from " + m_rigidBody.rotation.eulerAngles + " and torque add is " + torque);
			rigidbody.AddTorque(torque * 0.2f);
		}

		private void DoStuckCheck()
		{
			//var offset = Vector3.Distance(rigidbody.position, rootPosition);
			if (Vector3.Distance(rigidbody.position, rootPosition) > 0.01f)
			{
				if (Vector3.Distance(rigidbody.position, lastPosition) < 0.001f * Time.fixedDeltaTime)
				{
					stuck = true;
					//Debug.Log("Got stuck; distance is " + Vector3.Distance(rigidbody.position, lastPosition).ToString("N5") + " and vel is " + rigidbody.velocity.magnitude);
				}
			}

			lastPosition = rigidbody.position;
		}

		private void OnCollisionStay(Collision collision)
		{
			if (collision.gameObject.GetComponent<Monitor>() != null && !ignoredCollisions.Contains(collision.gameObject))
			{
				if (stuck)
				{
					Physics.IgnoreCollision(collision.collider, collider);
					ignoredCollisions.Add(collision.gameObject);
				}
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.gameObject.GetComponent<Monitor>() != null && !ignoredCollisions.Contains(collision.gameObject))
			{
				if (stuck)
				{
					Physics.IgnoreCollision(collision.collider, collider);
					ignoredCollisions.Add(collision.gameObject);
				}
			}
		}

		private void OnCollisionExit(Collision collision)
		{
			if (ignoredCollisions.Contains(collision.gameObject))
			{
				ignoredCollisions.Remove(collision.gameObject);
				if (ignoredCollisions.Count == 0)
				{
					stuck = false;
				}
			}
		}

		internal void SetBorderSize(float pixels)
		{
			borderSizeInPixels = pixels;
			ResizeScreen();
		}

		private void ResizeScreen()
		{
			// calculate the scale of the screen by finding out the scale of the border of the screen
			screen.transform.localScale = new Vector3((monitorWidth - borderSizeInPixels) / monitorWidth, (monitorHeight - borderSizeInPixels) / monitorHeight, 1);

			screenWidth = Mathf.RoundToInt(monitorWidth * screen.transform.localScale.x);
			screenHeight = Mathf.RoundToInt(monitorHeight * screen.transform.localScale.y);

			xstart = Mathf.RoundToInt(centerX - screenWidth / 2);
			ystart = Mathf.RoundToInt(centerY - screenHeight / 2);

			screenTexture = new Texture2D(screenWidth, screenHeight, TextureFormat.ARGB32, false);
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

			monitorWidth = xFrac * videoWidth;
			monitorHeight = yFrac * videoHeight;

			centerX = videoWidth * xPos; // in unrounded pixels
			centerY = videoHeight * yPos; // in unrounded pixels

			//Debug.Log("Center x and y are " + centerX + ", " + centerY);
			
			transform.localPosition = new Vector3((centerX - videoWidth / 2) / pixelsToUnits, (centerY - videoHeight / 2) / pixelsToUnits, 0);
			rootPosition = rigidbody.position;
			transform.localRotation = Quaternion.identity;

			//Debug.Log("x scale is shaping up 
			transform.localScale = new Vector3(monitorWidth / pixelsToUnits, monitorHeight / pixelsToUnits, 1);
			
			ResizeScreen();

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

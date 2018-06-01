﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		
		Vector3PID positionController;
		Vector3PID velocityController;

		QuaternionPID rotationController;
		Vector3PID angVelocityController;

		// These two are the size of the screen relative to the overall monitor (eg .9 means a monitor that is 90% screen, 10% border)
		/*float screenXscale;
		float screenYscale;*/

		[SerializeField]
		[Range(0.1f, 1f)]
		public float massDensity = 1f;

		[SerializeField]
		[HideInInspector]
		float monitorWidth;
		[SerializeField]
		[HideInInspector]
		float monitorHeight;

		// the localposition it's supposed to be at
		private Vector3 rootPosition;

		private bool stuck = false;
		private Vector3 lastPosition = Vector3.zero;

		private HashSet<GameObject> ignoredCollisions = new HashSet<GameObject>();
		
		private float borderSizeInUnits;

		void Initialize()
		{
			rigidbody = GetComponent<Rigidbody>();
			Assert.IsNotNull(rigidbody);

			collider = GetComponentInChildren<Collider>();
			Assert.IsNotNull(collider);

			Assert.IsNotNull(screen);
			if (Application.isPlaying)
			{
				screenMat = screen.material;
			}
			else
			{
				screenMat = screen.sharedMaterial;
			}
			Assert.IsNotNull(screenMat);
			
			float kp = 1f;
			float ki = 0f;
			float kd = 0.1f;
			positionController = new Vector3PID(kp, ki, kd);
			velocityController = new Vector3PID(kp, ki, kd);
			rotationController = new QuaternionPID(1f, 0.05f, 0f);
			angVelocityController = new Vector3PID(kp, ki, kd);
		}

		private void Start()
		{
			rigidbody = GetComponent<Rigidbody>();
		}

		// For some reason this will get called before initialize and after start, even though initialize is called after start?
		private void FixedUpdate()
		{
			DoStuckCheck();

			Assert.IsNotNull(rigidbody);
			var posCorrection = positionController.GetOutput(rigidbody.position, rootPosition, Time.fixedDeltaTime);
			var velCorrection = velocityController.GetOutput(rigidbody.velocity, Vector3.zero, Time.fixedDeltaTime);

			var force = posCorrection + velCorrection;

			//Debug.Log("Position correction is " + force.ToString("N5"));

			rigidbody.AddForce(force * rigidbody.mass);

			var rotCorrection = rotationController.GetOutput(rigidbody.rotation, Time.fixedDeltaTime);
			var angVelCorrection = angVelocityController.GetOutput(rigidbody.angularVelocity, Vector3.zero, Time.fixedDeltaTime);

			var torque = rotCorrection + angVelCorrection;

			// Debug.Log("Rot correction is " + rotCorrection + " from " + m_rigidBody.rotation.eulerAngles + " and torque add is " + torque);
			rigidbody.AddTorque(torque);// * rigidbody.mass * 0.2f);
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
			borderSizeInUnits = pixels;
			ResizeScreen();
		}

		private void ResizeScreen()
		{
			// calculate the scale of the screen by finding out the scale of the border of the screen
			screen.transform.localScale = new Vector3((monitorWidth - borderSizeInUnits) / monitorWidth, (monitorHeight - borderSizeInUnits) / monitorHeight, 1);
		}
		
		internal void SetParameters(RenderTexture texture, float screenWidthInUnits, float screenHeightInUnits, float borderInUnits, float xPos, float yPos, float xFrac, float yFrac)
		{
			borderSizeInUnits = borderInUnits;

			monitorWidth = xFrac * screenWidthInUnits;
			monitorHeight = yFrac * screenHeightInUnits;

			Initialize();

			screenMat.mainTexture = texture;
			screenMat.SetTexture("_EmissionMap", texture);
			
			if (Application.isPlaying)
			{
				var mesh = screen.gameObject.GetComponent<MeshFilter>().mesh;
				Vector2[] uvs = new Vector2[4];
				// NB rounding errors could push these past [0.0, 1] but who cares.  it'll just wrap a tiny little bit.
				uvs[0] = new Vector2(xPos - xFrac / 2f, yPos - yFrac / 2f);
				uvs[1] = new Vector2(xPos + xFrac / 2f, yPos + yFrac / 2f);
				uvs[2] = new Vector2(xPos + xFrac / 2f, yPos - yFrac / 2f);
				uvs[3] = new Vector2(xPos - xFrac / 2f, yPos + yFrac / 2f);
				mesh.uv = uvs;
			}

			transform.localPosition = new Vector3((xPos - 0.5f) * screenWidthInUnits, (yPos - 0.5f) * screenHeightInUnits, 0);
			rootPosition = rigidbody.position;
			transform.localRotation = Quaternion.identity;

			//Debug.Log("x scale is shaping up 
			transform.localScale = new Vector3(monitorWidth, monitorHeight, 1);

			// basically width x height since z scale is forced to 1
			rigidbody.mass = transform.localScale.sqrMagnitude * massDensity;

			ResizeScreen();
		}
	}
}

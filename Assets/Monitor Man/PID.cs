using System;
using UnityEngine;

namespace MonitorMan
{
	public class PID
	{
		public float Kp = 1.0f;
		public float Ki = 0.0f;
		public float Kd = 0.1f;//0.2f;
		
		private float P, I, D;
		private float prevError;

		public PID(float kp, float ki, float kd)
		{
			Kp = kp;
			Ki = ki;
			Kd = kd;
		}
		
		public float GetOutput(float currentError, float deltaTime)
		{
			P = currentError;
			I += P * deltaTime;
			D = (P - prevError) / deltaTime;
			prevError = currentError;

			return P * Kp + I * Ki + D * Kd;
		}
	}

	// NB not a real PID
	public class QuaternionPID : Vector3PID
	{
		public QuaternionPID(float kp, float ki, float kd) : base(kp, ki, kd)
		{
		}

		// NB doesn't work unless the target is zero
		public Vector3 GetOutput(Quaternion current, float deltaTime)
		{
			//var angles = Quaternion.Inverse(current).eulerAngles;
			var angles = current.eulerAngles;

			float x = angles.x;
			x = (x > 180) ? x - 360 : x;

			float y = angles.y;
			y = (y > 180) ? y - 360 : y;

			float z = angles.z;
			z = (z > 180) ? z - 360 : z;

			//return new Vector3(x, y, z) * deltaTime;
			var ret = GetOutput(new Vector3(x, y, z), Vector3.zero, deltaTime);
			//Debug.Log("Rot is " + new Vector3(x, y, z) + " and output is " + ret);
			return ret;
		}
	}

	public class Vector3PID
	{
		protected PID x;
		protected PID y;
		protected PID z;

		public Vector3PID(float kp, float ki, float kd)
		{
			x = new PID(kp, ki, kd);
			y = new PID(kp, ki, kd);
			z = new PID(kp, ki, kd);
		}

		public Vector3 GetOutput(Vector3 current, Vector3 target, float deltaTime)
		{
			var xf = x.GetOutput(target.x - current.x, deltaTime);
			var yf = y.GetOutput(target.y - current.y, deltaTime);
			var zf = z.GetOutput(target.z - current.z, deltaTime);

			return new Vector3(xf, yf, zf);
		}

		public void SetGain(float kp, float ki, float kz)
		{
			x.Kp = kp;
			x.Ki = ki;
			x.Kd = kz;

			y.Kp = kp;
			y.Ki = ki;
			y.Kd = kz;

			z.Kp = kp;
			z.Ki = ki;
			z.Kd = kz;
		}
	}
}

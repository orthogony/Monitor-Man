using System;
using UnityEngine;

namespace MonitorMan
{
	[Serializable]
	public class PIDGains
	{
		[Range(0,1)]
		public float kP;
		[Range(0, 1)]
		public float kI;
		[Range(0, 1)]
		public float kD;

		public PIDGains(float p, float i, float d)
		{
			kP = p;
			kI = i;
			kD = d;
		}
	}

	public class PID
	{
		public PIDGains gains;
		
		private float P, I, D;
		private float prevError;

		public PID(PIDGains gains)
		{
			this.gains = gains;
		}
		
		public float GetOutput(float currentError, float deltaTime)
		{
			P = currentError;
			I += P * deltaTime;
			D = (P - prevError) / deltaTime;
			prevError = currentError;

			return P * gains.kP + I * gains.kI + D * gains.kD;
		}
	}

	// NB not a real PID
	public class QuaternionPID : Vector3PID
	{
		public QuaternionPID(PIDGains gains) : base(gains)
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

		public Vector3PID(PIDGains gains)
		{
			x = new PID(gains);
			y = new PID(gains);
			z = new PID(gains);
		}

		public Vector3 GetOutput(Vector3 current, Vector3 target, float deltaTime)
		{
			var xf = x.GetOutput(target.x - current.x, deltaTime);
			var yf = y.GetOutput(target.y - current.y, deltaTime);
			var zf = z.GetOutput(target.z - current.z, deltaTime);

			return new Vector3(xf, yf, zf);
		}

		public void SetGain(PIDGains gains)
		{
			x.gains = gains;

			y.gains = gains;

			z.gains = gains;
		}
	}
}

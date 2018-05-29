using UnityEngine;

namespace MonitorMan
{
	public class PID
	{
		public float Kp = 1.0f;
		public float Ki = 0.1f;
		public float Kd = 0.2f;
		
		private float P, I, D;
		private float prevError;
		
		public float GetOutput(float currentError, float deltaTime)
		{
			P = currentError;
			I += P * deltaTime;
			D = (P - prevError) / deltaTime;
			prevError = currentError;

			return P * Kp + I * Ki + D * Kd;
		}
	}

	public class Vector3PID
	{
		private PID x = new PID();
		private PID y = new PID();
		private PID z = new PID();

		public Vector3 GetOutput(Vector3 current, Vector3 target, float deltaTime)
		{
			var xf = x.GetOutput(target.x - current.x, deltaTime);
			var yf = y.GetOutput(target.y - current.y, deltaTime);
			var zf = z.GetOutput(target.z - current.z, deltaTime);
			return new Vector3(xf, yf, zf);
		}
	}

}

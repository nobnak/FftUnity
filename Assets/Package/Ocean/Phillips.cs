using UnityEngine;
using System.Collections;

namespace OceanSystem {

	public class Phillips {
		public int N { get; private set; }
		public K K { get; private set; }

		public float WindSpeed { get; private set; }
		public Vector2 WindDirection { get; private set; }

		private float[,] _p;

		public Phillips(K K, float windSpeed, Vector2 windDirection) {
			this.K = K;
			this.WindSpeed = windSpeed;
			this.WindDirection = windDirection;
			init();
		}

		public float this[int n, int m] {
			get { return _p[n, m]; }
		}

		public static float PhillipsSpectrum(Vector2 k, float windSpeed, Vector2 windDirection) {
			var k2mag = k.sqrMagnitude;
			if (k2mag < 1e-12f)
				return 0f;
			
			var k4mag = k2mag * k2mag;
			var k6mag = k4mag * k2mag;
			var L = windSpeed * windSpeed / OceanUtil.GRAVITY;
			var kDotW = Vector2.Dot(k, windDirection);
			var kDotW2 = kDotW * kDotW * kDotW * kDotW * kDotW * kDotW / k6mag;
			
			float damping   = 0.001f;
			float l2        = L * L * damping * damping;
			
			return Mathf.Exp(-1f / (k2mag * L * L)) * kDotW2 / k4mag * Mathf.Exp(- k2mag * l2);
		}

		private void init() {
			N = K.N;
			_p = new float[N, N];
			for (var y = 0; y < N; y++)
				for (var x = 0; x < N; x++)
					_p[x, y] = calc(x, y);
		}

		private float calc(int n, int m) {
			var k = K[n, m];
			return PhillipsSpectrum(k, WindSpeed, WindDirection);
		}
	}
}
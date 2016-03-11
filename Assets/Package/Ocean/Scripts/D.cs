using UnityEngine;
using System.Collections;

namespace OceanSystem {

	public class D {
		public int N { get; private set; }
		public K K { get; private set; }
		public H H { get; private set; }

		private float _t;
		private float[] _Dx;
		private float[] _Dy;

		public float[] Dx { get { return _Dx; } }
		public float[] Dy { get { return _Dy; } }

		public D(int N, K k, H h) {
			this.N = N;
			this.K = k;
			this.H = h;

			_Dx = new float[2 * N * N];
			_Dy = new float[2 * N * N];

			_Jump(0f);
		}

		public void Jump(float t) {
			if (t == _t)
				return;
			_Jump(t);
		}
		private void _Jump(float t) {
			_t = t;

			var h = H.Current;
			for (var y = 0; y < N; y++) {
				for (var x = 0; x < N; x++) {
					var i = 2 * (x + y * N);
					var kNormalized = K.Normalized(x, y);
					var hx = h[i];
					var hy = h[i + 1];

					var hRotReal = hy;
					var hRotImg = -hx;
					_Dx[i]		= kNormalized.x * hRotReal;
					_Dx[i + 1]	= kNormalized.x * hRotImg;
					_Dy[i]		= kNormalized.y * hRotReal;
					_Dy[i + 1]	= kNormalized.y * hRotImg;
				}
			}
		}
	}
}
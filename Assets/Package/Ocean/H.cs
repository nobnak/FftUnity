using UnityEngine;
using System.Collections;

namespace OceanSystem {

	public class H {
		public int N { get; private set; }
		public H0 H0 { get; private set; }
		public W W { get; private set; }

		private float _t;
		private float[] _h;

		public H(H0 H0, W W) {
			this.H0 = H0;
			this.W = W;

			N = H0.N;
			this._h = new float[2 * N * N];
			_Jump(0f);
		}

		public Vector2 this[int n, int m] {
			get {
				var i = 2 * (n + m * N);
				return new Vector2(_h[i], _h[i + 1]); 
			}
		}

		public float[] Current { get { return _h; } }

		public void Jump(float t) {
			if (t == _t)
				return;
			_Jump(t);
		}

		private void _Jump(float t) {
			_t = t;
			for (var y = 0; y < N; y++) {
				for (var x = 0; x < N; x++) {
					var theta = W[x, y] * t;
					var c = Mathf.Cos(theta);
					var s = Mathf.Sin(theta);
					var hp = H0[x, y];
					var hm = H0.ConjMinusK(x, y);

					var i = 2 * (x + y * N);
					_h[i] = (hp.x + hm.x) * c + (hm.y - hp.y) * s;
					_h[i + 1] = (hp.y + hm.y) * c + (hp.x - hm.x) * s;
				}
			}
		}
	}
}
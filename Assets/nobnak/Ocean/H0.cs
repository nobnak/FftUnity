using UnityEngine;
using System.Collections;

namespace nobnak.Ocean {

	public class H0 {
		public int N { get; private set; }
		public Phillips P { get; private set; }

		private Vector2[,] _h0; 
		private Vector2[,] _h0Conj;

		public H0(Phillips P) {
			this.P = P;
			init();
		}

		public Vector2 this[int n, int m] {
			get { return _h0[n, m]; }
		}
		public Vector2 ConjMinusK(int n, int m) {
			return _h0Conj[n, m];
		}

		private Vector2 calc(int n, int m) {
			var p = P[n, m];
			var r = OceanUtil.StandardNormalDistribution();
			return (OceanUtil.ONE_OVER_SQRT_TWO * Mathf.Sqrt(p)) * r;
		}

		private void init() {
			N = P.N;

			_h0 = new Vector2[N, N];
			for (var y = 0; y < N; y++)
				for (var x = 0; x < N; x++)
					_h0[x, y] = calc(x, y);

			_h0Conj = new Vector2[N, N];
			for (var y = 0; y < N; y++) {
				for (var x = 0; x < N; x++) {
					var h = _h0[(-x + N) % N, (-y + N) % N];
					_h0Conj[x, y] = new Vector2(h.x, -h.y);
				}
			}
		}
	}
}
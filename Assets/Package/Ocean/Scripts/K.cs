using UnityEngine;
using System.Collections;

namespace OceanSystem {

	public class K {
		public int N { get; private set; }
		public float L { get; private set; }

		private int _halfN;
		private float _twopiOverL;

		private Vector2[,] _k;
		private Vector2[,] _kNormalized;

		public Vector2 this[int n, int m]{ get { return _k[n, m]; } }
		public Vector2 Normalized(int n, int m) {
			return _kNormalized[n, m];
		}
		
		public K(int N, float L) {
			this.N = N;
			this.L = L;

			this._halfN = N >> 1;
			this._twopiOverL = 2f * Mathf.PI / L;
			Init();
		}

		public void Init() {
			_k = new Vector2[N,N];
			_kNormalized = new Vector2[N,N];

			for (var y = 0; y < N; y++) {
				for (var x = 0; x < N; x++) {
					var k = _k[x, y] = Calc(x, y);
					_kNormalized[x, y] = k.normalized;
				}
			}
		}

		public Vector2 Calc (int n, int m) {
			n = (n + _halfN) % N - _halfN;
			m = (m + _halfN) % N - _halfN;
			return new Vector2 (n * _twopiOverL, m * _twopiOverL);
		}
	}
}
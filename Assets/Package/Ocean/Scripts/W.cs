using UnityEngine;
using System.Collections;

namespace OceanSystem {

	public class W {
		public const float T = 100f;
		public const float W0 = OceanUtil.TWO_PI / T;

		public int N { get; private set; }
		public K K { get; private set; }

		private float[,] _w;

		public W(K K) {
			this.K = K;
			Init();
		}

		public float this[int n, int m] {
			get { return _w[n, m]; }
		}

		private void Init() {
			N = K.N;
			_w = new float[N, N];
			for (var y = 0; y < N; y++)
				for (var x = 0; x < N; x++)
					_w[x, y] = Calc(x, y);
		}

		private float Calc(int n, int m) {
			var k = K[n, m];
			var w = W_DeepWater(k);
			return (int)(w / W0) * W0;
		}
		
		public float W_DeepWater(Vector2 k) {
			return Mathf.Sqrt(OceanUtil.GRAVITY * k.magnitude);
		}
	}
}
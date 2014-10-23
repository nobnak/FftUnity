using UnityEngine;
using System.Collections;

namespace nobnak.Ocean {

	public class Eta {
		public int N { get; private set; }
		public K K { get; private set; }
		public H H { get; private set; }
		
		private float _t;
		private float[] _Ex;
		private float[] _Ey;
		
		public float[] Ex { get { return _Ex; } }
		public float[] Ey { get { return _Ey; } }
		
		public Eta(int N, K k, H h) {
			this.N = N;
			this.K = k;
			this.H = h;
			
			_Ex = new float[2 * N * N];
			_Ey = new float[2 * N * N];
			
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
					var k = K[x, y];
					var hx = h[i];
					var hy = h[i + 1];
					
					var ihReal = -hy;
					var ihImg  = hx;
					_Ex[i]		= k.x * ihReal;
					_Ex[i + 1]	= k.x * ihImg;
					_Ey[i]		= k.y * ihReal;
					_Ey[i + 1]	= k.y * ihImg;
				}
			}
		}
	}
}
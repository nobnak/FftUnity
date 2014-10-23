using UnityEngine;
using System.Collections;

namespace nobnak.FFT {

	public static class BitReversal {

		public static uint[] Sequence(int n) {
			var res = new uint[n];
			for (var i = 0; i < n; i++)
				res[i] = (uint)i;
			return res;
		}

		public static uint[] Reverse(uint[] xs) {
			return Reverse(xs, Digits(xs.Length));
		}
		public static uint[] Reverse(uint[] xs, int n) {
			var len = xs.Length;
			var res = new uint[len];
			for (var i = 0; i < len; i++)
				res[i] = Reverse(xs[i], n);
			return res;
		}
		public static uint Reverse(uint x, int n) {
			uint res = 0;
			for (int i = 0; i < n; i++)
				res |= (uint)( ((x >> (n - 1 - i)) & 1u) << i );
			return res;
		}

		public static int Digits(int len) {
			var d = 1;
			len -= 1;
			while ((len >>= 1) != 0)
				d++;
			return d;
		}
	}
}
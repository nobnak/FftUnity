using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace nobnak.FFT {

	public class FFT : System.IDisposable {
		public enum Direction { Forward = 0, Backward }

		public const int NTHREADS_IN_GROUP = 8;

		public const int KERNEL_BitReversalX = 0;
		public const int KERNEL_BitReversalY = 1;
		public const int KERNEL_DIT_X_FORWARD = 2;
		public const int KERNEL_DIT_Y_FORWARD = 3;
		public const int KERNEL_DIT_X_BACKWARD = 4;
		public const int KERNEL_DIT_Y_BACKWARD = 5;
		public const int KERNEL_SCALE = 6;

		public const string SHADER_N = "N";
		public const string SHADER_NS = "Ns";
		public const string SHADER_NS2 = "_Ns2";
		public const string SHADER_SCALE = "ScaleFact";
		
		public const string SHADER_BIT_REVERSAL = "BitReversal";
		public const string SHADER_FFT_IN = "FftIn";
		public const string SHADER_FFT_OUT = "FftOut";

		public ComputeShader fft;

		private int _n = -1;
		private int _nGroups;

		private ComputeBuffer _bitReversalBuf;

		private RenderTexture _fftTexIn;
		private RenderTexture _fftTexOut;

		~FFT() { Release(); }
		#region IDisposable implementation
		public void Dispose () { Release(); }
		#endregion

		public Texture Forward(Texture spaceTex) {
			Init(spaceTex);
			Graphics.Blit(spaceTex, _fftTexIn);
			FftX(Direction.Forward);
			FftY(Direction.Forward);
			Normalize();
			return _fftTexIn;
		}
		public Texture Backward(Texture freqTex) {
			Init(freqTex);
			Graphics.Blit(freqTex, _fftTexIn);
			FftX(Direction.Backward);
			FftY(Direction.Backward);
			Normalize();
			return _fftTexIn;
		}

		void Release() {
			if (_fftTexIn != null)
				_fftTexIn.Release();
			if (_fftTexOut != null)
				_fftTexOut.Release();
			if (_bitReversalBuf != null)
				_bitReversalBuf.Release();
		}
		void Init(Texture texIn) {
			if (texIn.width != texIn.height) {
				Debug.Log("Tex width & height must match");
				return;
			}
			if (_n == texIn.width)
				return;

			Release();
			_n = texIn.width;
			_nGroups = _n / NTHREADS_IN_GROUP;
			fft.SetInt(SHADER_N, _n);
			fft.SetFloat(SHADER_SCALE, 1f / _n);
			
			_fftTexIn = new RenderTexture(_n, _n, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
			_fftTexOut = new RenderTexture(_n, _n, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
			_fftTexIn.filterMode = _fftTexOut.filterMode = texIn.filterMode;
			_fftTexIn.enableRandomWrite = _fftTexOut.enableRandomWrite = true;
			_fftTexIn.Create();
			_fftTexOut.Create();

			var bitReversals = BitReversal.Reverse(BitReversal.Sequence(_n));
			_bitReversalBuf = new ComputeBuffer(bitReversals.Length, Marshal.SizeOf(bitReversals[0]));
			_bitReversalBuf.SetData(bitReversals);
		}
		
		void Swap() { var tmp = _fftTexIn; _fftTexIn = _fftTexOut; _fftTexOut = tmp; }
		void FftX(Direction dir) {
			var dit = (dir == Direction.Forward ? KERNEL_DIT_X_FORWARD : KERNEL_DIT_X_BACKWARD);

			fft.SetTexture (KERNEL_BitReversalX, SHADER_FFT_IN, _fftTexIn);
			fft.SetTexture (KERNEL_BitReversalX, SHADER_FFT_OUT, _fftTexOut);
			fft.SetBuffer (KERNEL_BitReversalX, SHADER_BIT_REVERSAL, _bitReversalBuf);
			fft.Dispatch (KERNEL_BitReversalX, _nGroups, _nGroups, 1);
			Swap ();

			var ns = 1;
			while ((ns <<= 1) <= _n) {
				fft.SetInt (SHADER_NS, ns);
				fft.SetInt (SHADER_NS2, ns / 2);
				fft.SetTexture (dit, SHADER_FFT_IN, _fftTexIn);
				fft.SetTexture (dit, SHADER_FFT_OUT, _fftTexOut);
				fft.Dispatch (dit, _nGroups, _nGroups, 1);
				Swap ();
			}
		}
		void FftY(Direction dir) {
			var dit = (dir == Direction.Forward ? KERNEL_DIT_Y_FORWARD : KERNEL_DIT_Y_BACKWARD);

			fft.SetTexture (KERNEL_BitReversalY, SHADER_FFT_IN, _fftTexIn);
			fft.SetTexture (KERNEL_BitReversalY, SHADER_FFT_OUT, _fftTexOut);
			fft.SetBuffer (KERNEL_BitReversalY, SHADER_BIT_REVERSAL, _bitReversalBuf);
			fft.Dispatch (KERNEL_BitReversalY, _nGroups, _nGroups, 1);
			Swap ();
			var ns = 1;
			while ((ns <<= 1) <= _n) {
				fft.SetInt (SHADER_NS, ns);
				fft.SetInt (SHADER_NS2, ns / 2);
				fft.SetTexture (dit, SHADER_FFT_IN, _fftTexIn);
				fft.SetTexture (dit, SHADER_FFT_OUT, _fftTexOut);
				fft.Dispatch (dit, _nGroups, _nGroups, 1);
				Swap ();
			}
		}
		void Normalize() {
			fft.SetTexture (KERNEL_SCALE, SHADER_FFT_IN, _fftTexIn);
			fft.SetTexture (KERNEL_SCALE, SHADER_FFT_OUT, _fftTexOut);
			fft.Dispatch (KERNEL_SCALE, _nGroups, _nGroups, 1);
			Swap ();
		}
	}
}
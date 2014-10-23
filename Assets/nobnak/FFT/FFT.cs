using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace nobnak.FFT {

	public class FFT : MonoBehaviour {
		public const int NTHREADS_IN_GROUP = 8;

		public const int KERNEL_BitReversalX = 0;
		public const int KERNEL_BitReversalY = 1;
		public const int KERNEL_DitX = 2;
		public const int KERNEL_DitY = 3;
		public const int KERNEL_SCALE = 4;

		public const string SHADER_N = "N";
		public const string SHADER_NS = "Ns";
		public const string SHADER_NS2 = "_Ns2";
		public const string SHADER_SCALE = "ScaleFact";
		
		public const string SHADER_BIT_REVERSAL = "BitReversal";
		public const string SHADER_FFT_IN = "FftIn";
		public const string SHADER_FFT_OUT = "FftOut";

		public ComputeShader fft;
		public Texture2D photo;

		private int _n;
		private int _nGroups;

		private ComputeBuffer _bitReversalBuf;
		private uint[] _bitSequences;
		private uint[] _bitReversals;

		private RenderTexture _fftTexIn;
		private RenderTexture _fftTexOut;

		void OnDisable() {
			_fftTexIn.Release();
			_fftTexOut.Release();
			_bitReversalBuf.Release();
		}
		void OnEnable() {
			_n = photo.width;
			_nGroups = _n / NTHREADS_IN_GROUP;
			fft.SetInt(SHADER_N, _n);
			fft.SetFloat(SHADER_SCALE, 1f / _n);

			_fftTexIn = new RenderTexture(_n, _n, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
			_fftTexOut = new RenderTexture(_n, _n, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
			_fftTexIn.filterMode = _fftTexOut.filterMode = photo.filterMode;
			_fftTexIn.enableRandomWrite = _fftTexOut.enableRandomWrite = true;
			_fftTexIn.Create();
			_fftTexOut.Create();

			_bitSequences = BitReversal.Sequence(_n);
			_bitReversals = BitReversal.Reverse(_bitSequences);
			_bitReversalBuf = new ComputeBuffer(_bitReversals.Length, Marshal.SizeOf(_bitReversals[0]));
			_bitReversalBuf.SetData(_bitReversals);
			//_bitReversalBuf.SetData(_bitSequences);
		}

		void Start() {
			Graphics.Blit(photo, _fftTexIn);

			FftX();
			FftY();
			Normalize();
		}
		void OnGUI() {
			var scrRect = new Rect(0.5f * (Screen.width - Screen.height), 0f, Screen.height, Screen.height);
			Graphics.DrawTexture(scrRect, _fftTexIn);
		}

		void Swap() { var tmp = _fftTexIn; _fftTexIn = _fftTexOut; _fftTexOut = tmp; }

		void FftX() {
			fft.SetTexture (KERNEL_BitReversalX, SHADER_FFT_IN, _fftTexIn);
			fft.SetTexture (KERNEL_BitReversalX, SHADER_FFT_OUT, _fftTexOut);
			fft.SetBuffer (KERNEL_BitReversalX, SHADER_BIT_REVERSAL, _bitReversalBuf);
			fft.Dispatch (KERNEL_BitReversalX, _nGroups, _nGroups, 1);
			Swap ();

			var ns = 1;
			while ((ns <<= 1) <= _n) {
				fft.SetInt (SHADER_NS, ns);
				fft.SetInt (SHADER_NS2, ns / 2);
				fft.SetTexture (KERNEL_DitX, SHADER_FFT_IN, _fftTexIn);
				fft.SetTexture (KERNEL_DitX, SHADER_FFT_OUT, _fftTexOut);
				fft.Dispatch (KERNEL_DitX, _nGroups, _nGroups, 1);
				Swap ();
			}
		}
		void FftY() {
			fft.SetTexture (KERNEL_BitReversalY, SHADER_FFT_IN, _fftTexIn);
			fft.SetTexture (KERNEL_BitReversalY, SHADER_FFT_OUT, _fftTexOut);
			fft.SetBuffer (KERNEL_BitReversalY, SHADER_BIT_REVERSAL, _bitReversalBuf);
			fft.Dispatch (KERNEL_BitReversalY, _nGroups, _nGroups, 1);
			Swap ();
			var ns = 1;
			while ((ns <<= 1) <= _n) {
				fft.SetInt (SHADER_NS, ns);
				fft.SetInt (SHADER_NS2, ns / 2);
				fft.SetTexture (KERNEL_DitY, SHADER_FFT_IN, _fftTexIn);
				fft.SetTexture (KERNEL_DitY, SHADER_FFT_OUT, _fftTexOut);
				fft.Dispatch (KERNEL_DitY, _nGroups, _nGroups, 1);
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
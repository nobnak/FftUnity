using UnityEngine;
using System.Collections;
using nobnak.FFT;
using nobnak.Ocean;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class TestOcean : MonoBehaviour {
	public ComputeShader fft;
	public ComputeShader ocean;

	public float oceanSize = 100f;
	public int N = 64;
	public Vector2 windVelocity = new Vector2(5f, 0f);

	private int _nGroups;
	private FFT _fft;
	private H0 _h0;
	private W _w;
	private float _time = 0f;

	private ComputeBuffer _h0Buf;
	private ComputeBuffer _wBuf;
	private RenderTexture _h0Tex;
	private RenderTexture _wTex;
	private RenderTexture _hTex;

	void OnDestroy() {
		_fft.Dispose();

		_h0Buf.Release();
		_wBuf.Release();

		_h0Tex.Release();
		_wTex.Release();
		_hTex.Release();
	}
	void Start () {
		_fft = new FFT(fft);

		var k = new K(N, oceanSize);
		var phillips = new Phillips(k, windVelocity.magnitude, windVelocity.normalized);
		_h0 = new H0(phillips);
		_w = new W(k);

		_nGroups = N / OceanConst.NTHREADS_IN_GROUP;

		var h0BufData = new Vector4[N * N];
		var wBufData = new Vector4[N * N];
		for (var y = 0; y < N; y++) {
			for (var x = 0; x < N; x++) {
				var i = x + y * N;
				var hp = _h0[x, y];
				var hm = _h0.ConjMinusK(x, y);
				h0BufData[i] = new Vector4(hp.x, hp.y, hm.x, hm.y);
				wBufData[i] = new Vector4(_w[x, y], 0f, 0f, 0f);
			}
		}
		_h0Buf = new ComputeBuffer(h0BufData.Length, Marshal.SizeOf(h0BufData[0]));
		_wBuf = new ComputeBuffer(wBufData.Length, Marshal.SizeOf(wBufData[0]));
		_h0Buf.SetData(h0BufData);
		_wBuf.SetData(wBufData);

		_h0Tex = new RenderTexture(N, N, 0, RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
		_wTex = new RenderTexture(N, N, 0, RenderTextureFormat.ARGBFloat,RenderTextureReadWrite.Linear);
		_hTex = new RenderTexture(N, N, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
		_h0Tex.filterMode = _wTex.filterMode = _hTex.filterMode = FilterMode.Point;
		_h0Tex.enableRandomWrite = _wTex.enableRandomWrite = _hTex.enableRandomWrite = true;
		_h0Tex.Create();
		_wTex.Create();
		_hTex.Create();

		ocean.SetInt(OceanConst.SHADER_N, N);

		ocean.SetBuffer(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_BUF_IN, _h0Buf);
		ocean.SetTexture(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_TEX_OUT, _h0Tex);
		_h0Tex.DiscardContents();
		ocean.Dispatch(OceanConst.KERNEL_BUF2TEX, _nGroups, _nGroups, 1);
		
		ocean.SetBuffer(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_BUF_IN, _wBuf);
		ocean.SetTexture(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_TEX_OUT, _wTex);
		_wTex.DiscardContents();
		ocean.Dispatch(OceanConst.KERNEL_BUF2TEX, _nGroups, _nGroups, 1);
	}

	void Update() {
		ocean.SetFloat(OceanConst.SHADER_TIME, _time += Time.deltaTime);
		ocean.SetTexture(OceanConst.KERNEL_UPDATE_H, OceanConst.SHADER_H0_TEX, _h0Tex);
		ocean.SetTexture(OceanConst.KERNEL_UPDATE_H, OceanConst.SHADER_W_TEX, _wTex);
		ocean.SetTexture(OceanConst.KERNEL_UPDATE_H, OceanConst.SHADER_H_TEX, _hTex);
		_hTex.DiscardContents();
		ocean.Dispatch(OceanConst.KERNEL_UPDATE_H, _nGroups, _nGroups, 1);

		renderer.sharedMaterial.mainTexture = _fft.Backward(_hTex);
	}
}

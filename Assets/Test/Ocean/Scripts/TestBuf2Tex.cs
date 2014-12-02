using UnityEngine;
using System.Collections;
using nobnak.Ocean;
using System.Runtime.InteropServices;

public class TestBuf2Tex : MonoBehaviour {
	public Texture2D photo;
	public ComputeShader ocean;

	private ComputeBuffer _photoBuf;
	private RenderTexture _cloneTex;

	void OnDestroy() { Release(); }
	void Update() {
		CheckInit();

		var n = photo.width;
		var nGroups = n / OceanConst.NTHREADS_IN_GROUP;
		ocean.SetInt(OceanConst.SHADER_N, n);
		ocean.SetBuffer(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_BUF_IN, _photoBuf);
		ocean.SetTexture(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_TEX_OUT, _cloneTex);
		_cloneTex.DiscardContents();
		ocean.Dispatch(OceanConst.KERNEL_BUF2TEX, nGroups, nGroups, 1);
	}

	void Release() {
		if (_photoBuf != null)
			_photoBuf.Release();
		if (_cloneTex != null)
			_cloneTex.Release();
	}
	void CheckInit() {
		var n = photo.width;
		if (_cloneTex != null && _cloneTex.width == n)
			return;

		Release();
		var photoColors = photo.GetPixels();
		var photoPixs = new Vector4[photoColors.Length];
		for (var i = 0; i < photoColors.Length; i++)
			photoPixs[i] = photoColors[i];
		_photoBuf = new ComputeBuffer(photoPixs.Length, Marshal.SizeOf(photoPixs[0]));
		_photoBuf.SetData(photoPixs);
		
		_cloneTex = new RenderTexture(n, n, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		_cloneTex.filterMode = photo.filterMode;
		_cloneTex.enableRandomWrite = true;
		_cloneTex.Create();
		
		renderer.sharedMaterial.mainTexture = _cloneTex;
	}
}

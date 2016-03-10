using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using FFTSystem;

namespace OceanSystem {

    public class Ocean : MonoBehaviour {
    	public const string SHADER_HEIGHT_MAP = "_HeightMap";
    	public const string SHADER_NORMAL_MAP = "_BumpMap";
    	public const string SHADER_WORLD_VIEW = "_WorldViewPos";
    	public const string SHADER_H0_MAP = "_H0Tex";
    	public const string SHADER_W_MAP = "_WTex";
    	public const string SHADER_HEIGHT = "_Height";

    	public ComputeShader fft;
    	public ComputeShader ocean;

    	public float oceanSize = 100f;
    	public int N = 64;
    	public Vector2 windVelocity = new Vector2(5f, 0f);
    	public float height = 1f;
        public float timeScale = 1f;
        public int randomSeed = 100;
    	public Transform view;

    	int _nGroups;
    	FFT _fft;
    	H0 _h0;
    	W _w;
    	float _time = 0f;

    	ComputeBuffer _h0Buf;
    	ComputeBuffer _wBuf;
    	RenderTexture _h0Tex;
    	RenderTexture _wTex;
    	RenderTexture _hTex;
    	RenderTexture _nTex;

    	Rect _guiWindow;
    	Renderer _renderer;
    	MaterialPropertyBlock _block;

    	void OnDestroy() {
    		_fft.Dispose();

    		_h0Buf.Release();
    		_wBuf.Release();

    		_h0Tex.Release();
    		_wTex.Release();
    		_hTex.Release();
    		_nTex.Release();
    	}
    	void Start () {
            Random.seed = randomSeed;
    		Camera.main.depthTextureMode |= DepthTextureMode.Depth;
    		var winSize = new Vector2(300f, 100f);
    		_guiWindow = new Rect(Screen.width - winSize.x, Screen.height - winSize.y, winSize.x, winSize.y);

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

    		_h0Tex = new RenderTexture(N, N, 0, RenderTextureFormat.ARGBFloat);
    		_wTex = new RenderTexture(N, N, 0, RenderTextureFormat.ARGBFloat);
    		_hTex = new RenderTexture(N, N, 0, RenderTextureFormat.RGFloat);
    		_nTex = new RenderTexture(N, N, 0, RenderTextureFormat.ARGBFloat);
    		_h0Tex.filterMode = _wTex.filterMode = _hTex.filterMode = _nTex.filterMode = FilterMode.Bilinear;
    		_h0Tex.wrapMode = _wTex.wrapMode = _hTex.wrapMode = _nTex.wrapMode = TextureWrapMode.Repeat;
    		_h0Tex.enableRandomWrite = _wTex.enableRandomWrite = _hTex.enableRandomWrite = _nTex.enableRandomWrite = true;
    		_h0Tex.Create();
    		_wTex.Create();
    		_hTex.Create();
    		_nTex.Create();

    		ocean.SetInt(OceanConst.SHADER_N, N);

    		ocean.SetBuffer(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_BUF_IN, _h0Buf);
    		ocean.SetTexture(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_TEX_OUT, _h0Tex);
    		_h0Tex.DiscardContents();
    		ocean.Dispatch(OceanConst.KERNEL_BUF2TEX, _nGroups, _nGroups, 1);
    		
    		ocean.SetBuffer(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_BUF_IN, _wBuf);
    		ocean.SetTexture(OceanConst.KERNEL_BUF2TEX, OceanConst.SHADER_COPY_TEX_OUT, _wTex);
    		_wTex.DiscardContents();
    		ocean.Dispatch(OceanConst.KERNEL_BUF2TEX, _nGroups, _nGroups, 1);

    		_renderer = GetComponent<Renderer> ();
    		_block = new MaterialPropertyBlock ();
    		_renderer.GetPropertyBlock (_block);
    	}

    	void OnGUI() {
    		_guiWindow = GUILayout.Window(1, _guiWindow, Window, "View Position");
    	}
    	void Window(int id) {
    		GUILayout.BeginVertical();
    		var pos = view.position;
    		GUILayout.Label(string.Format("Horizontal : {0:f2}", pos.x));
    		pos.x = GUILayout.HorizontalSlider(pos.x, -15f, 15f);
    		GUILayout.Label(string.Format("Vertical : {0:f2}", pos.z));
    		pos.z = GUILayout.HorizontalSlider(pos.z, -15f, 15f);
    		view.position = pos;
    		GUILayout.EndVertical();
    		GUI.DragWindow();
    	}

    	void Update() {
            ocean.SetFloat(OceanConst.SHADER_TIME, _time += Time.deltaTime * timeScale);
    		ocean.SetTexture(OceanConst.KERNEL_UPDATE_H, OceanConst.SHADER_H0_TEX, _h0Tex);
    		ocean.SetTexture(OceanConst.KERNEL_UPDATE_H, OceanConst.SHADER_W_TEX, _wTex);
    		ocean.SetTexture(OceanConst.KERNEL_UPDATE_H, OceanConst.SHADER_H_TEX, _hTex);
    		_hTex.DiscardContents();
    		ocean.Dispatch(OceanConst.KERNEL_UPDATE_H, _nGroups, _nGroups, 1);

    		var heightTex = _fft.Backward(_hTex);
    		var heightTexDx = 1f / heightTex.width;
    		ocean.SetFloat(OceanConst.SHADER_DX, oceanSize * heightTexDx);
    		ocean.SetFloat(OceanConst.SHADER_HEIGHT_TEX_DX, heightTexDx);
    		ocean.SetFloat(OceanConst.SHADER_HEIGHT, height);
    		ocean.SetTexture(OceanConst.KERNEL_UPDATE_N, OceanConst.SHADER_HEIGHT_TEX, heightTex);
    		ocean.SetTexture(OceanConst.KERNEL_UPDATE_N, OceanConst.SHADER_N_TEX, _nTex);
    		_nTex.DiscardContents();
    		ocean.Dispatch(OceanConst.KERNEL_UPDATE_N, _nGroups, _nGroups, 1);

    		_block.SetTexture(SHADER_HEIGHT_MAP, heightTex);
    		_block.SetTexture(SHADER_NORMAL_MAP, _nTex);
    		var viewPos = (Vector4)view.position;
    		viewPos.w = 1f;
    		_block.SetVector(SHADER_WORLD_VIEW, viewPos);
    		_block.SetTexture(SHADER_H0_MAP, _h0Tex);
    		_block.SetTexture(SHADER_W_MAP, _wTex);
    		_block.SetFloat (SHADER_HEIGHT, height);
    		_renderer.SetPropertyBlock (_block);
    	}
    }
}

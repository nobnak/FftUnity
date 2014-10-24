using UnityEngine;
using System.Collections;
using nobnak.FFT;

public class TestFFT : MonoBehaviour {
	public Texture2D photo;
	public ComputeShader fft;

	private FFT _fft;

	void OnDisable() {
		_fft.Dispose();
	}
	void OnEnable() {
		_fft = new FFT(fft);
	}
	void Update() {
		renderer.sharedMaterial.mainTexture = _fft.Forward(photo);
	}
}

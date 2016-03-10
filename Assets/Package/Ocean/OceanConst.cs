using UnityEngine;
using System.Collections;

namespace OceanSystem {

	public static class OceanConst {
		public const int KERNEL_BUF2TEX = 0;
		public const int KERNEL_UPDATE_H = 1;
		public const int KERNEL_UPDATE_N = 2;

		public const int NTHREADS_IN_GROUP = 8;
		
		public const string SHADER_N = "N";
		public const string SHADER_COPY_BUF_IN = "CopyBufIn";
		public const string SHADER_COPY_TEX_OUT = "CopyTexOut";

		public const string SHADER_TIME = "Time";
		public const string SHADER_H0_TEX = "H0Tex";
		public const string SHADER_W_TEX = "WTex";
		public const string SHADER_H_TEX = "HTex";

		public const string SHADER_DX = "Dx";
		public const string SHADER_HEIGHT = "Height";
		public const string SHADER_HEIGHT_TEX_DX = "HeightTexDx";
		public const string SHADER_HEIGHT_TEX = "HeightTex";
		public const string SHADER_N_TEX = "NTex";
	}
}
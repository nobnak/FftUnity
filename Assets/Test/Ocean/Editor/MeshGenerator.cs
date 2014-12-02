﻿using UnityEngine;
using System.Collections;
using UnityEditor;

public static class MeshGenerator {
	[MenuItem("Custom/Generate Quads")]
	public static void GenQuad() {
		var repeat = 3;
		var nVertices = 4 * repeat * repeat;
		var nIndices = 6 * repeat * repeat;
		var offset = new Vector3(-0.5f * repeat, -0.5f * repeat, 0f);

		var vertices = new Vector3[nVertices];
		var uv = new Vector2[nVertices];
		var normals = new Vector3[nVertices];
		var tangents = new Vector4[nVertices];
		var indices = new int[nIndices];

		var iVertex = 0;
		var iIndex = 0;
		for (var y = 0; y < repeat; y++) {
			for (var x = 0; x < repeat; x++) {
				vertices[iVertex    ] = new Vector3(    x,     y, 0) + offset;
				vertices[iVertex + 1] = new Vector3(x + 1,     y, 0) + offset;
				vertices[iVertex + 2] = new Vector3(    x, y + 1, 0) + offset;
				vertices[iVertex + 3] = new Vector3(x + 1, y + 1, 0) + offset;
				uv[iVertex    ] = new Vector2(0f, 0f);
				uv[iVertex + 1] = new Vector2(1f, 0f);
				uv[iVertex + 2] = new Vector2(0f, 1f);
				uv[iVertex + 3] = new Vector2(1f, 1f);
				normals[iVertex] = normals[iVertex + 1] = normals[iVertex + 2] = normals[iVertex + 3]
					= new Vector3(0f, 0f, -1f);
				tangents[iVertex] = tangents[iVertex + 1] = tangents[iVertex + 2] = tangents[iVertex + 3]
					= new Vector4(1f, 0f, 0f, -1f);
				var baseVertex = 4 * (x + y * repeat);
				indices[iIndex    ] = baseVertex;
				indices[iIndex + 1] = baseVertex + 3;
				indices[iIndex + 2] = baseVertex + 1;
				indices[iIndex + 3] = baseVertex;
				indices[iIndex + 4] = baseVertex + 2;
				indices[iIndex + 5] = baseVertex + 3;
				iVertex += 4;
				iIndex += 6;
			}
		}

		var mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.normals = normals;
		mesh.tangents = tangents;
		mesh.triangles = indices;
		mesh.RecalculateBounds();

		AssetDatabase.CreateAsset(mesh, string.Format("Assets/Quad{0}.asset", repeat));
	}
}

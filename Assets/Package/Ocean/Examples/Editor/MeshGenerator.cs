using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace OceanSystem {

    public class MeshGenerator : EditorWindow {
        int _lod = 10;

        [MenuItem("Window/Custom/MeshGenerator")]
        public static void ShowWindow() {
            EditorWindow.GetWindow (typeof(MeshGenerator));
        }

        void OnGUI() {
            _lod = EditorGUILayout.IntField ("Mesh LOD", _lod);
            if (GUILayout.Button ("Generate"))
                Generate ();
        }
        void Generate() {
            var uvs = new List<Vector2> ();
            var triangles = new List<int> ();

            var width = 1 << _lod;
            var dx = 1f / width;

            for (var y = 0; y <= width; y++)
                for (var x = 0; x <= width; x++)
                    uvs.Add (new Vector2 (x * dx, y * dx));

            var nextY = width + 1;
            for (var y = 0; y < width; y++) {
                for (var x = 0; x < width; x++) {
                    var v = x + y * (width+1) + 1;
                    triangles.Add (v);
                    triangles.Add (v + 1);
                    triangles.Add (v + nextY + 1);
                    triangles.Add (v);
                    triangles.Add (v + nextY + 1);
                    triangles.Add (v + nextY);
                }
            }

            var name = string.Format ("Grid{0:D5}", width);
            var path = string.Format ("Assets/{0}.obj", name);
            using (var writer = new StreamWriter (path, false, System.Text.Encoding.UTF8)) {
                writer.WriteLine ("o {0}", name);
                foreach (var uv in uvs) {
                    var v = new Vector3 (0.5f-uv.x, 0f, uv.y-0.5f);
                    writer.WriteLine ("v {0:e3} {1:e3} {2:e3}", v.x, v.y, v.z);
                }
                foreach (var uv in uvs)
                    writer.WriteLine ("vt {0:e3} {1:e3}", uv.x, uv.y);
                var triangleCount = triangles.Count;
                for (var i = 0; i < triangleCount; i+=3)
                    writer.WriteLine ("f {0}/{0} {1}/{1} {2}/{2}", triangles[i], triangles[i+1], triangles[i+2]);
            }
        }
    }
}
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace OceanSystem {

    public class MeshGenerator : EditorWindow {
        public const int BUFFER_SIZE = 1 << 19;

        float _width = 1f;
        float _height = 1f;
        int _lodw = 1;
        int _lodh = 1;

        [MenuItem("Window/Custom/MeshGenerator")]
        public static void ShowWindow() {
            EditorWindow.GetWindow (typeof(MeshGenerator));
        }

        void OnGUI() {
            GUILayout.Label ("Size (W x H)");
            EditorGUILayout.BeginHorizontal ();
            _width = EditorGUILayout.FloatField (_width);
            _height = EditorGUILayout.FloatField (_height);
            EditorGUILayout.EndHorizontal ();

            GUILayout.Label ("LOD");
            EditorGUILayout.BeginHorizontal ();
            _lodw = EditorGUILayout.IntField (_lodw);
            _lodh = EditorGUILayout.IntField (_lodh);
            EditorGUILayout.EndHorizontal ();

            if (GUILayout.Button ("Generate"))
                Generate ();
        }
        void Generate() {
            var xlimit = 1 << _lodw;
            var ylimit = 1 << _lodh;
            //var duv = new Vector2 (1f / xlimit, 1f / ylimit);
            var dx = new Vector2(_width / xlimit, _height / ylimit);
            var size = new Vector2 (_width, _height);

            var name = string.Format ("Grid{0:D4}_{1:D4}", xlimit, ylimit);
            var path = string.Format ("Assets/{0}.obj", name);
            using (var writer = new StreamWriter (path, false, System.Text.Encoding.UTF8, BUFFER_SIZE)) {
                writer.WriteLine ("g {0}", name);
                foreach (var v in VertexIterator(xlimit, ylimit, size, dx))
                    writer.WriteLine ("v {0:e3} {1:e3} {2:e3}", v.x, v.y, v.z);
                foreach (var uv in UVIterator(xlimit, ylimit, dx))
                    writer.WriteLine ("vt {0:e3} {1:e3}", uv.x, uv.y);
                foreach (var t in TriangleIterator(xlimit, ylimit))
                    writer.WriteLine ("f {0}/{0} {1}/{1} {2}/{2}", t.x, t.y, t.z);
            }
        }

        public static IEnumerable<Vector3> VertexIterator(int xlimit, int ylimit, Vector2 size, Vector2 dx) {
            var offset = new Vector2 (-0.5f * size.x, -0.5f * size.y);
            for (var y = 0; y <= ylimit; y++)
                for (var x = 0; x <= xlimit; x++)
                    yield return new Vector3 (-(x * dx.x + offset.x), 0f, y * dx.y + offset.y);
        }
        public static IEnumerable<Vector2> UVIterator(int xlimit, int ylimit, Vector2 dx) {
            for (var y = 0; y <= ylimit; y++)
                for (var x = 0; x <= xlimit; x++)
                    yield return new Vector2 (x * dx.x, y * dx.y);
        }
        public static IEnumerable<Int3> TriangleIterator (int xlimit, int ylimit) {
            var nextY = xlimit + 1;
            for (var y = 0; y < ylimit; y++) {
                for (var x = 0; x < xlimit; x++) {
                    var v = x + y * (xlimit + 1) + 1;
                    yield return new Int3 (v, v + 1, v + nextY + 1);
                    yield return new Int3(v, v + nextY + 1, v + nextY);
                }
            }
        }

        public struct Int3 {
            public int x, y, z;
            public Int3(int x, int y, int z) {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
    }
}
using UnityEngine;

namespace PulseHighway.Visual
{
    public static class MeshFactory
    {
        public static Mesh CreateOctahedron(float radius = 0.5f)
        {
            var mesh = new Mesh { name = "Octahedron" };
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, radius, 0),
                new Vector3(radius, 0, 0),
                new Vector3(0, 0, radius),
                new Vector3(-radius, 0, 0),
                new Vector3(0, 0, -radius),
                new Vector3(0, -radius, 0)
            };
            mesh.triangles = new int[]
            {
                0,2,1, 0,3,2, 0,4,3, 0,1,4,
                5,1,2, 5,2,3, 5,3,4, 5,4,1
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Gem-shaped note block with beveled top — wider, more visible, catches light better.
        /// </summary>
        public static Mesh CreateGemBlock(float width = 1.8f, float height = 0.8f, float depth = 0.8f)
        {
            var mesh = new Mesh { name = "GemBlock" };

            float hw = width * 0.5f;
            float hh = height * 0.5f;
            float hd = depth * 0.5f;
            float bev = 0.7f;

            Vector3[] verts = new Vector3[]
            {
                new Vector3(-hw, -hh, -hd),
                new Vector3( hw, -hh, -hd),
                new Vector3( hw, -hh,  hd),
                new Vector3(-hw, -hh,  hd),
                new Vector3(-hw*bev, hh, -hd*bev),
                new Vector3( hw*bev, hh, -hd*bev),
                new Vector3( hw*bev, hh,  hd*bev),
                new Vector3(-hw*bev, hh,  hd*bev),
            };

            int[] tris = new int[]
            {
                0,1,2, 0,2,3,
                4,6,5, 4,7,6,
                3,2,6, 3,6,7,
                0,5,1, 0,4,5,
                0,3,7, 0,7,4,
                1,5,6, 1,6,2,
            };

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateBox(Vector3 size)
        {
            var mesh = new Mesh { name = "Box" };
            float x = size.x * 0.5f, y = size.y * 0.5f, z = size.z * 0.5f;

            Vector3[][] faces = {
                new[]{ new Vector3(-x,-y,z), new Vector3(x,-y,z), new Vector3(x,y,z), new Vector3(-x,y,z) },
                new[]{ new Vector3(x,-y,-z), new Vector3(-x,-y,-z), new Vector3(-x,y,-z), new Vector3(x,y,-z) },
                new[]{ new Vector3(-x,y,z), new Vector3(x,y,z), new Vector3(x,y,-z), new Vector3(-x,y,-z) },
                new[]{ new Vector3(-x,-y,-z), new Vector3(x,-y,-z), new Vector3(x,-y,z), new Vector3(-x,-y,z) },
                new[]{ new Vector3(x,-y,z), new Vector3(x,-y,-z), new Vector3(x,y,-z), new Vector3(x,y,z) },
                new[]{ new Vector3(-x,-y,-z), new Vector3(-x,-y,z), new Vector3(-x,y,z), new Vector3(-x,y,-z) },
            };

            Vector3[] verts = new Vector3[24];
            int[] tris = new int[36];
            for (int f = 0; f < 6; f++)
            {
                int v = f * 4, t = f * 6;
                for (int i = 0; i < 4; i++) verts[v + i] = faces[f][i];
                tris[t] = v; tris[t+1] = v+2; tris[t+2] = v+1;
                tris[t+3] = v; tris[t+4] = v+3; tris[t+5] = v+2;
            }

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateCylinder(float radius, float height, int segments = 24)
        {
            var mesh = new Mesh { name = "Cylinder" };
            var verts = new System.Collections.Generic.List<Vector3>();
            var tris = new System.Collections.Generic.List<int>();
            float halfH = height * 0.5f;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float cx = Mathf.Cos(angle) * radius;
                float cz = Mathf.Sin(angle) * radius;
                verts.Add(new Vector3(cx, -halfH, cz));
                verts.Add(new Vector3(cx, halfH, cz));
            }

            for (int i = 0; i < segments; i++)
            {
                int a = i*2, b = i*2+1, c = (i+1)*2, d = (i+1)*2+1;
                tris.Add(a); tris.Add(b); tris.Add(c);
                tris.Add(c); tris.Add(b); tris.Add(d);
            }

            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreatePlane(float width, float height)
        {
            var mesh = new Mesh { name = "Plane" };
            float w = width * 0.5f, h = height * 0.5f;
            mesh.vertices = new Vector3[] {
                new Vector3(-w,0,-h), new Vector3(w,0,-h), new Vector3(w,0,h), new Vector3(-w,0,h)
            };
            mesh.triangles = new int[] { 0,2,1, 0,3,2 };
            mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            mesh.uv = new Vector2[] { new(0,0), new(1,0), new(1,1), new(0,1) };
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateGridMesh(float size, int divisions)
        {
            var mesh = new Mesh { name = "Grid" };
            int lineCount = (divisions + 1) * 2;
            var verts = new Vector3[lineCount * 2];
            var indices = new int[lineCount * 2];
            float half = size * 0.5f, step = size / divisions;
            int vi = 0;

            for (int i = 0; i <= divisions; i++)
            {
                float z = -half + i * step;
                verts[vi] = new Vector3(-half, 0, z); indices[vi] = vi; vi++;
                verts[vi] = new Vector3(half, 0, z);  indices[vi] = vi; vi++;
            }
            for (int i = 0; i <= divisions; i++)
            {
                float x = -half + i * step;
                verts[vi] = new Vector3(x, 0, -half); indices[vi] = vi; vi++;
                verts[vi] = new Vector3(x, 0, half);  indices[vi] = vi; vi++;
            }

            mesh.vertices = verts;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

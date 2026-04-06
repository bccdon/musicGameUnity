using UnityEngine;

namespace PulseHighway.Visual
{
    public static class MeshFactory
    {
        public static Mesh CreateOctahedron(float radius = 0.5f)
        {
            var mesh = new Mesh();
            mesh.name = "Octahedron";

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(0, radius, 0),     // top
                new Vector3(radius, 0, 0),     // right
                new Vector3(0, 0, radius),     // front
                new Vector3(-radius, 0, 0),    // left
                new Vector3(0, 0, -radius),    // back
                new Vector3(0, -radius, 0)     // bottom
            };

            int[] triangles = new int[]
            {
                // Top faces
                0, 2, 1,
                0, 3, 2,
                0, 4, 3,
                0, 1, 4,
                // Bottom faces
                5, 1, 2,
                5, 2, 3,
                5, 3, 4,
                5, 4, 1
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public static Mesh CreateBox(Vector3 size)
        {
            var mesh = new Mesh();
            mesh.name = "Box";

            float x = size.x * 0.5f;
            float y = size.y * 0.5f;
            float z = size.z * 0.5f;

            Vector3[] vertices = new Vector3[]
            {
                // Front
                new Vector3(-x, -y, z), new Vector3(x, -y, z), new Vector3(x, y, z), new Vector3(-x, y, z),
                // Back
                new Vector3(x, -y, -z), new Vector3(-x, -y, -z), new Vector3(-x, y, -z), new Vector3(x, y, -z),
                // Top
                new Vector3(-x, y, z), new Vector3(x, y, z), new Vector3(x, y, -z), new Vector3(-x, y, -z),
                // Bottom
                new Vector3(-x, -y, -z), new Vector3(x, -y, -z), new Vector3(x, -y, z), new Vector3(-x, -y, z),
                // Right
                new Vector3(x, -y, z), new Vector3(x, -y, -z), new Vector3(x, y, -z), new Vector3(x, y, z),
                // Left
                new Vector3(-x, -y, -z), new Vector3(-x, -y, z), new Vector3(-x, y, z), new Vector3(-x, y, -z)
            };

            int[] triangles = new int[36];
            for (int face = 0; face < 6; face++)
            {
                int v = face * 4;
                int t = face * 6;
                triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                triangles[t + 3] = v; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public static Mesh CreateCylinder(float radius, float height, int segments = 24)
        {
            var mesh = new Mesh();
            mesh.name = "Cylinder";

            int vertCount = (segments + 1) * 2 + (segments + 1) * 2; // sides + caps
            var vertices = new Vector3[vertCount];
            var normals = new Vector3[vertCount];
            var tris = new System.Collections.Generic.List<int>();

            float halfHeight = height * 0.5f;
            int vi = 0;

            // Side vertices
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;

                vertices[vi] = new Vector3(x, -halfHeight, z);
                normals[vi] = new Vector3(x, 0, z).normalized;
                vi++;

                vertices[vi] = new Vector3(x, halfHeight, z);
                normals[vi] = new Vector3(x, 0, z).normalized;
                vi++;
            }

            // Side triangles
            for (int i = 0; i < segments; i++)
            {
                int a = i * 2;
                int b = i * 2 + 1;
                int c = (i + 1) * 2;
                int d = (i + 1) * 2 + 1;
                tris.Add(a); tris.Add(b); tris.Add(c);
                tris.Add(c); tris.Add(b); tris.Add(d);
            }

            // Top cap center
            int topCenter = vi;
            vertices[vi] = new Vector3(0, halfHeight, 0);
            normals[vi] = Vector3.up;
            vi++;

            // Bottom cap center
            int botCenter = vi;
            vertices[vi] = new Vector3(0, -halfHeight, 0);
            normals[vi] = Vector3.down;
            vi++;

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();

            return mesh;
        }

        public static Mesh CreatePlane(float width, float height)
        {
            var mesh = new Mesh();
            mesh.name = "Plane";

            float w = width * 0.5f;
            float h = height * 0.5f;

            mesh.vertices = new Vector3[]
            {
                new Vector3(-w, 0, -h),
                new Vector3(w, 0, -h),
                new Vector3(w, 0, h),
                new Vector3(-w, 0, h)
            };

            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
            };
            mesh.RecalculateBounds();

            return mesh;
        }

        public static Mesh CreateGridMesh(float size, int divisions)
        {
            var mesh = new Mesh();
            mesh.name = "Grid";

            int lineCount = (divisions + 1) * 2;
            var vertices = new Vector3[lineCount * 2];
            var indices = new int[lineCount * 2];

            float half = size * 0.5f;
            float step = size / divisions;
            int vi = 0;

            // Horizontal lines
            for (int i = 0; i <= divisions; i++)
            {
                float z = -half + i * step;
                vertices[vi] = new Vector3(-half, 0, z);
                indices[vi] = vi; vi++;
                vertices[vi] = new Vector3(half, 0, z);
                indices[vi] = vi; vi++;
            }

            // Vertical lines
            for (int i = 0; i <= divisions; i++)
            {
                float x = -half + i * step;
                vertices[vi] = new Vector3(x, 0, -half);
                indices[vi] = vi; vi++;
                vertices[vi] = new Vector3(x, 0, half);
                indices[vi] = vi; vi++;
            }

            mesh.vertices = vertices;
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}

using UnityEngine;

public static class MeshUtils
{
    public static void DrawPolygon(Mesh mesh, Vector3[] points)
    {
        mesh.Clear();
        mesh.vertices = points;
        int numTris = points.Length - 2;
        int[] tris = new int[numTris * 3];
        int i = 0;
        for (int t = 0; t < numTris; t++)
        {
            tris[i++] = 0;
            tris[i++] = t + 2;
            tris[i++] = t + 1;
        }
        mesh.triangles = tris;
    }
}

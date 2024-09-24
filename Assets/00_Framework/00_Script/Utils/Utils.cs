using TMPro;
using UnityEngine;

public static class Utils
{
    #region TextMeshPro
    public static TextMeshPro MakeWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), float fontSize = 40, Color textColor = default, TextAlignmentOptions alignmentOptions = TextAlignmentOptions.TopLeft, int sortingOrder = 0)
    {
        if (textColor == default)
        {
            textColor = Color.white;
        }
        return MakeWorldText(parent, text, localPosition, fontSize, textColor, alignmentOptions, sortingOrder);
    }

    public static TextMeshPro MakeWorldText(Transform parent, string text, Vector3 localPosition, float fontSize, Color color, TextAlignmentOptions alignmentOptions, int sortingOrder)
    {
        var gameObject = new GameObject("WorldText", typeof(TextMeshPro));
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = localPosition;
        
        var textMeshPro = gameObject.GetComponent<TextMeshPro>();
        textMeshPro.alignment = alignmentOptions;
        textMeshPro.text = text;
        textMeshPro.fontSize = fontSize;
        textMeshPro.color = color;
        textMeshPro.sortingOrder = sortingOrder;
        return textMeshPro;
    }
    #endregion

    #region Mesh
    public struct VertexBuffer
    {
        public Vector3[] Positions;
        public Vector2[] UVs;
        public int[] Indices;
    }

    public static Mesh MakeEmptyMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[0];
        mesh.uv = new Vector2[0];
        mesh.triangles = new int[0];
        return mesh;
    }

    public static VertexBuffer MakeEmptyMeshVertexBuffer(uint quadCount)
    {
        VertexBuffer vertexBuffer;
        vertexBuffer.Positions = new Vector3[4 * quadCount];
        vertexBuffer.UVs = new Vector2[4 * quadCount];
        vertexBuffer.Indices = new int[6 * quadCount];
        return vertexBuffer;
    }

    public static Mesh MakeMesh(Vector3 pos, float rot, Vector3 size, Vector2 uv00, Vector2 uv11)
    {
        return AddToMesh(null, pos, rot, size, uv00, uv11);
    }

    public static Mesh AddToMesh(Mesh mesh, Vector3 pos, float rot, Vector3 size, Vector2 uv00, Vector2 uv11)
    {
        if (mesh == null)
        {
            mesh = MakeEmptyMesh();
        }

        VertexBuffer vb;
        vb.Positions = new Vector3[4 + mesh.vertices.Length];
        vb.UVs = new Vector2[4 + mesh.uv.Length];
        vb.Indices = new int[6 * mesh.triangles.Length];

        mesh.vertices = vb.Positions;
        mesh.uv = vb.UVs;
        mesh.triangles = vb.Indices;

        var beginIndex = (vb.Positions.Length - 4);
        var vIndices = new int[4];
        vIndices[0] = beginIndex;
        vIndices[1] = beginIndex + 1;
        vIndices[2] = beginIndex + 2;
        vIndices[3] = beginIndex + 3;

        var halfSize = size * 0.5f;
        if (halfSize.x != halfSize.y)
        {
            vb.Positions[vIndices[0]] = pos + GetQuaternionEuler(rot) * new Vector3(-halfSize.x, halfSize.y);
            vb.Positions[vIndices[1]] = pos + GetQuaternionEuler(rot) * new Vector3(-halfSize.x, -halfSize.y);
            vb.Positions[vIndices[2]] = pos + GetQuaternionEuler(rot) * new Vector3(halfSize.x, -halfSize.y);
            vb.Positions[vIndices[3]] = pos + GetQuaternionEuler(rot) * new Vector3(halfSize.x, halfSize.y); 
        }
        else
        {
            vb.Positions[vIndices[0]] = pos + GetQuaternionEuler(rot - 270) * halfSize;
            vb.Positions[vIndices[1]] = pos + GetQuaternionEuler(rot - 180) * halfSize;
            vb.Positions[vIndices[2]] = pos + GetQuaternionEuler(rot - 90) * halfSize;
            vb.Positions[vIndices[3]] = pos + GetQuaternionEuler(rot - 0) * halfSize;
        }

        vb.UVs[vIndices[0]] = new Vector2(uv00.x, uv11.y);
        vb.UVs[vIndices[1]] = new Vector2(uv00.x, uv00.y);
        vb.UVs[vIndices[2]] = new Vector2(uv11.x, uv00.y);
        vb.UVs[vIndices[3]] = new Vector2(uv11.x, uv11.y);


        beginIndex = (vb.Indices.Length - 6);
        vb.Indices[beginIndex + 0] = vIndices[0];
        vb.Indices[beginIndex + 1] = vIndices[3];
        vb.Indices[beginIndex + 2] = vIndices[1];
        vb.Indices[beginIndex + 3] = vIndices[3];
        vb.Indices[beginIndex + 4] = vIndices[2];
        vb.Indices[beginIndex + 5] = vIndices[1];

        return mesh;
    }

    public static void AddToVertexBuffer(ref VertexBuffer vb, uint index, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11)
    {
        var beginIndex = (int)index * 4;
        var vIndices = new int[4];
        vIndices[0] = beginIndex;
        vIndices[1] = beginIndex + 1;
        vIndices[2] = beginIndex + 2;
        vIndices[3] = beginIndex + 3;

        var halfSize = baseSize * 0.5f;
        if (halfSize.x != halfSize.y)
        {
            vb.Positions[vIndices[0]] = pos + GetQuaternionEuler(rot) * new Vector3(-halfSize.x, halfSize.y);
            vb.Positions[vIndices[1]] = pos + GetQuaternionEuler(rot) * new Vector3(-halfSize.x, -halfSize.y);
            vb.Positions[vIndices[2]] = pos + GetQuaternionEuler(rot) * new Vector3(halfSize.x, -halfSize.y);
            vb.Positions[vIndices[3]] = pos + GetQuaternionEuler(rot) * new Vector3(halfSize.x, halfSize.y);
            ;
        }
        else
        {
            vb.Positions[vIndices[0]] = pos + GetQuaternionEuler(rot - 270) * halfSize;
            vb.Positions[vIndices[1]] = pos + GetQuaternionEuler(rot - 180) * halfSize;
            vb.Positions[vIndices[2]] = pos + GetQuaternionEuler(rot - 90) * halfSize;
            vb.Positions[vIndices[3]] = pos + GetQuaternionEuler(rot - 0) * halfSize;
        }

        vb.UVs[vIndices[0]] = new Vector2(uv00.x, uv11.y);
        vb.UVs[vIndices[1]] = new Vector2(uv00.x, uv00.y);
        vb.UVs[vIndices[2]] = new Vector2(uv11.x, uv00.y);
        vb.UVs[vIndices[3]] = new Vector2(uv11.x, uv11.y);

        beginIndex = (int)index * 6;
        vb.Indices[beginIndex + 0] = vIndices[0];
        vb.Indices[beginIndex + 1] = vIndices[3];
        vb.Indices[beginIndex + 2] = vIndices[1];
        vb.Indices[beginIndex + 3] = vIndices[3];
        vb.Indices[beginIndex + 4] = vIndices[2];
        vb.Indices[beginIndex + 5] = vIndices[1];
    }

    private static Quaternion[] cachedQuaternionEulerArr;
    private static void CachedQuaternionEuler()
    {
        if (cachedQuaternionEulerArr != null)
            return;

        cachedQuaternionEulerArr = new Quaternion[360];
        for (int i = 0; i < 360; ++i)
        {
            cachedQuaternionEulerArr[i] = Quaternion.Euler(0, 0, i);
        }
    }
    private static Quaternion GetQuaternionEuler(float rotFloat)
    {
        // test
        //return Quaternion.Euler(0, 0, rotFloat);

        var rot = Mathf.RoundToInt(rotFloat);
        rot %= 360;
        if (rot < 0)
            rot += 360;
        if (cachedQuaternionEulerArr == null)
        {
            CachedQuaternionEuler();
        }
        return cachedQuaternionEulerArr[rot];

    }
    #endregion
}
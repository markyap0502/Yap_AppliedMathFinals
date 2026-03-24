using System.Collections.Generic;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlatformSpawner : MonoBehaviour
{
    public Material material;
    public int instanceCount = 100;
    private Mesh cubeMesh;
    private List<Matrix4x4> matrices = new List<Matrix4x4>();
    private List<int> colliderIds = new List<int>();
    
    public float width = 1f;
    public float height = 1f;
    public float depth = 1f;

    private List<Vector2> platforms = new List<Vector2>();

    private int platformID = 0;
    private int groundID = 1;
    
    // Z-position constant for all boxes
    public float constantZPosition = 0f;
    
    // Ground plane settings
    public float groundY = -5f;
    public float groundWidth = 200f;
    public float groundDepth = 200f;
    
    void Start()
    {
        platforms.Add(new Vector2(0,0));
        platforms.Add(new Vector2(1,0));
        platforms.Add(new Vector2(2, 0));
        platforms.Add(new Vector2(3, 0));
        platforms.Add(new Vector2(7, 0));
        platforms.Add(new Vector2(8, 0));
        platforms.Add(new Vector2(9, 0));
        platforms.Add(new Vector2(12, 0));
        platforms.Add(new Vector2(12, 1));
        platforms.Add(new Vector2(12, 2));
        platforms.Add(new Vector2(13, 0));
        platforms.Add(new Vector2(14, 0));
        platforms.Add(new Vector2(15, 0));
        platforms.Add(new Vector2(16, 0));
        platforms.Add(new Vector2(17, 0));
        platforms.Add(new Vector2(20, 0));
        platforms.Add(new Vector2(23, 0));
        platforms.Add(new Vector2(24, 0));
        platforms.Add(new Vector2(25, 0));
        platforms.Add(new Vector2(28, 0));
        platforms.Add(new Vector2(29, 0));
        platforms.Add(new Vector2(29, 1));
        platforms.Add(new Vector2(29, 2));
        platforms.Add(new Vector2(30, 0));
        platforms.Add(new Vector2(31, 0));
        platforms.Add(new Vector2(32, 0));
        platforms.Add(new Vector2(33, 0));
        platforms.Add(new Vector2(34, 0));
        platforms.Add(new Vector2(35, 0));
        platforms.Add(new Vector2(36, 0));
        platforms.Add(new Vector2(36, 1));
        platforms.Add(new Vector2(36, 2));
        platforms.Add(new Vector2(37, 0));
        platforms.Add(new Vector2(38, 0));
        platforms.Add(new Vector2(39, 0));
        platforms.Add(new Vector2(40, 0));
            
        // Create the cube mesh
        CreateCubeMesh();
        
        //Create Ground
        CreateGround();
        
        // Set up platforms
        for (int i = 0; i < platforms.Count; i++)
        {
            SpawnBox(platforms[i]);
        }
    }

    void CreateCubeMesh()
    {
        cubeMesh = new Mesh();
        
        // Create 8 vertices for the cube (corners)
        Vector3[] vertices = new Vector3[8]
        {
            // Bottom face vertices
            new Vector3(0, 0, 0),       // Bottom front left - 0
            new Vector3(width, 0, 0),   // Bottom front right - 1
            new Vector3(width, 0, depth),// Bottom back right - 2
            new Vector3(0, 0, depth),   // Bottom back left - 3
            
            // Top face vertices
            new Vector3(0, height, 0),       // Top front left - 4
            new Vector3(width, height, 0),   // Top front right - 5
            new Vector3(width, height, depth),// Top back right - 6
            new Vector3(0, height, depth)    // Top back left - 7
        };
        
        // Triangles for the 6 faces (2 triangles per face)
        int[] triangles = new int[36]
        {
            // Front face triangles (facing -Z)
            0, 4, 1,
            1, 4, 5,
            
            // Back face triangles (facing +Z)
            2, 6, 3,
            3, 6, 7,
            
            // Left face triangles (facing -X)
            0, 3, 4,
            4, 3, 7,
            
            // Right face triangles (facing +X)
            1, 5, 2,
            2, 5, 6,
            
            // Bottom face triangles (facing -Y)
            0, 1, 3,
            3, 1, 2,
            
            // Top face triangles (facing +Y)
            4, 7, 5,
            5, 7, 6
        };
        
        Vector2[] uvs = new Vector2[8];
        for (int i = 0; i < 8; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / width, vertices[i].z / depth);
        }

        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.uv = uvs;
        cubeMesh.RecalculateNormals();
        cubeMesh.RecalculateBounds();
    }

    void Update()
    {
        RenderBoxes();
    }
    
    void RenderBoxes()
    {
        // Convert list to array for Graphics.DrawMeshInstanced
        Matrix4x4[] matrixArray = matrices.ToArray();
        
        // Draw instanced meshes in batches of 1023 (GPU limit)
        for (int i = 0; i < matrixArray.Length; i += 1023) {
            int batchSize = Mathf.Min(1023, matrixArray.Length - i);
            Matrix4x4[] batchMatrices = new Matrix4x4[batchSize];
            System.Array.Copy(matrixArray, i, batchMatrices, 0, batchSize);
            Graphics.DrawMeshInstanced(cubeMesh, 0, material, batchMatrices, batchSize);
        }
    }
    
    void CreateGround()
    {
        // Create a large ground plane
        Vector3 groundPosition = new Vector3(0, groundY, constantZPosition);
        Vector3 groundScale = new Vector3(groundWidth, 1f, groundDepth);
        Quaternion groundRotation = Quaternion.identity;
        
        // Register with collision system - use actual dimensions
        groundID = CollisionManager.Instance.RegisterCollider(
            groundPosition, 
            new Vector3(groundWidth, 1f, groundDepth), 
            false);
        
        // Create transformation matrix
        Matrix4x4 groundMatrix = Matrix4x4.TRS(groundPosition, groundRotation, groundScale);
        matrices.Add(groundMatrix);
        colliderIds.Add(groundID);
        // Update the matrix in collision manager
        CollisionManager.Instance.UpdateMatrix(groundID, groundMatrix);
    }

    void DecomposeMatrix(Matrix4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        position = matrix.GetPosition();
        rotation = matrix.rotation;
        scale = matrix.lossyScale;
    }

    public void SpawnBox(Vector2 pos)
    {
        Vector3 position = new Vector3(
            pos.x, pos.y,
            constantZPosition
        );
        
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        
        // Random non-uniform scale - different for each dimension
        Vector3 scale = new Vector3(
            1,
            1,
            1
        );
        
        // Register with collision system - properly handle rectangular shapes
        platformID = CollisionManager.Instance.RegisterCollider(
            position, 
            new Vector3(width * scale.x, height * scale.y, depth * scale.z), 
            false);
        
        Matrix4x4 boxMatrix = Matrix4x4.TRS(position, rotation, scale);
        matrices.Add(boxMatrix);
        colliderIds.Add(platformID);
        
        CollisionManager.Instance.UpdateMatrix(platformID, boxMatrix);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using System;
using TMPro;

// Enhanced MeshGenerator with collision, player control, and camera following
public class GameManager : MonoBehaviour
{
    //Entity Materials
    public Material playerMaterial;
    public Material platformMaterial;
    public Material goalMaterial;
    public Material groundMaterial;
    public Material enemyMaterial;
    
    //UI
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI healthText;
    public GameObject WinPanel;
    
    public int instanceCount = 100;
    private Mesh cubeMesh;
    private List<Matrix4x4> matrices = new List<Matrix4x4>();
    private List<int> colliderIds = new List<int>();
    
    //Cube Mesh
    public float width = 1f;
    public float height = 1f;
    public float depth = 1f;
    public float time;
    
    //Player
    public int playerHealth = 3;
    public float invincibilityDuration = 1.5f; // Slightly longer since they aren't pushed away
    private float invincibilityTimer = 0f;
    public float movementSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = 9.8f;
    [Range(0f, 1f)]
    public float airControl = 0.5f;
    
    //Enemy
    private Vector3 enemyPos;
    private float enemyDirection = 1f; // 1 for Right, -1 for Left
    public float enemySpeed = 3f;
    
    //MeshIDs
    private int playerID = -1;
    private int platformID = 0;
    private int groundID = 1;
    private int goalID = 2;
    private int enemyID = -5;
    
    //PlayerStates
    private Vector3 playerVelocity = Vector3.zero;
    private bool isGrounded = false;
    
    //Platforms List
    private List<Vector2> platforms = new List<Vector2>();

    //Events
    public static event Action onWinGame;
    public static event Action onLoseGame;
    
    // Camera reference
    public PlayerCameraFollow cameraFollow;
    
    // Z-position constant for all boxes
    public float constantZPosition = 0f;
    
    // Ground plane settings
    public float groundY = 0f;
    public float groundWidth = 200f;
    public float groundDepth = 1f;
    
    void Start()
    {
        Time.timeScale = 1f;
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
        platforms.Add(new Vector2(41, 0));
        
        platforms.Add(new Vector2(45, 0));
        platforms.Add(new Vector2(45, 1));
        platforms.Add(new Vector2(46, 0));
        platforms.Add(new Vector2(47, 0));
        platforms.Add(new Vector2(48, 0));
        platforms.Add(new Vector2(49, 0));
        platforms.Add(new Vector2(50, 0));
        platforms.Add(new Vector2(51, 0));
        platforms.Add(new Vector2(51, 1));
        platforms.Add(new Vector2(52, 0));
        platforms.Add(new Vector2(53, 0));
        platforms.Add(new Vector2(54, 0));
        platforms.Add(new Vector2(55, 0));
        platforms.Add(new Vector2(56, 0));
        platforms.Add(new Vector2(56, 1));
        
        // Find or create camera if not assigned
        SetupCamera();
        
        // Create the cube mesh
        CreateCubeMesh();
        
        // Create player box
        CreatePlayer();
        
        //Create Ground
        CreateGround();
        
        // Set up platforms
        for (int i = 0; i < platforms.Count - 1; i++)
        {
            SpawnBox(platforms[i], false);
        }

        // Spawn the very last one as the goal
        SpawnBox(platforms[platforms.Count - 1], true);
        
        //Spawn Enemies
        CreateEnemy(new Vector2(33,2));
    }
    
    void SetupCamera()
    {
        if (cameraFollow == null)
        {
            // Try to find existing camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Check if it already has our script
                cameraFollow = mainCamera.GetComponent<PlayerCameraFollow>();
                if (cameraFollow == null)
                {
                    // Add our script to existing camera
                    cameraFollow = mainCamera.gameObject.AddComponent<PlayerCameraFollow>();
                }
            }
            else
            {
                // No main camera found, create a new one
                GameObject cameraObj = new GameObject("PlayerCamera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cameraFollow = cameraObj.AddComponent<PlayerCameraFollow>();
                
                // Set this as the main camera
                cam.tag = "MainCamera";
            }
            
            // Configure default camera settings
            cameraFollow.offset = new Vector3(0, 0, -15);
            cameraFollow.smoothSpeed = 0.1f;
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
    
    void CreatePlayer()
    {
        // Create player at a specific position
        Vector3 playerPosition = new Vector3(0, 2, constantZPosition);
        Vector3 playerScale = Vector3.one;
        Quaternion playerRotation = Quaternion.identity;
        
        // Register with collision system - properly handle width/height/depth
        playerID = CollisionManager.Instance.RegisterCollider(
            playerPosition, 
            new Vector3(width * playerScale.x, height * playerScale.y, depth * playerScale.z), 
            true);
        
        // Create transformation matrix
        Matrix4x4 playerMatrix = Matrix4x4.TRS(playerPosition, playerRotation, playerScale);
        matrices.Add(playerMatrix);
        colliderIds.Add(playerID);
        
        // Update the matrix in collision manager
        CollisionManager.Instance.UpdateMatrix(playerID, playerMatrix);
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
    
    public void SpawnBox(Vector2 pos, bool isGoal)
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
        if (isGoal)
        {
            goalID = CollisionManager.Instance.RegisterCollider(
                position, 
                new Vector3(width * scale.x, height * scale.y, depth * scale.z), 
                false);
            Matrix4x4 goalMatrix = Matrix4x4.TRS(position, rotation, scale);
            matrices.Add(goalMatrix);
            colliderIds.Add(goalID);
            CollisionManager.Instance.UpdateMatrix(goalID, goalMatrix);
        }
        else
        {
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
    
    void CreateEnemy(Vector2 startGridPos)
    {
        enemyPos = new Vector3(startGridPos.x, startGridPos.y, constantZPosition);
    
        // Register the enemy with the collision system
        enemyID = CollisionManager.Instance.RegisterCollider(
            enemyPos, 
            new Vector3(width, height, depth), 
            true); // true because it moves (dynamic)

        // Add to our rendering lists
        Matrix4x4 enemyMatrix = Matrix4x4.TRS(enemyPos, Quaternion.identity, Vector3.one);
        matrices.Add(enemyMatrix);
        colliderIds.Add(enemyID);
    }
    
    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
        UpdatePlayer();
        UpdateEnemy();
        RenderBoxes();
        time += Time.deltaTime;
        UpdateUI();
    }

    void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        healthText.text = playerHealth.ToString();
    }
    
    void UpdatePlayer()
    {
        if (playerID == -1) return;
        
        // Get current player matrix
        Matrix4x4 playerMatrix = matrices[colliderIds.IndexOf(playerID)];
        DecomposeMatrix(playerMatrix, out Vector3 pos, out Quaternion rot, out Vector3 scale);
        
        // --- 1. MOVEMENT ---
        float horizontal = 0;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1;
        if (Input.GetKey(KeyCode.D)) horizontal += 1;
        
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            playerVelocity.y = jumpForce;
            isGrounded = false;
        }

        float currentSpeed = movementSpeed * (isGrounded ? 1f : airControl);
        Vector3 nextPosX = pos + new Vector3(horizontal * currentSpeed * Time.deltaTime, 0, 0);

        // Check if moving sideways hits anything
        if (CollisionManager.Instance.CheckCollision(playerID, nextPosX, out List<int> sideHitIds))
        {
            // Check for Win/Lose even if we just bumped into it sideways
            CheckGameStatus(sideHitIds);
    
            // Stop horizontal movement (Wall collision)
            playerVelocity.x = 0; 
        }
        else
        {
            pos.x = nextPosX.x;
        }
        
        // --- 2. VERTICAL MOVEMENT (Up/Down) ---
        if (!isGrounded)
        {
            playerVelocity.y -= gravity * Time.deltaTime;
        }

        Vector3 nextPosY = pos + new Vector3(0, playerVelocity.y * Time.deltaTime, 0);

        if (CollisionManager.Instance.CheckCollision(playerID, nextPosY, out List<int> vertHitIds))
        {
            CheckGameStatus(vertHitIds);

            if (playerVelocity.y <= 0) // Hitting floor
            {
                isGrounded = true;
                playerVelocity.y = 0;
            }
            else // Hitting ceiling
            {
                playerVelocity.y = 0;
            }
        }
        else
        {
            pos.y = nextPosY.y;
            // Probe slightly down to see if we are still standing on something
            isGrounded = CheckCollisionAt(playerID, pos + new Vector3(0, -0.01f, 0));
        }
        
        // Update matrix
        Matrix4x4 newMatrix = Matrix4x4.TRS(pos, rot, scale);
        matrices[colliderIds.IndexOf(playerID)] = newMatrix;
        
        // Update collider position - properly handle rectangular shape
        CollisionManager.Instance.UpdateCollider(playerID, pos, new Vector3(width * scale.x, height * scale.y, depth * scale.z));
        CollisionManager.Instance.UpdateMatrix(playerID, newMatrix);
        
        // Update camera to follow player
        if (cameraFollow != null)
        {
            cameraFollow.SetPlayerPosition(pos);
        }
    }
    
    void UpdateEnemy()
    {
        // 1. Calculate the intended move
        float moveAmount = enemyDirection * enemySpeed * Time.deltaTime;
        Vector3 nextPos = enemyPos + new Vector3(moveAmount, 0, 0);

        // 2. Check for wall collision
        if (CollisionManager.Instance.CheckCollision(enemyID, nextPos, out List<int> hitIds))
        {
            // If we hit a platform (not the player), turn around
            foreach (int id in hitIds)
            {
                if (id != playerID) 
                {
                    enemyDirection *= -1f; // Reverse direction
                    break;
                }
            }
        }
        else
        {
            // No wall? Move forward
            enemyPos = nextPos;
        }

        // 3. Update the visual matrix and the collider
        int index = colliderIds.IndexOf(enemyID);
        Matrix4x4 newMatrix = Matrix4x4.TRS(enemyPos, Quaternion.identity, Vector3.one);
        matrices[index] = newMatrix;
    
        CollisionManager.Instance.UpdateMatrix(enemyID, newMatrix);
        CollisionManager.Instance.UpdateCollider(enemyID, enemyPos, new Vector3(width, height, depth));
    }
    
    void CheckGameStatus(List<int> hitIds)
    {
        if (hitIds.Contains(groundID)) onLoseGame?.Invoke();
        if (hitIds.Contains(goalID)) onWinGame?.Invoke();
        if (hitIds.Contains(enemyID) && invincibilityTimer <= 0)
        {
            ApplyDamage();
        }
    }
    
    void ApplyDamage()
    {
        if (invincibilityTimer > 0) return;

        playerHealth--;
        Debug.Log("Player Hit! Health remaining: " + playerHealth);

        if (playerHealth <= 0)
        {
            onLoseGame?.Invoke();
            return;
        }
        invincibilityTimer = invincibilityDuration;
    }
    
    
    bool CheckCollisionAt(int id, Vector3 position)
    {
        return CollisionManager.Instance.CheckCollision(id, position, out _);
    }
    
    void RenderBoxes() 
    {
        List<Matrix4x4> playerMats = new List<Matrix4x4>();
        List<Matrix4x4> platformMats = new List<Matrix4x4>();
        List<Matrix4x4> goalMats = new List<Matrix4x4>();
        List<Matrix4x4> groundMats = new List<Matrix4x4>();
        List<Matrix4x4> enemyMats = new List<Matrix4x4>();
        
        for (int i = 0; i < matrices.Count; i++)
        {
            int id = colliderIds[i];
            if (id == playerID) playerMats.Add(matrices[i]);
            else if (id == goalID) goalMats.Add(matrices[i]);
            else if (id == groundID) groundMats.Add(matrices[i]);
            else if (id == enemyID) enemyMats.Add(matrices[i]);
            else platformMats.Add(matrices[i]);
        }
        
        if (invincibilityTimer > 0)
        {
            if (Mathf.Sin(Time.time * 30f) > 0) 
            {
                DrawBatch(playerMats, playerMaterial);
            }
        }
        else
        {
            DrawBatch(playerMats, playerMaterial);
        }
        DrawBatch(platformMats, platformMaterial);
        DrawBatch(goalMats, goalMaterial);
        DrawBatch(groundMats, groundMaterial);
        DrawBatch(enemyMats, enemyMaterial);
    }
    
    void DrawBatch(List<Matrix4x4> mats, Material mat)
    {
        if (mats.Count == 0) return;
    
        Matrix4x4[] array = mats.ToArray();
        for (int i = 0; i < array.Length; i += 1023)
        {
            int batchSize = Mathf.Min(1023, array.Length - i);
            Matrix4x4[] batch = new Matrix4x4[batchSize];
            System.Array.Copy(array, i, batch, 0, batchSize);
            Graphics.DrawMeshInstanced(cubeMesh, 0, mat, batch, batchSize);
        }
    }

    void DecomposeMatrix(Matrix4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
    {
        position = matrix.GetPosition();
        rotation = matrix.rotation;
        scale = matrix.lossyScale;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
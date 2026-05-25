using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Windows;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    [RequireComponent(typeof(RectTransform))]
    public class Puzzle : MonoBehaviour
    {
        // Editor options.
        [SerializeField]
        [Tooltip("Debug puzzle initialization time?")]
        private bool debugInitTime = true;
        [SerializeField]
        [Tooltip("Start puzzle with pieces at correct positions?")]
        private bool startAssembled = false;
        [SerializeField]
        private PuzzlePiece puzzlePiecePrefab;

        // Component data.
        private SavedGameData savedGameData;
        private bool isActive;
        private bool initialized;
        private float previousScale;
        private Vector3 previousPosition;
        private System.Diagnostics.Stopwatch puzzleInitTimer;
        /// <summary>
        /// Stores mask helper values.
        /// </summary>
        private MaskMaterialHelper maskMaterialHelper;
        private Shader puzzlePieceShader;
        private Material puzzleMeshMaterial;
        private List<Material> puzzlePieceMaterials;
#if UNITY_EDITOR
        // Just so masks are visible in editor.
        [SerializeField]
#endif
        private List<RenderTexture> masks;

        // Texture job components.
        private Coroutine textureToPuzzleRoutine;
        private PuzzleMeshDataJob textureToPuzzleJob;
        private JobHandle jobHandle;
        private List<PuzzlePiece> allPuzzlePieces = new List<PuzzlePiece>();

        public RectTransform RectTransform { get; private set; }
        public Texture PuzzleTexture { get; private set; }
        public List<PuzzlePiece> PuzzlePieces { get; private set; } = new List<PuzzlePiece>();
        public List<PuzzlePieceCluster> Clusters { get; private set; } = new List<PuzzlePieceCluster>();
        public PuzzleData PuzzleData { get; private set; }
        public float PuzzlePieceWorldSize { get; private set; }
        public float PuzzlePieceRectSize { get; private set; }
        public Vector2 HalfPuzzlePieceWorldSize { get; private set; }
        public Rect PuzzleBoardWorldRect { get; private set; }
        public bool StartAssembled => startAssembled;
        public Rect PuzzleTextureRect { get; private set; }

        // Settings.
        public bool PiecesRotationEnabled { get; set; }
        public bool FreeSnapEnabled { get; set; }
        public bool ShuffleOnInitialized { get; set; }

        // Actions.
        public Action OnPuzzleSizeUpdated { get; set; }
        public Action<PuzzlePiece> OnPuzzlePieceCreated { get; set; }
        public Action<bool> OnPuzzleInitialized { get; set; }
        public Action<PuzzlePieceCluster> OnClusterRemoved { get; set; }
        public Action OnPuzzleClearedValues { get; set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();

            previousScale = transform.lossyScale.x;
            previousPosition = transform.position;
            maskMaterialHelper = default;
            masks = new List<RenderTexture>();
            puzzlePieceMaterials = new List<Material>();
            puzzlePieceShader = Shader.Find("JigsawPuzzle/Default");
            puzzleMeshMaterial = new Material(Shader.Find("Unlit/Color"));
        }

        private void FixedUpdate()
        {
            if (previousScale != transform.lossyScale.x || previousPosition != transform.position)
            {
                previousScale = transform.lossyScale.x;
                previousPosition = transform.position;

                ViewRectUpdated();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!isActive)
            {
                return;
            }

            Color prev = Gizmos.color;
            Gizmos.color = Color.gray;
            Vector3[] corners = new Vector3[4];
            RectTransform.GetWorldCorners(corners);
            Vector2 size = corners[2] - corners[0];

            for (int x = 0; x < PuzzleData.columns + 1; x++)
            {
                float xPos = x * size.x / PuzzleData.columns;
                Gizmos.DrawLine(
                    corners[0] + new Vector3(xPos, 0f),
                    corners[0] + new Vector3(xPos, size.y));
            }

            for (int y = 0; y < PuzzleData.rows + 1; y++)
            {
                float yPos = y * size.y / PuzzleData.rows;
                Gizmos.DrawLine(
                    corners[0] + new Vector3(0f, yPos),
                    corners[0] + new Vector3(size.x, yPos));
            }
            Gizmos.color = prev;
        }
#endif

        public void Initialize(
            PuzzleDataPackage puzzleDataPackage,
            SavedGameData savedGameData = null,
            float puzzlePieceSize = 100f) =>
            Initialize(
                puzzleDataPackage.puzzleData, 
                puzzleDataPackage.puzzleTexture,
                savedGameData,
                puzzlePieceSize);

        public void Initialize(
            PuzzleData puzzleData, 
            Texture puzzleTexture,
            SavedGameData savedGameData = null,
            float puzzlePieceSize = 100f)
        {
            // If active, clear first.
            if (isActive)
            {
                Clear();
            }

            #region Debug option.

            // Start timer for tracking init time.
            if (debugInitTime)
            {
                puzzleInitTimer = new System.Diagnostics.Stopwatch();
                puzzleInitTimer.Start();
            }

            #endregion

            // Prepare puzzle data.
            this.savedGameData = savedGameData;
            PuzzleTexture = puzzleTexture;
            PuzzleData = puzzleData;
            PuzzleTextureRect = GetTextureRect();

            // Update puzzle size.
            RectTransform.sizeDelta = new Vector2(puzzleData.columns * puzzlePieceSize, puzzleData.rows * puzzlePieceSize);
            PuzzlePieceRectSize = puzzlePieceSize;

            OnPuzzleSizeUpdated?.Invoke();

            ViewRectUpdated();
            #region Create job for separating puzzle pieces in target texture.

            // Create new instance of texuteToPuzzleJob, and allocate it's input data.
            textureToPuzzleJob = new PuzzleMeshDataJob()
            {
                maxPointsPerEdge = new NativeReference<int>(Settings.PuzzleSettings.PointsPerEdge, Allocator.Persistent),
            };
            textureToPuzzleJob.Init(
                puzzleData, 
                Settings.PuzzleSettings.PuzzlePiecePaddingSize);

            // Create job handle, and schedule it. Amount of execution tasks is equal to amount of polygons.
            // CPU will use all available threads to work on it until complete.
            jobHandle = textureToPuzzleJob.Schedule(puzzleData.columns * puzzleData.rows, 1);
            // And start coroutine to wait for it's completion.
            textureToPuzzleRoutine = StartCoroutine(JobToPuzzleRoutine());

            #endregion
        }

        /// <summary>
        /// Check whether puzzle piece can be placed onto board, or be connected to cluster.
        /// </summary>
        /// <param name="piece"></param>
        public void CheckPuzzlePiece(PuzzlePiece piece)
        {
            // Puzzle piece rect.
            Vector3[] corners = new Vector3[4];
            piece.RectTransform.GetWorldCorners(corners);
            Rect pieceRect = new Rect(corners[0].x, corners[0].y, (corners[2] - corners[0]).x, (corners[2] - corners[0]).y);

            Vector2 pieceOnPuzzleBoardPosition = pieceRect.min - PuzzleBoardWorldRect.min;
            // Find piece BoardLocation.
            Vector2Int pieceLocation = new Vector2Int(
                Mathf.RoundToInt(pieceOnPuzzleBoardPosition.x / PuzzlePieceWorldSize),
                Mathf.RoundToInt(pieceOnPuzzleBoardPosition.y / PuzzlePieceWorldSize));

            // Only conitnue if puzzle piece have right orientation.
            if (piece.Direction == PuzzlePieceDirection.Up)
            {
                bool snappedToBoard = false;
                // Check if puzzle piece can be snapped to board.
                if (piece.Polygon.location.Equals(pieceLocation))
                {
                    bool canSnap = 
                        // Continue if freeSnap enabled.
                        FreeSnapEnabled ||
                        // or check for neighbors.
                        piece.GetClusterPieces().Any(piece => PieceOnEdge(piece) || GetNeighborsOnBoard(piece).Count > 0);

                    if (canSnap)
                    {
                        // Attach puzzle piece/cluster to puzzle board.
                        piece.TrySnapClusterToBoard();

                        // Check with surrounding pieces to snap to board.
                        if (CheckSurrounding(piece, out Dictionary<PuzzlePiece, List<PuzzlePiece>> snapTo))
                        {
                            foreach (var snapInfo in snapTo)
                            {
                                foreach (PuzzlePiece target in snapInfo.Value)
                                {
                                    if (target.State != PuzzlePieceState.Board)
                                    {
                                        // Snap to board
                                        target.TrySnapClusterToBoard();
                                    }
                                }
                            }
                        }

                        snappedToBoard = true;
                    }

                    // Play snappedToBoard animations.
                    if (snappedToBoard)
                    {
                        StartCoroutine(CirclePiecesAnimationRoutine(
                            piece.RectTransform.localPosition,
                            PuzzlePieces.Where(piece => piece.State == PuzzlePieceState.Board).ToList(),
                            PuzzlePieceAnimationType.SnappedToBoardAnimation));
                    }
                }

                // If wasn't snapped to board, check if can be connected to a cluster.
                if (!snappedToBoard)
                {
                    // Check with surrounding pieces to form clusters.
                    if (CheckSurrounding(piece, out Dictionary<PuzzlePiece, List<PuzzlePiece>> snapTo))
                    {
                        foreach (var snapInfo in snapTo)
                        {
                            foreach (PuzzlePiece target in snapInfo.Value)
                            {
                                // Only join targets that are not part of a cluster.
                                if (piece.Cluster == null || !piece.Cluster.PuzzlePieces.Contains(target))
                                {
                                    piece.ConnectTo(target);
                                }
                            }
                        }

                        // Play snappedToCluster animations.
                        if (snapTo.Count > 0)
                        {
                            // Either new cluster was created, on existing modified. So play cluster animation.
                            StartCoroutine(CirclePiecesAnimationRoutine(
                                piece.RectTransform.localPosition,
                                piece.GetClusterPieces(),
                                PuzzlePieceAnimationType.SnappedToClusterAnimation));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find any puzzle pieces close by to a target that it can be connecte to. 
        /// Also does the same for all the other pieces if it's a part of a cluster.
        /// </summary>
        /// <param name="target">Piece to check around.</param>
        /// <param name="results">Returns dictionay with piece as a key, and value as list of pieces surrounding it.</param>
        /// <returns>True if have any surrounding pieces.</returns>
        public bool CheckSurrounding(PuzzlePiece target, out Dictionary<PuzzlePiece, List<PuzzlePiece>> results)
        {
            results = new Dictionary<PuzzlePiece, List<PuzzlePiece>>();
            List<PuzzlePiece> clusterPuzzlePieces = target.GetClusterPieces();

            // Check all pieces of this cluster.
            foreach (PuzzlePiece clusterPiece in clusterPuzzlePieces)
            {
                foreach (PuzzlePiece piece in
                    PuzzlePieces.Where(piece =>
                        // Ignore pieces with wrong direction.
                        piece.Direction == PuzzlePieceDirection.Up &&
                        // Ignore snapped pieces.
                        piece.State != PuzzlePieceState.Board &&
                        // Ignore cluster members.
                        !clusterPuzzlePieces.Contains(piece)))
                {
                    Vector2 posDiff = clusterPiece.transform.position - piece.transform.position;

                    // Skip pieces that are too far or too close.
                    float distance = posDiff.magnitude;
                    if (distance < PuzzlePieceWorldSize * Settings.PuzzleSettings.PieceSnapPieceMinDistance ||
                        distance > PuzzlePieceWorldSize * Settings.PuzzleSettings.PieceSnapPieceMaxDistance)
                    {
                        continue;
                    }

                    Vector2 minVector = new Vector2(posDiff.y, posDiff.y);
                    float max = posDiff.x;
                    if (Mathf.Abs(posDiff.x) < Mathf.Abs(posDiff.y))
                    {
                        minVector = new Vector2(posDiff.x, posDiff.x);
                        max = posDiff.y;
                    }

                    // Skip pieces that deviate too much horizontally or vertically.
                    float directionDeviation = Mathf.Abs(minVector.x) / Mathf.Abs(max);
                    if (directionDeviation > Settings.PuzzleSettings.PieceSnapMaxDirectionDeviation)
                    {
                        continue;
                    }

                    Vector2 direction = (posDiff - minVector).normalized;

                    // Check if target can snap to this piece.
                    if (piece.Polygon.location + direction == clusterPiece.Polygon.location)
                    {
                        if (!results.ContainsKey(clusterPiece))
                        {
                            results.Add(clusterPiece, new List<PuzzlePiece>());
                        }

                        results[clusterPiece].Add(piece);
                    }
                }
            }

            return results.Count > 0;
        }

        /// <summary>
        /// Returns neighbors by board location (if any present at each neighbor location).
        /// </summary>
        /// <param name="piece">Target piece.</param>
        /// <returns>List of neigbors.</returns>
        public List<PuzzlePiece> GetNeighborsOnBoard(PuzzlePiece piece)
        {
            return PuzzlePieces.FindAll(toCheck =>
                toCheck.State == PuzzlePieceState.Board &&
                (toCheck.Polygon.location == piece.Polygon.location + new Vector2Int(1, 0) ||
                 toCheck.Polygon.location == piece.Polygon.location + new Vector2Int(-1, 0) ||
                 toCheck.Polygon.location == piece.Polygon.location + new Vector2Int(0, 1) ||
                 toCheck.Polygon.location == piece.Polygon.location + new Vector2Int(0, -1)));
        }

        /// <summary>
        /// Checks if puzzle piece lays on board edge.
        /// </summary>
        /// <param name="piece">Target piece.</param>
        /// <returns></returns>
        public bool PieceOnEdge(PuzzlePiece piece)
        {
            return piece.Polygon.location.x == 0 || piece.Polygon.location.x == PuzzleData.columns - 1 ||
                   piece.Polygon.location.y == 0 || piece.Polygon.location.y == PuzzleData.rows - 1;
        }

        /// <summary>
        /// Creates new cluster.
        /// </summary>
        /// <returns></returns>
        public PuzzlePieceCluster CreateCluster()
        {
            PuzzlePieceCluster cluster = new PuzzlePieceCluster(this);
            Clusters.Add(cluster);

            return cluster;
        }

        /// <summary>
        /// Clears target cluster.
        /// </summary>
        /// <param name="cluster"></param>
        public void ClearCluster(PuzzlePieceCluster cluster)
        {
            OnClusterRemoved?.Invoke(cluster);

            foreach (PuzzlePiece piece in cluster.PuzzlePieces)
            {
                piece.Cluster = null;
            }

            Clusters.Remove(cluster);
        }

        /// <summary>
        /// Clears puzzle assets. 
        /// Resets all puzzle pieces.
        /// </summary>
        public void Clear()
        {
            // Clear mask textures from memory.
            foreach (RenderTexture renderTexture in masks)
            {
                Destroy(renderTexture);
            }
            masks.Clear();
            // Clear materials from memory.
            foreach (Material maskMaterial in puzzlePieceMaterials)
            {
                Destroy(maskMaterial);
            }
            puzzlePieceMaterials.Clear();

            // Reset puzzle pieces.
            for (int puzzlePieceIndex = 0; puzzlePieceIndex < PuzzlePieces.Count; puzzlePieceIndex++)
            {
                PuzzlePieces[puzzlePieceIndex].ResetToDefault();

                PuzzlePieces[puzzlePieceIndex].transform.SetParent(transform, false);
                PuzzlePieces[puzzlePieceIndex].transform.SetAsLastSibling();
                PuzzlePieces[puzzlePieceIndex].transform.localScale = Vector3.one;
                PuzzlePieces[puzzlePieceIndex].gameObject.SetActive(false);
            }

            // Clear clusters.
            while (Clusters.Count > 0)
            {
                ClearCluster(Clusters[0]);
            }

            StopAllCoroutines();

            isActive = false;
            OnPuzzleClearedValues?.Invoke();
        }

        /// <summary>
        /// Creates new puzzle piece.
        /// </summary>
        /// <returns></returns>
        private PuzzlePiece InitPuzzlePiece()
        {
            PuzzlePiece piece = Instantiate(puzzlePiecePrefab, transform);

            return piece;
        }

        /// <summary>
        /// Applies saved game data to already initialized puzzle.
        /// </summary>
        private void ApplySavedGameData()
        {
            // Order pieces data by cluster id, so clusters are created as needed.
            foreach (PuzzlePieceSaveData savedPieceData in savedGameData.data.OrderBy(data => data.clusterId))
            {
                PuzzlePiece piece = PuzzlePieces.Find(piece => piece.Polygon.location == savedPieceData.boardLocation);

                if (piece == null)
                {
                    Debug.LogError($"Fake save file located, please clear it {savedGameData.puzzleId}");
                    continue;
                }

                // Apply rotation (only if enabled in settings).
                piece.RotateTo(savedPieceData.direction);

                switch (savedPieceData.state)
                {
                    // Case for already snapped pieces.
                    case PuzzlePieceState.Board:
                        piece.SnapToBoard(PuzzlePieceEventOrigin.SaveFile);

                        break;

                    // Case for free-floating puzzle pieces and clusters.
                    case PuzzlePieceState.Overlay:
                        piece.State = PuzzlePieceState.Overlay;
                        piece.transform.localPosition = savedPieceData.position;

                        if (savedPieceData.clusterId > -1)
                        {
                            // Create new if needed.
                            if (Clusters.Count <= savedPieceData.clusterId)
                            {
                                CreateCluster();
                            }

                            // And join it.
                            piece.JoinCluster(Clusters[savedPieceData.clusterId], PuzzlePieceEventOrigin.SaveFile);
                        }

                        break;
                }
            }
        }

        private Rect GetTextureRect()
        {
            float puzzleAspectRatio = (float)PuzzleData.columns / PuzzleData.rows;
            float textureAspectRatio = (float)PuzzleTexture.width / PuzzleTexture.height;

            Vector2 rectSize;
            Vector2 rectPos;
            if (textureAspectRatio > puzzleAspectRatio)
            {
                float aspectDiff = puzzleAspectRatio / textureAspectRatio;
                rectSize = new Vector2(aspectDiff, 1f);
                rectPos = new Vector2((1f - aspectDiff) * .5f, 0f);
            }
            else
            {
                float aspectDiff = textureAspectRatio / puzzleAspectRatio;
                rectSize = new Vector2(1f, aspectDiff);
                rectPos = new Vector2(0f, (1f - aspectDiff) * .5f);
            }

            return new Rect(rectPos, rectSize);
        }

        private void GeneratePuzzlePieces()
        {
            int previousIndex = allPuzzlePieces.Count;
            while (allPuzzlePieces.Count < PuzzleData.polygons.Length)
            {
                allPuzzlePieces.Add(InitPuzzlePiece());
            }

            // Only use the required amount of puzzlePieces.
            PuzzlePieces = allPuzzlePieces.Take(PuzzleData.polygons.Length).ToList();
            for (int puzzlePieceIndex = 0; puzzlePieceIndex < PuzzleData.polygons.Length; puzzlePieceIndex++)
            {
                int pieceX = puzzlePieceIndex % PuzzleData.columns;
                int pieceY = puzzlePieceIndex / PuzzleData.columns;

                int materialX = pieceX / maskMaterialHelper.maxPiecesPerTexture.x;
                int materialY = pieceY / maskMaterialHelper.maxPiecesPerTexture.y;
                // Get material index.
                int materialIndex = materialY * maskMaterialHelper.maxTextures.x + materialX;
                Material maskMaterial = puzzlePieceMaterials[materialIndex];

                Vector2 maskRelativeTextureSize = new Vector2(
                    (float)masks[materialIndex].width / maskMaterialHelper.totalTextureSize.x, 
                    (float)masks[materialIndex].height / maskMaterialHelper.totalTextureSize.y);
                Vector2 maskPositionOffset = new Vector2(
                    (float)Settings.PuzzleSettings.MaxMaskTextureSize.x / maskMaterialHelper.totalTextureSize.x * materialX,
                    (float)Settings.PuzzleSettings.MaxMaskTextureSize.y / maskMaterialHelper.totalTextureSize.y * materialY);

                PuzzlePiece puzzlePiece = PuzzlePieces[puzzlePieceIndex];
                // Activate and setup puzzle piece.
                puzzlePiece.gameObject.SetActive(true);
                puzzlePiece.State = PuzzlePieceState.None;

                // Update maskRect to utilize multiple mask textures.
                Rect maskRect = textureToPuzzleJob.GetMaskRect(puzzlePieceIndex);
                maskRect.position -= maskPositionOffset;
                maskRect.position /= maskRelativeTextureSize;
                maskRect.size /= maskRelativeTextureSize;

                Rect textureUVRect = textureToPuzzleJob.GetTextureRect(puzzlePieceIndex);
                // Stretch texturePolygonRect over puzzle rect.
                textureUVRect.position = PuzzleTextureRect.position + (textureUVRect.position * PuzzleTextureRect.size);
                textureUVRect.size *= PuzzleTextureRect.size;

                puzzlePiece.SetMaterial(maskMaterial);
                puzzlePiece.ConfigureTexture(
                    PuzzleTexture,
                    textureUVRect,
                    maskRect);
                puzzlePiece.UpdatePivotPoint(
                    textureToPuzzleJob.GetPolygonAnchorMin(puzzlePieceIndex),
                    textureToPuzzleJob.GetPolygonAnchorMax(puzzlePieceIndex));
                puzzlePiece.SetPolygonData(PuzzleData.polygons[puzzlePieceIndex]);
                puzzlePiece.SetSize(PuzzlePieceRectSize);
                puzzlePiece.SetPuzzleObject(this);

                if (startAssembled)
                {
                    Vector2 targetPosition =
                        HalfPuzzlePieceWorldSize +
                        PuzzleBoardWorldRect.min +
                        new Vector2(
                            PuzzleData.polygons[puzzlePieceIndex].location.x * PuzzlePieceWorldSize,
                            PuzzleData.polygons[puzzlePieceIndex].location.y * PuzzlePieceWorldSize);

                    puzzlePiece.transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
                }
            }

            // Invoke piece created event for all newly created pieces.
            for (int i = previousIndex; i < PuzzlePieces.Count; i++)
            {
                OnPuzzlePieceCreated?.Invoke(PuzzlePieces[i]);
            }
        }

        private void GenerateMaskFromMesh()
        {
            Vector2 pieceSize = new Vector2(
                1 + Settings.PuzzleSettings.PuzzlePiecePaddingSize * 2,
                1 + Settings.PuzzleSettings.PuzzlePiecePaddingSize * 2
            );

            maskMaterialHelper.maxPiecesPerTexture = new Vector2Int(
                Mathf.FloorToInt(Settings.PuzzleSettings.MaxMaskTextureSize.x / Settings.PuzzleSettings.PuzzlePiecePixelResolution),
                Mathf.FloorToInt(Settings.PuzzleSettings.MaxMaskTextureSize.y / Settings.PuzzleSettings.PuzzlePiecePixelResolution));
            maskMaterialHelper.maxTextures = new Vector2Int(
                Mathf.CeilToInt((float)PuzzleData.columns / maskMaterialHelper.maxPiecesPerTexture.x),
                Mathf.CeilToInt((float)PuzzleData.rows / maskMaterialHelper.maxPiecesPerTexture.y));
            maskMaterialHelper.totalTextureSize = new Vector2Int(
                PuzzleData.columns * Settings.PuzzleSettings.PuzzlePiecePixelResolution,
                PuzzleData.rows * Settings.PuzzleSettings.PuzzlePiecePixelResolution);

            RenderTexture prev = RenderTexture.active;
            Vector4 highlightStrength = new Vector4(
                puzzlePiecePrefab.HighlightStrength.x / 100f,
                puzzlePiecePrefab.HighlightStrength.y / 100f,
                puzzlePiecePrefab.HighlightStrength.z / 100f,
                puzzlePiecePrefab.HighlightStrength.w / 100f
            );
            float highlightSize = puzzlePiecePrefab.HighlightEffectSize / 100f;
            Mesh mesh = textureToPuzzleJob.GetMesh();
            Material blur = new Material(Shader.Find("JigsawPuzzle/Blur"));
            for (int yIndex = 0; yIndex < maskMaterialHelper.maxTextures.y; yIndex++)
            {
                for (int xIndex = 0; xIndex < maskMaterialHelper.maxTextures.x; xIndex++)
                {
                    Vector2 worldPosFrom = new Vector2(
                        Mathf.Min(xIndex * maskMaterialHelper.maxPiecesPerTexture.x * pieceSize.x, textureToPuzzleJob.meshSize.Value.x),
                        Mathf.Min(yIndex * maskMaterialHelper.maxPiecesPerTexture.y * pieceSize.y, textureToPuzzleJob.meshSize.Value.y));
                    Vector2 worldPosTo = new Vector2(
                        Mathf.Min((xIndex + 1) * maskMaterialHelper.maxPiecesPerTexture.x * pieceSize.x, textureToPuzzleJob.meshSize.Value.x),
                        Mathf.Min((yIndex + 1) * maskMaterialHelper.maxPiecesPerTexture.y * pieceSize.y, textureToPuzzleJob.meshSize.Value.y));

                    Vector2Int textureFrom = new Vector2Int(
                        Mathf.Min(
                            xIndex * maskMaterialHelper.maxPiecesPerTexture.x * Settings.PuzzleSettings.PuzzlePiecePixelResolution,
                            maskMaterialHelper.totalTextureSize.x),
                        Mathf.Min(
                            yIndex * maskMaterialHelper.maxPiecesPerTexture.y * Settings.PuzzleSettings.PuzzlePiecePixelResolution,
                            maskMaterialHelper.totalTextureSize.y));
                    Vector2Int textureTo = new Vector2Int(
                        Mathf.Min(
                            (xIndex + 1) * maskMaterialHelper.maxPiecesPerTexture.x * Settings.PuzzleSettings.PuzzlePiecePixelResolution,
                            maskMaterialHelper.totalTextureSize.x),
                        Mathf.Min(
                            (yIndex + 1) * maskMaterialHelper.maxPiecesPerTexture.y * Settings.PuzzleSettings.PuzzlePiecePixelResolution, 
                            maskMaterialHelper.totalTextureSize.y));

                    // R8 fails silently in Unity 6 URP (shader falls back to white default = square tiles).
                    RenderTexture mask = new RenderTexture(
                        textureTo.x - textureFrom.x,
                        textureTo.y - textureFrom.y,
                        0,
                        RenderTextureFormat.ARGB32);
                    mask.Create();

                    // CommandBuffer.ExecuteCommandBuffer is unreliable in Unity 6 URP for off-screen RTs.
                    // Use GL immediately-mode approach instead — bypasses URP pipeline entirely.
                    RenderTexture.active = mask;
                    GL.Clear(true, true, Color.clear);
                    GL.PushMatrix();
                    GL.LoadProjectionMatrix(Matrix4x4.Ortho(
                        worldPosFrom.x,
                        worldPosTo.x,
                        worldPosFrom.y,
                        worldPosTo.y,
                        .1f,
                        2f));
                    GL.modelview = Matrix4x4.TRS(new Vector3(0f, 0f, -1f), Quaternion.identity, Vector3.one);
                    puzzleMeshMaterial.SetPass(0);
#pragma warning disable CS0618
                    Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
#pragma warning restore CS0618
                    GL.PopMatrix();

                    // Blur mask?
                    if (Settings.PuzzleSettings.BlurMaskSize > 0f)
                    {
                        int piecesX = Mathf.CeilToInt((worldPosTo.x - worldPosFrom.x) / pieceSize.x);
                        int piecesY = Mathf.CeilToInt((worldPosTo.y - worldPosFrom.y) / pieceSize.y);

                        blur.SetFloat("_BlurX", Settings.PuzzleSettings.BlurMaskSize / piecesX);
                        blur.SetFloat("_BlurY", Settings.PuzzleSettings.BlurMaskSize / piecesY);
                        RenderTexture temp = RenderTexture.GetTemporary(mask.descriptor);
                        Graphics.Blit(mask, temp, blur);
                        Graphics.Blit(temp, mask);
                        RenderTexture.ReleaseTemporary(temp);
                    }

                    masks.Add(mask);

                    // Create new material for each mask texture.
                    Material material = new Material(puzzlePieceShader);
                    // Update texture to renderTexture.
                    material.SetTexture("_MaskTex", mask);
                    // Unity 6: [PerRendererData] on _MainTex is not reliably set via RawImage.texture
                    // when a custom material is assigned. Set it explicitly on the material instead.
                    material.SetTexture("_MainTex", PuzzleTexture);

                    Vector2 worldSize = worldPosTo - worldPosFrom;
                    // Update material outline value.
                    material.SetVector("_EffectSize", new Vector4(pieceSize.x / worldSize.x * highlightSize, pieceSize.y / worldSize.y * highlightSize));
                    material.SetVector("_EffectStrength", highlightStrength);

                    puzzlePieceMaterials.Add(material);
                }
            }
            Destroy(mesh);
            Destroy(blur);

            RenderTexture.active = prev;
        }

        private void ViewRectUpdated()
        {
            // Board rect.
            Vector3[] corners = new Vector3[4];
            RectTransform.GetWorldCorners(corners);
            PuzzleBoardWorldRect = new Rect(corners[0].x, corners[0].y, (corners[2] - corners[0]).x, (corners[2] - corners[0]).y);

            if (PuzzleData == null)
            {
                return;
            }
            PuzzlePieceWorldSize = PuzzleBoardWorldRect.size.x / PuzzleData.columns;
            HalfPuzzlePieceWorldSize = Vector2.one * PuzzlePieceWorldSize * .5f;
        }

        /// <summary>
        /// Puzzle creation routine.
        /// </summary>
        /// <returns></returns>
        private IEnumerator JobToPuzzleRoutine()
        {
            while (!jobHandle.IsCompleted)
            {
                yield return null;
            }

            // Only continue after mask texture creation job is complete.
            jobHandle.Complete();

            GenerateMaskFromMesh();

            // Generate and enable correct amount of puzzle pieces.
            GeneratePuzzlePieces();

            // Randomize if 'shuffle' is enabled.
            if (ShuffleOnInitialized)
            {
                PuzzlePieces = PuzzlePieces.OrderBy(piece => UnityEngine.Random.value).ToList();
            }

            // Apply saved game data if any suplied for active puzzle data.
            if (savedGameData != null)
            {
                ApplySavedGameData();
            }

            // Only rotate pieces on a piecesPanel(UI) if rotation is enabled.
            if (PiecesRotationEnabled)
            {
                foreach (PuzzlePiece piece in PuzzlePieces.Where(piece => piece.State == PuzzlePieceState.None))
                {
                    piece.RotateTo((PuzzlePieceDirection)UnityEngine.Random.Range(0, 4));
                }
            }

            // Clear job data.
            textureToPuzzleJob.Dispose();

            OnPuzzleInitialized?.Invoke(initialized);
            isActive = true;
            initialized = true;

            puzzleInitTimer.Stop();
            if (debugInitTime)
            {
                Debug.Log($"Generated puzzle texture from data in {puzzleInitTimer.ElapsedMilliseconds} ms");
            }
        }

        // Plays animations in toCheck puzzle pieces in a circle pattern.
        private IEnumerator CirclePiecesAnimationRoutine(
            Vector2 atLocation,
            List<PuzzlePiece> toCheck,
            PuzzlePieceAnimationType puzzlePieceAnimationType,
            float maxRadius = 9f,
            float speedMultiplier = 10f)
        {
            maxRadius *= PuzzlePieceRectSize;
            speedMultiplier *= PuzzlePieceRectSize;

            float radius = 0f;
            while (radius < maxRadius)
            {
                toCheck.RemoveAll(piece => {
                    if (Vector2.Distance(atLocation, piece.RectTransform.localPosition) < radius)
                    {
                        foreach (PuzzlePieceAnimation animation in piece
                            .GetComponents<PuzzlePieceAnimation>()
                            .Where(entry => entry.PuzzlePieceAnimationType == puzzlePieceAnimationType))
                        {
                            animation.PlayAnimations();
                        }

                        return true;
                    }

                    return false;
                });

                radius += Time.deltaTime * speedMultiplier;
                yield return null;
            }
        }

        private struct MaskMaterialHelper
        {
            public Vector2Int maxPiecesPerTexture;
            public Vector2Int maxTextures;
            public Vector2Int totalTextureSize;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
    using Game.Player;

    [RequireComponent(typeof(Tilemap))]
    public class TreeFader : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.45f;
        [SerializeField] private float fadeSpeed = 4f;

        private readonly Dictionary<Vector3Int, Rect> trees = new();
        private readonly Dictionary<Vector3Int, float> alphas = new();
        private readonly HashSet<Vector3Int> covering = new();
        private readonly List<Vector3Int> cells = new();
        private Tilemap tilemap;
        private Player player;
        private int searchX;
        private int searchY;

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
            player = GameManager.Instance.Player;
        }

        private void Start() => CollectTrees();

        private void LateUpdate()
        {
            covering.Clear();
            MarkCovering(player.transform.position);
            foreach (Vector3Int cell in covering)
            {
                if (!alphas.ContainsKey(cell))
                    alphas[cell] = 1f;
            }
            UpdateAlphas(Time.deltaTime);
        }

        private void CollectTrees()
        {
            Vector2 cellSize = tilemap.layoutGrid.cellSize;
            float maxHalfWidth = 0f;
            float maxHeight = 0f;
            BoundsInt area = tilemap.cellBounds;
            foreach (Vector3Int cell in area.allPositionsWithin)
            {
                Sprite sprite = tilemap.GetSprite(cell);
                if (sprite == null)
                    continue;

                Vector2 center = tilemap.GetCellCenterWorld(cell);
                Bounds bounds = sprite.bounds;
                trees[cell] = new Rect(center.x + bounds.min.x, center.y, bounds.size.x, bounds.max.y);
                tilemap.RemoveTileFlags(cell, TileFlags.LockColor);
                maxHalfWidth = Mathf.Max(maxHalfWidth, bounds.extents.x);
                maxHeight = Mathf.Max(maxHeight, bounds.max.y);
            }
            searchX = Mathf.CeilToInt(maxHalfWidth / cellSize.x);
            searchY = Mathf.CeilToInt(maxHeight / cellSize.y);
        }

        private void MarkCovering(Vector2 position)
        {
            Vector3Int origin = tilemap.WorldToCell(position);
            for (int dy = 0; dy <= searchY; dy++)
            {
                for (int dx = -searchX; dx <= searchX; dx++)
                {
                    Vector3Int cell = new(origin.x + dx, origin.y - dy, origin.z);
                    if (trees.TryGetValue(cell, out Rect canopy) && canopy.Contains(position))
                        covering.Add(cell);
                }
            }
        }

        private void UpdateAlphas(float deltaTime)
        {
            float step = fadeSpeed * deltaTime;
            cells.Clear();
            cells.AddRange(alphas.Keys);
            for (int i = 0; i < cells.Count; i++)
            {
                Vector3Int cell = cells[i];
                float target = covering.Contains(cell) ? fadedAlpha : 1f;
                float alpha = Mathf.MoveTowards(alphas[cell], target, step);
                if (alpha != alphas[cell])
                    tilemap.SetColor(cell, new Color(1f, 1f, 1f, alpha));
                if (alpha >= 1f)
                    alphas.Remove(cell);
                else
                    alphas[cell] = alpha;
            }
        }
    }
}

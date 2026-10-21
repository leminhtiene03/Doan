using UnityEngine;

namespace Game.Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MapBounds : MonoBehaviour
    {
        [SerializeField] private float wallThickness = 2f;
        [SerializeField] private float edgePadding = 1f;
        [SerializeField] private float obstacleCheckRadius = 0.5f;

        private readonly Collider2D[] obstacleBuffer = new Collider2D[1];
        private ContactFilter2D obstacleFilter;
        private Rect area;

        public Vector2 Center => area.center;

        private void Awake()
        {
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            Bounds bounds = box.bounds;
            area = new Rect(bounds.min, bounds.size);
            obstacleFilter = new ContactFilter2D { useTriggers = false };
            obstacleFilter.SetLayerMask(1 << GameLayers.Obstacle);
            CreateWalls();
        }

        public bool IsInside(Vector2 point)
            => point.x >= area.xMin + edgePadding && point.x <= area.xMax - edgePadding
            && point.y >= area.yMin + edgePadding && point.y <= area.yMax - edgePadding;

        public Vector2 ClampInside(Vector2 point) => new(
            Mathf.Clamp(point.x, area.xMin + edgePadding, area.xMax - edgePadding),
            Mathf.Clamp(point.y, area.yMin + edgePadding, area.yMax - edgePadding));

        public bool IsFree(Vector2 point)
            => IsInside(point) && Physics2D.OverlapCircle(point, obstacleCheckRadius, obstacleFilter, obstacleBuffer) == 0;

        private void CreateWalls()
        {
            float halfThickness = wallThickness * 0.5f;
            float spanX = area.width + wallThickness * 2f;
            float spanY = area.height + wallThickness * 2f;
            CreateWall("WallTop", new Vector2(area.center.x, area.yMax + halfThickness), new Vector2(spanX, wallThickness));
            CreateWall("WallBottom", new Vector2(area.center.x, area.yMin - halfThickness), new Vector2(spanX, wallThickness));
            CreateWall("WallLeft", new Vector2(area.xMin - halfThickness, area.center.y), new Vector2(wallThickness, spanY));
            CreateWall("WallRight", new Vector2(area.xMax + halfThickness, area.center.y), new Vector2(wallThickness, spanY));
        }

        private void CreateWall(string wallName, Vector2 position, Vector2 size)
        {
            GameObject wall = new(wallName) { layer = GameLayers.Obstacle };
            wall.transform.SetParent(transform, false);
            wall.transform.position = position;
            wall.AddComponent<BoxCollider2D>().size = size;
        }

        private void OnDrawGizmos()
        {
            if (!TryGetComponent(out BoxCollider2D box))
                return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
        }
    }
}

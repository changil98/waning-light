using NavMeshPlus.Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;

public class ProceduralRoomGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap floorMap;
    [SerializeField] private Tilemap wallMap;
    [SerializeField] private Tilemap backgroundMap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase backgroundTile;

    [Header("맵 크기")]
    [SerializeField] private int mapWidth = 80;
    [SerializeField] private int mapHeight = 60;
    [SerializeField] private int minSplitSize = 16;         // 분할 중단 기준

    [Header("배치")]
    [SerializeField] private RoomData roomData;
    [SerializeField] private GameObject playerPrefab;       // null이면 기존 오브젝트 이동
    [SerializeField] private Transform playerTransform;     // playerPrefab null일 때 사용
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject resourcePrefab;     // ResourceItem 붙어있는 프리팹
    [SerializeField] private GameObject exitPrefab;         // ExitTrigger 붙어있는 프리팹
    [SerializeField] private int enemyCount = 2;
    [SerializeField] private int minDistFromDoor = 3;       // 자원 배치 최소 거리

    private Transform _spawnedPlayer;
    private int _playerRoomIdx;
    private int _exitRoomIdx;
    private BSPNode _root;
    private readonly List<RectInt> _rooms = new();
    private readonly List<Vector2Int> _doorPositions = new();

    private async Awaitable Start()
    {
        Generate();                                             // 타일 + 플레이어 + 출구 + 자원
        await Awaitable.NextFrameAsync();                       // TilemapCollider2D 갱신 대기

        var surface = FindFirstObjectByType<NavMeshSurface>();
        if (surface != null)
        {
            var op = surface.BuildNavMeshAsync();
            float navTimeout = 10f;                             // NavMesh 빌드 제한 시간
            while (!op.isDone && navTimeout > 0f)
            {
                navTimeout -= Time.deltaTime;
                await Awaitable.NextFrameAsync();
            }

            if (navTimeout <= 0f)
            {
                Debug.LogError("[Generator] NavMesh 빌드 타임아웃. 적 스폰 생략.");
                EventBus.Publish(new LevelGeneratedEvent());
                return;
            }
        }

        SpawnEnemies();                                         // NavMesh 완료 후 적 생성
        EventBus.Publish(new LevelGeneratedEvent());            // 게임 시작 이벤트
    }

    private void Generate()
    {
        floorMap.ClearAllTiles();
        wallMap.ClearAllTiles();
        backgroundMap.ClearAllTiles();
        _rooms.Clear();
        _doorPositions.Clear();

        _root = new BSPNode(new RectInt(0, 0, mapWidth, mapHeight));
        Split(_root);
        CollectRooms(_root);
        PaintBackground();
        PaintTiles();
        ConnectRooms(_root);
        PaintWalls();
        PlaceObjects();
    }

    private void Split(BSPNode node)
    {
        bool tooNarrow = node.bounds.width < minSplitSize * 2;
        bool tooShort = node.bounds.height < minSplitSize * 2;
        if (tooNarrow && tooShort) { CreateRoom(node); return; }

        bool splitH = tooNarrow || (!tooShort && Random.value > 0.5f);

        if (splitH)
        {
            int s = Random.Range(minSplitSize, node.bounds.height - minSplitSize);
            node.left  = new BSPNode(new RectInt(node.bounds.x, node.bounds.y,     node.bounds.width, s));
            node.right = new BSPNode(new RectInt(node.bounds.x, node.bounds.y + s, node.bounds.width, node.bounds.height - s));
        }
        else
        {
            int s = Random.Range(minSplitSize, node.bounds.width - minSplitSize);
            node.left  = new BSPNode(new RectInt(node.bounds.x,     node.bounds.y, s,                     node.bounds.height));
            node.right = new BSPNode(new RectInt(node.bounds.x + s, node.bounds.y, node.bounds.width - s, node.bounds.height));
        }

        Split(node.left);
        Split(node.right);
    }

    private void CreateRoom(BSPNode node)
    {
        int w = Random.Range(roomData.minWidth,  Mathf.Min(roomData.maxWidth,  node.bounds.width  - 2) + 1);
        int h = Random.Range(roomData.minHeight, Mathf.Min(roomData.maxHeight, node.bounds.height - 2) + 1);
        int x = Random.Range(node.bounds.x + 1, node.bounds.xMax - w - 1);
        int y = Random.Range(node.bounds.y + 1, node.bounds.yMax - h - 1);
        node.room = new RectInt(x, y, w, h);
    }

    private void CollectRooms(BSPNode node)
    {
        if (node == null) return;
        if (node.IsLeaf) { _rooms.Add(node.room); return; }
        CollectRooms(node.left);
        CollectRooms(node.right);
    }

    private void PaintBackground()
    {
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                backgroundMap.SetTile(new Vector3Int(x, y, 0), backgroundTile);
    }

    private void PaintTiles()
    {
        foreach (var room in _rooms)
            for (int x = room.x; x < room.xMax; x++)
                for (int y = room.y; y < room.yMax; y++)
                    floorMap.SetTile(new Vector3Int(x, y, 0), floorTile);
    }

    // 재귀로 형제 방을 연결하면서 연결점을 _doorPositions에 기록
    // 반환값: 이 서브트리의 대표 중심정 (부모가 통로 연결에 사용)
    private Vector2Int ConnectRooms(BSPNode node)
    {
        if (node.IsLeaf)
            return new Vector2Int(node.room.x + node.room.width / 2, node.room.y + node.room.height / 2);

        Vector2Int lc = ConnectRooms(node.left);
        Vector2Int rc = ConnectRooms(node.right);

        // L자 통로: 수평 -> 수직
        DrawHCorridor(lc.x, rc.x, lc.y);
        DrawVCorridor(lc.y, rc.y, rc.x);

        _doorPositions.Add(lc);
        _doorPositions.Add(rc);

        return lc;
    }

    private void DrawHCorridor(int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            floorMap.SetTile(new Vector3Int(x, y, 0), floorTile);
    }

    private void DrawVCorridor(int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
            floorMap.SetTile(new Vector3Int(x, y, 0), floorTile);
    }

    private void PaintWalls()
    {
        // floor 타일 주변 8방향에 빈 칸이 있으면 벽 세팅
        int[] dx = { 0, 0, 1, -1, 1, 1, -1, -1 };
        int[] dy = { 1, -1, 0, 0, 1, -1, 1, -1 };

        var scan = new BoundsInt(-1, -1, 0, mapWidth + 2, mapHeight + 2, 1);
        foreach (var pos in scan.allPositionsWithin)
        {
            if (floorMap.GetTile(pos) != null) continue;

            for (int i = 0; i < 8; i++)
            {
                if (floorMap.GetTile(new Vector3Int(pos.x + dx[i], pos.y + dy[i], 0)) != null)
                {
                    wallMap.SetTile(pos, wallTile);
                    break;
                }
            }
        }
    }

    private void PlaceObjects()
    {
        var shuffled = new List<RectInt>(_rooms);
        Shuffle(shuffled);

        // 플레이어 (첫 번째 방) - Transform 저장
        Vector3 playerPos = RoomCenter(shuffled[0]);
        _playerRoomIdx = _rooms.IndexOf(shuffled[0]);
        if (playerPrefab != null)
            _spawnedPlayer = Instantiate(playerPrefab, playerPos, Quaternion.identity).transform;
        else if (playerTransform != null)
        {
            playerTransform.position = playerPos;
            _spawnedPlayer = playerTransform;
        }

        if (_spawnedPlayer == null)
        {
            Debug.LogError("[Generator] playerPrefab과 playerTransform이 둘 다 없습니다. 인스펙터를 확인하세요.");
            return;
        }

        var vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null) vcam.Follow = _spawnedPlayer;

        // 출구 (마지막 방)
        Instantiate(exitPrefab, RoomCenter(shuffled[^1]), Quaternion.identity);
        _exitRoomIdx = _rooms.IndexOf(shuffled[^1]);

        // 자원 배치 (중간 방들, 문에서 거리 조건 통과해야 배치)
        int resourcesPlaced = 0;
        for (int i = 1; i < shuffled.Count - 1 && resourcesPlaced < GameManager.RequiredResources; i++)
        {
            Vector2Int? spot = FindResourceSpot(shuffled[i]);
            if (spot == null) continue;
            Instantiate(resourcePrefab, new Vector3(spot.Value.x + 0.5f, spot.Value.y + 0.5f, 0), Quaternion.identity);
            resourcesPlaced++;
        }

        if (resourcesPlaced < GameManager.RequiredResources)
            Debug.LogWarning($"[Generator] 리소스 {resourcesPlaced}/{GameManager.RequiredResources}개만 배치됨.");
    }

    private void SpawnEnemies()
    {
        int enemiesPlaced = 0;
        var indices = Enumerable.Range(0, _rooms.Count).ToList();
        Shuffle(indices);

        foreach (var idx in indices)
        {
            if (enemiesPlaced >= enemyCount) break;
            if (idx == _playerRoomIdx || idx == _exitRoomIdx) continue;

            var room = _rooms[idx];
            var enemy = Instantiate(enemyPrefab, RoomCenter(room), Quaternion.identity);
            var stateMachine = enemy.GetComponent<EnemyStateMachine>();
            var waypoints = new GameObject { name = $"Waypoints_{idx}" };
            waypoints.transform.SetParent(transform);
            stateMachine?.SetWaypoints(CreateWaypoints(room, waypoints.transform));
            if (_spawnedPlayer != null)            
                stateMachine?.SetPlayer(_spawnedPlayer);            
            enemiesPlaced++;
        }
    }

    private Vector3 RoomCenter(RectInt room) =>
        new Vector3(room.x + room.width / 2f, room.y + room.height / 2f, 0);

    private Vector2Int? FindResourceSpot(RectInt room)
    {
        var candidates = new List<Vector2Int>();
        for (int x = room.x + 1; x < room.xMax - 1; x++)
            for (int y = room.y + 1; y < room.yMax - 1; y++)
                candidates.Add(new Vector2Int(x, y));

        Shuffle(candidates);
        foreach (var pos in candidates)
            if (IsFarFromDoors(pos, minDistFromDoor)) return pos;

        return null;
    }

    // 방 내부 4개 꼭짓점 위치에 waypoint GameObject 생성
    private Transform[] CreateWaypoints(RectInt room, Transform parent)
    {
        Vector2Int[] corners =
        {
            new(room.x + 1,    room.y + 1),
            new(room.xMax - 2, room.y + 1),
            new(room.xMax - 2, room.yMax -2),
            new(room.x + 1,    room.yMax - 2),
        };

        var result = new Transform[corners.Length];
        for (int i = 0; i < corners.Length; i++)
        {
            var go = new GameObject($"Waypoint_{i}");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(corners[i].x + 0.5f, corners[i].y + 0.5f, 0);
            result[i] = go.transform;
        }
        return result;
    }

    private bool IsFarFromDoors(Vector2Int pos, int minDist)
    {
        foreach (var door in _doorPositions)
            if (Mathf.Abs(pos.x - door.x) + Mathf.Abs(pos.y - door.y) < minDist)
                return false;
        return true;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private class BSPNode
    {
        public RectInt bounds;  // 이 노드가 차지하는 전체 영역
        public RectInt room;    // 리프 노드만 사용 - 실제 방
        public BSPNode left, right;
        public bool IsLeaf => left == null && right == null;

        public BSPNode(RectInt bounds) { this.bounds = bounds; }
    }
}
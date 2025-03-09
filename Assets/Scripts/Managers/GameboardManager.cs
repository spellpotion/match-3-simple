using System.Collections.Generic;
using UnityEngine;

namespace Match3Simple.Managers
{
    public class  GameboardManager : ManagerBase<GameboardManager>
    {
        private static readonly Vector2 gameboardSize = new(10f, 10f);
        private static readonly float positionOffsetZ = .5f;

        public static Vector2Int TileCount => Instance.tileCount;

        public static IReadOnlyList<IReadOnlyList<Tile>> Gameboard => Instance.gameboard;

        public static void Initialize()
            => Instance.InitializeGameboard();

        public static void RemoveTile(Tile tile)
            => Instance.RemoveTile_Implementation(tile);

        public static bool RemoveMatchingTiles()
            => Instance.RemoveMatchingTiles_Implementation();

        public static bool RepositionTiles()
            => Instance.RepositionTiles_Implementation();

        [SerializeField] private Vector2Int tileCount = new(10, 10);
        [Space]
        [SerializeField] private Tile[] tilePrefabs;

        private Vector2 originPosition;
        private Vector2 tileSize;
        private float tileScaleValue;

        private Vector3 TileScale => new(tileScaleValue, tileScaleValue, tileScaleValue);

        private List<Tile>[] gameboard; // collumns of rows

        protected void Awake()
        {
            originPosition = -gameboardSize * .5f;
            tileSize = gameboardSize / tileCount;
            tileScaleValue = Mathf.Min(tileSize.x, tileSize.y);
        }

        private void InitializeGameboard()
        {
            gameboard = new List<Tile>[tileCount.x];

            for (var x = 0; x < tileCount.x; x++)
            {
                gameboard[x] = new List<Tile>();

                for (var y = 0; y < tileCount.y; y++)
                {
                    gameboard[x].Add(InitializeTile(x, y));
                }
            }
        }

        Tile GetRandomTilePrefab() => tilePrefabs[Random.Range(0, tilePrefabs.Length)];

        private Tile InitializeTile(int x, int y)
        {
            var original = GetRandomTilePrefab();

            while (!MatchInX() || !MatchInY())
            {
                original = GetRandomTilePrefab();
            }

            var position = CalculatePosition(x, y);
            var rotation = Quaternion.identity;
            var instance = Instantiate(original, position, rotation);

            instance.transform.localScale = TileScale;

            return instance;

            bool MatchInX()
            {
                if (x < GameManager.MatchLength - 1) return true;

                for (var i = 1; i < GameManager.MatchLength; i++)
                {
                    if (!gameboard[x - i][y].name.Contains(original.name)) return true;
                }

                return false;
            }

            bool MatchInY()
            {
                if (y < GameManager.MatchLength - 1) return true;

                for (var i = 1; i < GameManager.MatchLength; i++)
                {
                    if (!gameboard[x][y - i].name.Contains(original.name)) return true;
                }

                return false;
            }
        }

        private Vector3 CalculatePosition(int col, int row)
        {
            return new Vector3(
                originPosition.x + tileSize.x * col + tileSize.x * .5f,
                originPosition.y + tileSize.y * row + tileSize.y * .5f,
                -tileScaleValue * .5f + positionOffsetZ
                );
        }

        private void RemoveTile_Implementation(Tile tile)
        {
            for (var col = 0; col < tileCount.x; col++)
            {
                if (gameboard[col].Remove(tile))
                {
                    Destroy(tile.gameObject);

                    return;
                }
            }
        }

        private bool RepositionTiles_Implementation()
        {
            var changed = false;

            for (var col = 0; col < tileCount.x; col++)
            {
                for (var row = 0; row < gameboard[col].Count; row++)
                {
                    if (gameboard[col][row].SetPosition(CalculatePosition(col, row)))
                    {
                        changed = true;
                    }
                }
            }

            return changed;
        }

        private bool RemoveMatchingTiles_Implementation()
        {
            var anyDestroyed = false;

            foreach (var col in gameboard)
            {
                anyDestroyed |= col.RemoveAll(tile => tile.IsMatch && DestroyTile(tile)) > 0;
            }

            return anyDestroyed;

            static bool DestroyTile(Tile tile)
            {
                Destroy(tile.gameObject);
                return true;
            }
        }

        private void OnValidate()
        {
            Debug.Assert(tileCount.x > 2);
            Debug.Assert(tileCount.y > 2);
            Debug.Assert(tilePrefabs.Length > 1);
        }
    }
}
using System.Collections;
using UnityEngine;

namespace Match3Simple.Managers
{

    public class GameManager : ManagerBase<GameManager>
    {
        public static int MatchLength => Instance.matchLength;
        [SerializeField] private int matchLength = 3;

        private Coroutine findMatchWait;

        protected override void OnEnable()
        {
            base.OnEnable();

            Tile.OnTileSelected += OnTileSelected;
        }

        protected override void OnDisable()
        {
            Tile.OnTileSelected -= OnTileSelected;

            base.OnDisable();
        }

        void Start()
        {
            GameboardManager.Initialize();
        }

        private void OnTileSelected(Tile tile)
        {
            if (findMatchWait != null) return;

            GameboardManager.RemoveTile(tile);

            UpdateGameboard();
        }

        private void UpdateGameboard()
        {
            if (!GameboardManager.RepositionTiles()) return;

            findMatchWait = StartCoroutine(FindMatchWait());
        }

        private void FindMatch()
        {
            var gameboard = GameboardManager.Gameboard;

            FindMatchX();
            FindMatchY();

            if (!GameboardManager.RemoveMatchingTiles()) return;

            UpdateGameboard();

            void FindMatchX()
            {
                for (var x = 0; x < gameboard.Count; x++)
                {
                    var column = gameboard[x];
                    if (column.Count < matchLength) continue;

                    var count = 1;

                    for (var y = 1; y < column.Count; y++)
                    {
                        if (column[y].name == column[y - 1].name)
                        {
                            count++;
                        }
                        else
                        {
                            if (count >= matchLength) SetMatch(x, y - 1);

                            count = 1;
                        }
                    }

                    if (count >= matchLength) SetMatch(x, column.Count - 1);
                }
            }

            void FindMatchY()
            {
                for (var y = 0; y < GameboardManager.TileCount.y; y++)
                {
                    string nameLast = null;
                    var count = 0;

                    for (var x = 0; x < gameboard.Count; x++)
                    {
                        if (y >= gameboard[x].Count) // no tile gap
                        {
                            if (count >= matchLength) SetMatch(x - 1, y);

                            nameLast = null;
                            count = 0;
                            continue;
                        }

                        var nameCurrent = gameboard[x][y].name;

                        if (nameCurrent == nameLast)
                        {
                            count++;
                        }
                        else
                        {
                            if (count >= matchLength) SetMatch(x - 1, y);

                            nameLast = nameCurrent;
                            count = 1;
                        }
                    }

                    if (count >= matchLength) SetMatch(gameboard.Count - 1, y);
                }
            }

            void SetMatch(int x, int y, string name = null)
            {
                if (x < 0 || y < 0) return;
                if (x >= gameboard.Count) return;
                if (y >= gameboard[x].Count) return;

                var tile = gameboard[x][y];

                if (tile.IsMatch) return;
                if (name != null && tile.name != name) return;

                tile.IsMatch = true;
                name ??= tile.name;

                SetMatch(x - 1, y, name);
                SetMatch(x, y - 1, name);
                SetMatch(x + 1, y, name);
                SetMatch(x, y + 1, name);
            }
        }

        private IEnumerator FindMatchWait()
        {
            yield return new WaitForSeconds(Tile.durationAnimation);

            findMatchWait = null;

            FindMatch();
        }

        private void OnValidate()
        {
            Debug.Assert(matchLength > 1);
        }
    }
}

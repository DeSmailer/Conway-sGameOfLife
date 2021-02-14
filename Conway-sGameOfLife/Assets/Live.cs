using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Live : MonoBehaviour
{
    [SerializeField] private Tile _life;
    [SerializeField] private Tile _frame;
    [SerializeField] private Tile _justDrawn;
    [SerializeField] private Tilemap _render;

    private static int sizeHorizontal = 100;
    private static int sizeVertical = 100;

    private readonly Cell[,] _cells = new Cell[sizeHorizontal, sizeVertical];

    public float UpdateSpeed = 0.3f;
    private readonly float defaultSpeed = 0.3f;

    private bool isPlay = true;
    private void Awake()
    {
        Seed();
        InvokeRepeating(nameof(Tick), 0, UpdateSpeed);
    }
    private void Start()
    {
        for (int x = -1; x <= sizeHorizontal; x++)
        {
            _render.SetTile(new Vector3Int(x, -1, 0), _frame);
            _render.SetTile(new Vector3Int(x, sizeHorizontal+1, 0), _frame);
        }
        for (int x = -1; x <= sizeVertical; x++)
        {
            _render.SetTile(new Vector3Int(-1, x, 0), _frame);
            _render.SetTile(new Vector3Int(sizeVertical + 1, x, 0), _frame);
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DrowClick();
        }
    }
    private void Seed()
    {
        Cell.Live.Tile = _life;
        Cell.Died.Tile = null;

        for (int x = 0; x < _cells.GetLength(0); x++)
        {
            for (int y = 0; y < _cells.GetLength(1); y++)
            {
                _cells[x, y] = Cell.Died;
            }
        }
        _cells[20, 20] = Cell.Live;
        _cells[21, 20] = Cell.Live;
        _cells[19, 20] = Cell.Live;
        _cells[20, 21] = Cell.Live;
    }
    private void Tick()
    {
        Render(_render);
        Simulate();
    }
    private void Render(Tilemap render)
    {
        for (int x = 0; x < _cells.GetLength(0); x++)
        {
            for (int y = 0; y < _cells.GetLength(1); y++)
            {
                render.SetTile(new Vector3Int(x, y, 0), _cells[x, y].Tile);
            }
        }
    }
    private void Simulate()
    {
        int[,] parents = new int[sizeHorizontal, sizeVertical];

        for (int x = 0; x < _cells.GetLength(0); x++)
        {
            for (int y = 0; y < _cells.GetLength(1); y++)
            {

                _cells[x, y].Affect(parents, x, y);
            }
        }

        for (int x = 0; x < _cells.GetLength(0); x++)
        {
            for (int y = 0; y < _cells.GetLength(1); y++)
            {
                _cells[x, y] = _cells[x, y].Transform(parents[x, y]);

            }
        }
    }

    public void DrowClick()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Cell.Live.Tile = _life;
        int x = (int)Math.Round(pos.x, 0);
        int y = (int)Math.Round(pos.y, 0);
        _cells[x, y] = Cell.Live;
        _render.SetTile(new Vector3Int(x, y, 0), _justDrawn);
    }

    public void Pause()
    {
        if (isPlay)
        {
            CancelInvoke(nameof(Tick));
            isPlay = false;
        }
    }
    public void Continue()
    {
        if (!isPlay)
        {
            InvokeRepeating(nameof(Tick), 0, UpdateSpeed);
            isPlay = true;
        }
    }

}
class Cell
{
    private readonly int _influence;
    private readonly int[] _transitions;

    public static Cell Live = new Cell(1, new int[] { 0, 0, 1, 1, 0, 0, 0, 0, 0 });
    public static Cell Died = new Cell(0, new int[] { 0, 0, 0, 1, 0, 0, 0, 0, 0 });

    public Tile Tile { get; set; }

    private static Cell[] TransitionTarget => TransitionTargetsEnumerable.ToArray();

    private static IEnumerable<Cell> TransitionTargetsEnumerable
    {
        get
        {
            yield return Died;
            yield return Live;
        }
    }
    public Cell(int influencei, params int[] transitions)
    {
        _influence = influencei;
        _transitions = transitions;
    }
    public void Affect(int[,] parents, int x, int y)
    {
        //for (int i = -1; i < 2; i++)
        //{
        //    for (int j = -1; j < 2; j++)
        //    {
        //        if (i != 0 && j != 0 && x + i < parents.GetLength(0)-1 && y + j < parents.GetLength(1)-1 && x + i > 0 && y + j > 0)
        //        {
        //            parents[x + i, y + j] += _influence;
        //        }
        //    }
        //}
        try
        {
            parents[x + 1, y + 1] += _influence;
            parents[x - 1, y - 1] += _influence;
            parents[x + 1, y - 1] += _influence;
            parents[x - 1, y + 1] += _influence;
            parents[x + 1, y] += _influence;
            parents[x, y + 1] += _influence;
            parents[x - 1, y] += _influence;
            parents[x, y - 1] += _influence;
        }
        catch (Exception)
        {
        }

    }
    public Cell Transform(int parents)
    {
        return TransitionTarget[_transitions[parents]];
    }
}
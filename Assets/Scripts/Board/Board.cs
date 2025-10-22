using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    private int boardSizeX;
    private int boardSizeY;
    private Cell[,] m_cells;
    private Transform m_root;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;

        m_cells = new Cell[boardSizeX, boardSizeY];

        CreateBoard();
    }

    private void CreateBoard()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }
        }
    }

    public void ClearBoard()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();

                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }
    }

    internal bool IsBoardEmpty()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (m_cells[x, y].Item != null) return false;
            }
        }
        return true;
    }

    internal void FillBoard()
    {
        int totalCells = boardSizeX * boardSizeY;

        List<NormalItem.eNormalType> types =
            Enum.GetValues(typeof(NormalItem.eNormalType))
                .Cast<NormalItem.eNormalType>()
                .ToList();

        // Tao pool
        List<NormalItem.eNormalType> pool = new List<NormalItem.eNormalType>();
        int idx = 0;
        while (pool.Count < totalCells)
        {
            var t = types[idx % types.Count];
            pool.Add(t);
            pool.Add(t);
            pool.Add(t);
            idx++;
        }

        // Loai bo cac phan tu thua
        if (pool.Count > totalCells)
            pool.RemoveRange(totalCells, pool.Count - totalCells);

        Utils.ShuffleList(pool);

        int k = 0;
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];

                var item = new NormalItem();
                item.SetType(pool[k++]);
                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(false);
            }
        }
    }

    public List<Cell> GetNotEmptyCells()
    {
        var list = new List<Cell>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                var c = m_cells[x, y];
                if (c != null && !c.IsEmpty) list.Add(c);
            }
        }
        return list;
    }
}

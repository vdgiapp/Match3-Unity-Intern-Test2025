using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellQueue
{
    private int m_size;
    private Cell[] m_cells;
    private Transform m_root;

    public CellQueue(Transform transform, GameSettings gameSettings)
    {
        m_size = gameSettings.LevelQueueSize;
        m_root = transform;
        m_cells = new Cell[m_size];
        CreateCellBackground();
    }

    private void CreateCellBackground()
    {
        Vector3 origin = new Vector3(-m_size * 0.5f + 0.5f, -4f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int i = 0; i < m_size; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.GetComponent<SpriteRenderer>().color = Color.green;
            go.name = "cellQueueBackground";

            go.transform.position = origin + new Vector3(i, 0f, 0f);
            go.transform.SetParent(m_root);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i, 0);

            m_cells[i] = cell;
        }
    }

    public bool IsQueueFull()
    {
        for (int i = 0; i < m_size; i++)
        {
            var c = m_cells[i];
            if (c == null) continue;
            if (c.IsEmpty) return false;
        }
        return true;
    }

    public int CountOccupied()
    {
        int cnt = 0;
        for (int i = 0; i < m_size; i++)
        {
            var c = m_cells[i];
            if (c == null) continue;
            if (!c.IsEmpty) cnt++;
        }
        return cnt;
    }

    public int CountType(NormalItem.eNormalType type)
    {
        int cnt = 0;
        for (int i = 0; i < m_size; i++)
        {
            var c = m_cells[i];
            if (c == null || c.IsEmpty) continue;
            var ni = c.Item as NormalItem;
            if (ni != null && ni.ItemType == type) cnt++;
        }
        return cnt;
    }

    public List<NormalItem.eNormalType> GetCurrentItemTypes()
    {
        List<NormalItem.eNormalType> result = new List<NormalItem.eNormalType>();

        foreach (var cell in m_cells)
        {
            if (cell == null) continue;
            if (cell.IsEmpty) continue;

            if (cell.Item is NormalItem ni)
            {
                result.Add(ni.ItemType);
            }
        }

        return result;
    }

    public bool IsQueueNotFull()
    {
        for (int i = 0; i < m_size; i++)
        {
            var c = m_cells[i];
            if (c == null) continue;
            if (c.IsEmpty) return true;
        }
        return false;
    }

    public bool AddCell(Cell cell)
    {
        if (cell == null || cell.IsEmpty) return false;

        for (int i = 0; i < m_size; i++)
        {
            Cell slot = m_cells[i];
            if (slot == null) continue;
            if (slot.IsEmpty)
            {
                Item item = cell.Item;
                Vector3 oldPos = Vector3.zero;

                // Luu vi tri View de thuc hien animation
                if (item.View != null) oldPos = item.View.position;
                else oldPos = cell.transform.position;

                // Luu vi tri origin de khi bam vao se tra ve vi tri cu
                item.OriginCell = cell;

                cell.Clear();

                slot.Assign(item);
                item.SetCell(slot);

                // Set View, root va vi tri de thuc hien animation
                item.SetView();
                item.SetViewRoot(m_root);
                item.SetViewPosition(oldPos);
                item.AnimationMoveToPosition();

                ClearTriplets();
                return true;
            }
        }
        return false;
    }

    public bool ReturnCell(Cell bottomSlot)
    {
        if (bottomSlot == null) return false;
        bool isBottom = false;

        for (int i = 0; i < m_size; i++)
        {
            if (m_cells[i] == bottomSlot)
            {
                isBottom = true;
                break;
            }
        }
        if (!isBottom) return false;
        if (bottomSlot.IsEmpty) return false;

        Item item = bottomSlot.Item;
        if (item == null || item.OriginCell == null) return false;

        Cell origin = item.OriginCell;
        if (!origin.IsEmpty) return false; // Khong the tra ve vi tri cu neu vi tri do da bi chiem

        // Xoa khoi Queue va quay tro ve vi tri cu (OriginCell)
        bottomSlot.Clear();

        origin.Assign(item);
        item.SetCell(origin);

        // Set View, root va vi tri de thuc hien animation
        item.SetView();
        item.SetViewRoot(origin.transform.parent ?? m_root);
        item.SetViewPosition(bottomSlot.transform.position);
        item.AnimationMoveToPosition();

        return true;
    }

    public void ClearTriplets()
    {
        Dictionary<NormalItem.eNormalType, int> typeCount = new Dictionary<NormalItem.eNormalType, int>();

        for (int i = 0; i < m_size; i++)
        {
            Cell cell = m_cells[i];
            if (cell == null || cell.IsEmpty) continue;

            NormalItem item = cell.Item as NormalItem;
            if (item == null) continue;

            if (!typeCount.ContainsKey(item.ItemType)) typeCount[item.ItemType] = 0;
            typeCount[item.ItemType]++;
        }

        // Chi xoa nhung loai da du 3 cai
        List<NormalItem.eNormalType> clearTypes =
            typeCount.Where(kvp => kvp.Value == 3)
                .Select(kvp => kvp.Key)
                .ToList();

        if (clearTypes.Count == 0) return;

        foreach (NormalItem.eNormalType type in clearTypes)
        {
            for (int i = 0; i < m_size; i++)
            {
                Cell cell = m_cells[i];
                if (cell == null || cell.IsEmpty) continue;

                NormalItem item = cell.Item as NormalItem;
                if (item != null && item.ItemType == type)
                {
                    cell.ExplodeItem();
                    cell.Clear();
                }
            }
        }
    }

    public bool IsCellInQueue(Cell cell)
    {
        if (cell == null) return false;
        for (int i = 0; i < m_size; i++)
        {
            if (m_cells[i] == cell) return true;
        }
        return false;
    }
}
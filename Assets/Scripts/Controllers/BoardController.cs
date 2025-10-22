using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action<int> OnQueueSizeChanged = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;
    private CellQueue m_cellQueue;
    private GameManager m_gameManager;

    private Camera m_cam;
    private Collider2D m_hitCollider;
    private GameSettings m_gameSettings;

    private bool m_gameOver;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;
        m_gameManager.StateChangedAction += OnGameStateChange;
        m_cam = Camera.main;
        m_board = new Board(this.transform, gameSettings);
        m_cellQueue = new CellQueue(this.transform, gameSettings);
        Fill();
    }

    private void Fill()
    {
        m_board.FillBoard();
    }

    internal void Clear()
    {
        m_board.ClearBoard();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                break;
            case GameManager.eStateGame.GAME_WON:
                m_gameOver = true;
                break;
        }
    }

    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                m_hitCollider = hit.collider;
                Cell cell = m_hitCollider.GetComponent<Cell>();
                if (cell == null) return;

                if (m_gameManager.LevelMode == GameManager.eLevelMode.TIMER)
                {
                    // Neu la Queue item thi tra ve vi tri cu
                    if (m_cellQueue.IsCellInQueue(cell))
                    {
                        if (!cell.IsEmpty)
                        {
                            bool returned = m_cellQueue.ReturnCell(cell);
                            if (returned) InvokeQueueSizeChanged();
                        }
                        ResetRayCast();
                        return;
                    }
                }

                // Con neu la o trong Board thi chuyen xuong Queue
                if (cell.IsEmpty) return;
                cell.Item.SetSortingLayerHigher();

                bool added = m_cellQueue.AddCell(cell);
                if (added)
                {
                    InvokeQueueSizeChanged();
                    if (m_board.IsBoardEmpty())
                        m_gameManager.WinGame();
                }
                ResetRayCast();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetRayCast();
        }
    }

    private void ResetRayCast()
    {
        m_hitCollider = null;
    }

    public bool IsBoardEmpty()
    {
        return m_board.IsBoardEmpty();
    }

    public bool IsQueueFull()
    {
        return m_cellQueue.IsQueueFull();
    }

    public void InvokeQueueSizeChanged()
    {
        OnQueueSizeChanged(m_cellQueue.CountOccupied());
    }

    public void PerformAutoLose()
    {
        List<Cell> nonEmpty = m_board.GetNotEmptyCells();
        if (nonEmpty == null || nonEmpty.Count == 0) return;

        List<NormalItem.eNormalType> bottomTypes = m_cellQueue.GetCurrentItemTypes();

        // Chon nhung cai khac nhau
        foreach (Cell c in nonEmpty)
        {
            if (c.Item is NormalItem ni && !bottomTypes.Contains(ni.ItemType))
            {
                m_cellQueue.AddCell(c);
                InvokeQueueSizeChanged();
                return;
            }
        }

        // Neu khong con cai khac nhau thi chon random
        Cell randomPick = nonEmpty[UnityEngine.Random.Range(0, nonEmpty.Count)];
        m_cellQueue.AddCell(randomPick);
        InvokeQueueSizeChanged();
    }

    public void PerformAutoWin()
    {
        List<Cell> nonEmpty = m_board.GetNotEmptyCells();
        if (nonEmpty == null || nonEmpty.Count == 0) return;

        List<NormalItem.eNormalType> bottomTypes = m_cellQueue.GetCurrentItemTypes();

        // Neu chua co loai nao thi chon random
        if (bottomTypes.Count == 0)
        {
            Cell pick = nonEmpty[UnityEngine.Random.Range(0, nonEmpty.Count)];
            m_cellQueue.AddCell(pick);
            InvokeQueueSizeChanged();
            return;
        }

        // Neu co 1 loai thi chon cai cung loai
        foreach (Cell c in nonEmpty)
        {
            if (c.Item is NormalItem ni && bottomTypes.Contains(ni.ItemType))
            {
                m_cellQueue.AddCell(c);
                InvokeQueueSizeChanged();
                return;
            }
        }

        // Khong thi chon random
        Cell randomPick = nonEmpty[UnityEngine.Random.Range(0, nonEmpty.Count)];
        m_cellQueue.AddCell(randomPick);
        InvokeQueueSizeChanged();
    }
}

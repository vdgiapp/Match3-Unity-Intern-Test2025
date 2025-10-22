using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelFullQueue : LevelCondition
{
    private int m_maxQueueSize;
    private int m_queueSize = 0;

    private BoardController m_board;

    public override void Setup(float value, Text txt, BoardController board)
    {
        base.Setup(value, txt);

        m_maxQueueSize = (int)value;

        m_board = board;

        m_board.OnQueueSizeChanged += OnQueueSizeChange;

        UpdateText();
    }

    private void OnQueueSizeChange(int currentSize)
    {
        if (m_conditionCompleted) return;

        m_queueSize = currentSize;

        UpdateText();

        if(m_queueSize >= m_maxQueueSize)
        {
            OnConditionComplete();
        }
    }

    protected override void UpdateText()
    {
        m_txt.text = string.Format("SIZE:\n{0}/{1}", m_queueSize, m_maxQueueSize);
    }

    protected override void OnDestroy()
    {
        if (m_board != null) m_board.OnQueueSizeChanged -= OnQueueSizeChange;

        base.OnDestroy();
    }
}

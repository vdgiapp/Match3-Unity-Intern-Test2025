using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        QUEUE
    }
    private eLevelMode m_levelMode;
    public eLevelMode LevelMode
    {
        get { return m_levelMode; }
        private set { m_levelMode = value; }
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        GAME_WON
    }
    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;
    private BoardController m_boardController;
    private UIMainManager m_uiMenu;
    private LevelCondition m_levelCondition;

    private Coroutine m_autoplayCoroutine;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);

        if (mode == eLevelMode.QUEUE)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelFullQueue>();
            m_levelCondition.Setup(m_gameSettings.LevelQueueSize, m_uiMenu.GetLevelConditionView(), m_boardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelTime, m_uiMenu.GetLevelConditionView(), this);
        }

        m_levelCondition.ConditionCompleteEvent += GameOver;
        LevelMode = mode;
        State = eStateGame.GAME_STARTED;
    }

    public void GameOver()
    {
        StopAutoplay();
        StartCoroutine(WaitBoardController());
    }

    public void WinGame()
    {
        StopAutoplay();
        State = eStateGame.GAME_WON;
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = eStateGame.GAME_OVER;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;

            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }

    // Autoplay
    public void StartAutoplay()
    {
        StopAutoplay();
        m_autoplayCoroutine = StartCoroutine(AutoplayRoutine(false));
    }

    public void StartAutoLose()
    {
        StopAutoplay();
        m_autoplayCoroutine = StartCoroutine(AutoplayRoutine(true));
    }

    public void StopAutoplay()
    {
        if (m_autoplayCoroutine != null)
        {
            StopCoroutine(m_autoplayCoroutine);
            m_autoplayCoroutine = null;
        }
    }

    private IEnumerator AutoplayRoutine(bool goalIsToLose)
    {
        if (m_boardController == null) yield break;

        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (m_boardController == null) yield break;

            if (!goalIsToLose)
            {
                m_boardController.PerformAutoWin();
            }
            else
            {
                m_boardController.PerformAutoLose();
            }

            if (m_boardController.IsBoardEmpty())
            {
                WinGame();
                yield break;
            }

            if (goalIsToLose && m_boardController.IsQueueFull())
            {
                if (LevelMode == eLevelMode.QUEUE)
                {
                    GameOver();
                    yield break;
                }
            }
        }
    }
}

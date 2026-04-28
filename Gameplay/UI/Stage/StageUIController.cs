using System.Threading;
using UnityEngine;
using Sirenix.OdinInspector;

public class StageUIController : MonoBehaviour
{
    [SerializeField, Required]
    private StagePanel _stagePanel;
    [SerializeField, Required]
    private GameObject _pausePanel;
    [SerializeField, Required]
    private GameObject _gameOverPanel;

    private void Awake()
    {
        StageManager.Instance.StateChanged += OnStageStateChanged;
    }

    private void Start()
    {
        _stagePanel.gameObject.SetActive(true);
        _pausePanel.SetActive(false);
        _gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        _stagePanel.Dispose();
    }

    private void OnStageStateChanged(StageState from, StageState to)
    {
        if (to == StageState.Initialized)
        {
            _stagePanel.Initialize();
        }
        else if (to == StageState.Paused)
        {
            _pausePanel.SetActive(true);
        }
        else if (to == StageState.Resuming)
        {
            
        }
        else if (to == StageState.Playing)
        {
            if (from == StageState.Paused)
                _pausePanel.SetActive(false);
        }
        else if (to == StageState.GameOver)
        {
            _stagePanel.gameObject.SetActive(false);
            _gameOverPanel.SetActive(true);
        }
    }
}
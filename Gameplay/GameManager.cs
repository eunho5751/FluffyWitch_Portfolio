using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

public class GameManager : SingletonBehaviourBase<GameManager>
{
    [SerializeField]
    private AssetReferenceScene _titleScene;
    [SerializeField]
    private AssetReferenceScene _stageScene;

    private bool _isSceneLoading;

    private async UniTaskVoid Start()
    {
        await InitializeAsync();
        MoveToTitle();
    }

    public void MoveToTitle()
    {
        LoadScene(_titleScene).Forget();
    }

    public void MoveToStage()
    {
        LoadScene(_stageScene).Forget();
    }
    
    private async UniTask InitializeAsync()
    {
        Application.targetFrameRate = 60;

        await Database.LoadAsync();
        await VFXManager.Instance.LoadAsync();
    }

    private async UniTaskVoid LoadScene(AssetReferenceScene scene)
    {
        if (_isSceneLoading)
        {
            Debug.LogError("Scene loading is currently in progress!");
            return;
        }

        _isSceneLoading = true;
        await Addressables.LoadSceneAsync(scene);
        _isSceneLoading = false;
    }

    public Database Database { get; } = new();
}
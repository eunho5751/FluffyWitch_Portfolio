using UnityEngine;

public abstract class SingletonBehaviourBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    [SerializeField]
    private bool _dontDestroy = true;

    private void Awake()
    {
        if (_dontDestroy)
            DontDestroyOnLoad(gameObject);
        OnAwake();
    }

    protected virtual void OnAwake() { }

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                    Debug.LogError($"Failed to find '{typeof(T)}' instance in the scenes.");
            }
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;
}
using UnityEngine;
using Sirenix.OdinInspector;

public class OutOfBoundsDespawner : MonoBehaviour
{
    [SerializeField, Required]
    private EntityBase _entity;

    private StageManager _stageMgr;

    private void Awake()
    {
        _stageMgr = StageManager.Instance;
    }

    private void Update()
    {
        if (_stageMgr.IsOutOfDespawnBounds(transform.position))
        {
            _entity.Despawn();
        }
    }
}
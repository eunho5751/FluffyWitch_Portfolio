using UnityEngine;
using Sirenix.OdinInspector;

public class CloudsRandomizer : MonoBehaviour
{
    [SerializeField, Required]
    private LayerObjectScroller _cloudsScroller;
    [SerializeField]
    private int _cloudsLayerSortingOrder = 20;
    [SerializeField]
    private int _backLayerSortingOrder = 10;
    [SerializeField, MinValue(0f), MaxValue(1f)]
    private float _cloudsLayerChance = 0.3f;
    [SerializeField]
    private Sprite[] _sprites;

    private void OnEnable()
    {
        _cloudsScroller.ObjectSpawned += OnCloudSpawned;
    }

    private void OnDisable()
    {
        _cloudsScroller.ObjectSpawned -= OnCloudSpawned;
    }

    private void OnCloudSpawned(GameObject cloud)
    {
        if (_sprites.Length == 0 || !cloud.TryGetComponent(out SpriteRenderer renderer))
            return;

        renderer.sprite = _sprites[Random.Range(0, _sprites.Length)];
        renderer.sortingOrder = Random.value < _cloudsLayerChance ? _cloudsLayerSortingOrder : _backLayerSortingOrder;
    }
}
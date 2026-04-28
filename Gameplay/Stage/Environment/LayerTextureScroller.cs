using UnityEngine;

public class LayerTextureScroller : MonoBehaviour
{
    [SerializeField]
    private Renderer _renderer;
    [SerializeField]
    private float _speed = 1f;

    private Material _mat;

    private void Awake()
    {
        _mat = _renderer.material;
    }

    private void Update()
    {
        var offset = _mat.mainTextureOffset;
        offset.x += Time.deltaTime * _speed / transform.lossyScale.x;
        _mat.mainTextureOffset = offset;
    }
}
using UnityEngine;

public class AspectRatioScaler : MonoBehaviour
{
    [SerializeField]
    private Vector2Int _referenceResolution = new(1920, 1080);

    private void Start()
    {
        var camera = Camera.main;
        float targetAspect = (float)camera.pixelWidth / camera.pixelHeight;
        float referenceAspect = (float)_referenceResolution.x / _referenceResolution.y;
        float scaler = targetAspect / referenceAspect;
        transform.localScale *= scaler;
    }
}
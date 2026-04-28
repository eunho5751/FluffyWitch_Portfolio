using UnityEngine;

public class ViewportAnchor : MonoBehaviour
{
    [SerializeField]
    private Vector2 _viewportPoint;

    private void Start()
    {
        Vector2 wp = Camera.main.ViewportToWorldPoint(_viewportPoint);
        transform.position = wp;
    }
}
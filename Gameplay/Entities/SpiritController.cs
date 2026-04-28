using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SpiritController : MonoBehaviour
{
    [SerializeField, MinValue(0f)]
    private float _orbitRadius = 1.5f;
    [SerializeField, MinValue(0f)]
    private float _orbitSpeed = 120f;
    [SerializeField, MinValue(0f)]
    private float _repositionSpeed = 5f;

    private readonly List<Transform> _spirits = new();
    private readonly List<float> _currentOffsets = new();
    private float _angle;

    public void Register(Transform spirit)
    {
        _currentOffsets.Add(_spirits.Count > 0 ? _currentOffsets[_spirits.Count - 1] : 0f);
        _spirits.Add(spirit);
    }

    public void Unregister(Transform spirit)
    {
        int index = _spirits.IndexOf(spirit);
        if (index < 0)
            return;

        _spirits.RemoveAt(index);
        _currentOffsets.RemoveAt(index);
    }

    private void Update()
    {
        int count = _spirits.Count;
        if (count == 0)
            return;

        _angle += _orbitSpeed * Time.deltaTime;
        if (_angle >= 360f)
            _angle -= 360f;

        float angleDiff = 360f / count;
        Vector2 center = transform.position;
        float lerpT = 1f - Mathf.Exp(-_repositionSpeed * Time.deltaTime);

        for (int i = 0; i < count; i++)
        {
            float targetOffset = angleDiff * i;
            float currentOffset = Mathf.LerpAngle(_currentOffsets[i], targetOffset, lerpT);
            _currentOffsets[i] = currentOffset;

            float spiritAngle = _angle + currentOffset;
            float rad = spiritAngle * Mathf.Deg2Rad;
            _spirits[i].position = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _orbitRadius;
        }
    }

    public int SpiritCount => _spirits.Count;
}

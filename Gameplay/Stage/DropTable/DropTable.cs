using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/DropTable")]
public class DropTable : ScriptableObject, IReadOnlyList<DropEntry>
{
    [SerializeField]
    private List<DropEntry> _dropEntries;
    [SerializeField, MinValue(0f)]
    private float _missWeight;

    public List<DropEntry>.Enumerator GetEnumerator() => _dropEntries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<DropEntry> IEnumerable<DropEntry>.GetEnumerator() => GetEnumerator();

    public DropEntry this[int index] => _dropEntries[index];
    public int Count => _dropEntries.Count;
    public IReadOnlyList<DropEntry> DropEntries => _dropEntries;
    public float MissWeight => _missWeight;
}
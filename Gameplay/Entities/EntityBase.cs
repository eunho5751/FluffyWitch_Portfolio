using UnityEngine;
using Sirenix.OdinInspector;

public abstract class EntityBase : MonoBehaviour
{
    [SerializeField, Required]
    private Rigidbody2D _rigidbody;
    [SerializeField, Required]
    private Collider2D _collider;

    private Vector2 _velocity;

    public void Spawn(Vector2 position, Quaternion rotation)
    {
        if (IsSpawned)
            return;

        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
        IsSpawned = true;
        OnSpawn();
    }

    public void Spawn(Vector2 position)
    {
        Spawn(position, transform.rotation);
    }

    public void Despawn()
    {
        if (!IsSpawned)
            return;

        OnDespawn();
        gameObject.SetActive(false);
        IsSpawned = false;
    }

    public void ExcludeCollisionLayers(LayerMask layers)
    {
        _rigidbody.excludeLayers = layers;
    }

    protected virtual void OnSpawn() { }
    protected virtual void OnDespawn() { }
    protected virtual void OnMove(ref Vector2 velocity) { }
    protected virtual void OnRotate(ref float angle) { }
    protected virtual void OnCollision(EntityBase entity) { }

    private void FixedUpdate()
    {
        float angle = _rigidbody.rotation;
        OnMove(ref _velocity);
        OnRotate(ref angle);
        
        Vector2 nextPos = _rigidbody.position + _velocity * Time.fixedDeltaTime;
        _rigidbody.MovePositionAndRotation(nextPos, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsSpawned)
            return;

        if (collision.TryGetComponent(out EntityBase entity))
        {
            OnCollision(entity);
        }
    }

    protected Collider2D Collider => _collider;

    public bool IsSpawned { get; private set;}

    public Vector2 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    public float Rotation
    {
        get => _rigidbody.rotation;
        set => _rigidbody.rotation = value;
    }

    public Vector2 PhysicsPosition => _rigidbody.position;
    public Vector2 Position => transform.position;

    public Bounds Bounds => _collider.bounds;

   public int Layer => gameObject.layer;
}
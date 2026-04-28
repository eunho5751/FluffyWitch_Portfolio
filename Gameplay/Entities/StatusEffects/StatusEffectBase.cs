using UnityEngine;

public abstract class StatusEffectBase<T> : StatusEffectBase where T : StatusEffectDataBase
{
    protected StatusEffectBase(T data) : base(data) { }

    public new T Data => (T)base.Data;
}

public abstract class StatusEffectBase
{
    protected StatusEffectBase(StatusEffectDataBase data)
    {
        Data = data;
    }

    public void Initialize(CharacterBase caster, CharacterBase target)
    {
        Caster = caster;
        Target = target;
        Stacks = 1;
        RemainingDuration = Data.Duration;
    }

    public void AddStack()
    {
        if (Data.CanStack && Stacks < Data.MaxStacks)
        {
            Stacks++;
            OnStackAdded(Stacks);
        }

        if (!IsPermanent)
        {
            RemainingDuration = Data.DurationPolicy switch
            {
                StatusEffectDurationPolicy.Refresh    => Data.Duration,
                StatusEffectDurationPolicy.Extend     => RemainingDuration + Data.Duration,
                StatusEffectDurationPolicy.Ignore     => RemainingDuration,
                _ => RemainingDuration
            };
        }
    }

    public void Tick(float deltaTime)
    {
        OnTick();
        if (!IsPermanent)
            RemainingDuration -= deltaTime;
    }

    public virtual void OnApply() { }
    public virtual void OnTick() { }
    public virtual void OnRemove() { }
    public virtual void OnStackAdded(int stacks) { }

    public StatusEffectDataBase Data { get; }
    public CharacterBase Caster { get; private set;}
    public CharacterBase Target { get; private set; }
    public int Stacks { get; private set; }
    public float RemainingDuration { get; private set; }
    public bool IsPermanent => Data.Duration <= 0f;
    public bool IsExpired => !IsPermanent && RemainingDuration <= 0f;
}

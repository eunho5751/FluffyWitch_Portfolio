using System;
using UnityEngine;

public class PlayerCharacterFeedback : CharacterFeedbackBase
{
    [SerializeField]
    private FXClip _dieFX;

    protected override void OnDied()
    {
        _dieFX.Play(Character.Position);
    }
}
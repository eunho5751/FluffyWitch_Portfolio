using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class ChainLightning : MonoBehaviour
{
    private const int ZigzagSegments = 3;
    private const float ZigzagAmplitude = 0.15f;

    [SerializeField, Required]
    private LineRenderer _renderer;

    public void Execute(CharacterBase attacker, EntityBase firstTarget, int maxChains, float chainRange, float damage)
    {
        var hitEntities = new HashSet<EntityBase>
        {
            firstTarget
        };

        // Chain to nearby enemies
        Vector2 currentPos = firstTarget.Position;
        for (int i = 0; i < maxChains; i++)
        {
            var next = EntityHelper.FindClosestOpponent(attacker.IsPlayerCharacter, currentPos, 0f, chainRange, hitEntities);
            if (next == null)
                break;

            bool isValidHit = false;
            HitContext damageCtx = HitContext.Impact(damage, DamageFlags.None, attacker, next);
            if (CombatSystem.Instance.TryApplyHit(damageCtx, out var result))
            {
                if (result.IsResolved)
                {
                    hitEntities.Add(next);
                    currentPos = next.Position;
                    isValidHit = true;
                }
            }

            if (!isValidHit)
            {
                i--;
            }
        }

        if (hitEntities.Count > 1)
        {
            // Build lightning line
            var hitPositions = hitEntities.Select(x => x.Position).ToArray();
            BuildLightningLine(hitPositions);
        }
        else
        {
            _renderer.positionCount = 0;
        }
    }

    private void BuildLightningLine(IReadOnlyList<Vector2> positions)
    {
        if (positions.Count < 2)
        {
            _renderer.positionCount = positions.Count;
            if (positions.Count == 1)
                _renderer.SetPosition(0, (Vector3)positions[0]);
            return;
        }

        int segmentCount = positions.Count - 1;
        int totalPoints = 1 + segmentCount * (ZigzagSegments + 1);
        var linePoints = new Vector3[totalPoints];
        int idx = 0;

        linePoints[idx++] = (Vector3)positions[0];
        for (int i = 0; i < segmentCount; i++)
        {
            Vector2 from = positions[i];
            Vector2 to = positions[i + 1];
            Vector2 dir = (to - from).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x);
            float segmentLength = Vector2.Distance(from, to);

            for (int j = 1; j <= ZigzagSegments; j++)
            {
                float t = (float)j / (ZigzagSegments + 1);
                Vector2 midPoint = Vector2.Lerp(from, to, t);
                float offset = UnityEngine.Random.Range(-ZigzagAmplitude, ZigzagAmplitude) * segmentLength;
                midPoint += perp * offset;
                linePoints[idx++] = (Vector3)midPoint;
            }

            linePoints[idx++] = (Vector3)to;
        }

        _renderer.positionCount = totalPoints;
        _renderer.SetPositions(linePoints);
    }
}
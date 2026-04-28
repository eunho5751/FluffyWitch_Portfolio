using UnityEngine;
using Sirenix.OdinInspector;

[HideMonoScript]
[CreateAssetMenu(menuName = "FluffyWitch/Stages/StageConfig", order = int.MaxValue)]
public class StageConfig : ScriptableObject
{
    [SerializeField, Required]
    private StageThemeCollection _themeCollection;
    [SerializeField, Required]
    private PlayerCharacterData _defaultPlayerCharacter;
    [SerializeField]
    private DropTable _dropTable;
    [SerializeField, MinValue(0f)]
    private float _bossSpawnTimeReductionPerKill = 0.1f;

    public StageThemeCollection ThemeCollection => _themeCollection;
    public PlayerCharacterData DefaultPlayerCharacter => _defaultPlayerCharacter;
    public DropTable DropTable => _dropTable;
    public float BossSpawnTimeReductionPerKill => _bossSpawnTimeReductionPerKill;
}
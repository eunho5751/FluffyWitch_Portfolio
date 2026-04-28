using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class PlayerCharacterStatusUI : MonoBehaviour
{
    [SerializeField, Required]
    private Image _hpBar;
    [SerializeField, Required]
    private TextMeshProUGUI _currentHpText;
    [SerializeField, Required]
    private TextMeshProUGUI _maxHpText;
    [SerializeField, Required]
    private TextMeshProUGUI _levelText;
    [SerializeField, Required]
    private Image _expBar;

    private PlayerCharacter _playerCharacter;

    public void Initialize()
    {
        _playerCharacter = StageManager.Instance.PlayerCharacter;

        _playerCharacter.HpChanged += OnPlayerCharacterHpChanged;
        OnPlayerCharacterHpChanged(_playerCharacter.CurrentHp);

        _playerCharacter.LevelChanged += OnPlayerCharacterLevelChanged;
        OnPlayerCharacterLevelChanged(_playerCharacter.CurrentLevel);

        _playerCharacter.ExpChanged += OnPlayerCharacterExpChanged;
        OnPlayerCharacterExpChanged(_playerCharacter.CurrentExp);
    }

    private void OnPlayerCharacterHpChanged(float currentHp)
    {
        float maxHp = _playerCharacter.GetStat(StatType.MaxHp).Value;
        UpdateHp(currentHp, maxHp);
    }

    private void OnPlayerCharacterLevelChanged(int currentLevel)
    {
        UpdateLevel(currentLevel);
    }

    private void OnPlayerCharacterExpChanged(int currentExp)
    {
        int maxExp = _playerCharacter.NextLevelUpExp;
        UpdateExp(currentExp, maxExp);
    }

    private void UpdateLevel(int level)
    {
        _levelText.text = level.ToString();
    }

    private void UpdateExp(int currentExp, int maxExp)
    {
        _expBar.fillAmount = (float)currentExp / maxExp;
    }

    private void UpdateHp(float currentHp, float maxHp)
    {
        _hpBar.fillAmount = currentHp / maxHp;
        _currentHpText.text = Mathf.CeilToInt(currentHp).ToString();
        _maxHpText.text = Mathf.CeilToInt(maxHp).ToString();
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class BossStatusUI : MonoBehaviour
{
    [SerializeField, Required]
    private Image _hpBar;
    [SerializeField, Required]
    private TextMeshProUGUI _nameText;

    private Enemy _bossCache;

    public void Initialize()
    {
    }

    public void Show(Enemy boss)
    {
        if (gameObject.activeSelf)
            return;
        gameObject.SetActive(true);
  
        _bossCache = boss;
        _bossCache.HpChanged += OnBossHpChanged;
        SetInitialState();
    }

    public void Hide()
    {
        if (!gameObject.activeSelf)
            return;

        _bossCache.HpChanged -= OnBossHpChanged;
        _bossCache = null;
        gameObject.SetActive(false);
    }
    
    private void OnBossHpChanged(float hp)
    {
        float maxHp = _bossCache.GetStat(StatType.MaxHp).Value;
        UpdateHp(hp, maxHp);
    }

    private void SetInitialState()
    {
        _nameText.text = _bossCache.Data.DisplayName;
        OnBossHpChanged(_bossCache.CurrentHp);
    }
    
    private void UpdateHp(float currentHp, float maxHp)
    {
        _hpBar.fillAmount = currentHp / maxHp;
    }
}
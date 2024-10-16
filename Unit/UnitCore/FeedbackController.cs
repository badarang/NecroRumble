using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using LOONACIA.Unity;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

public class FeedbackController : MonoBehaviour
{
    private Unit _unit;
    private Transform _modelContainer;
    private Rigidbody2D _rb;
    private Transform _feedbackContainer => _modelContainer.Find("FeelFeedbacks");
    
    #region OuterEffects 
    //Flipable에 영향받지 않음
    private Transform _outerEffectContainer;
    private GameObject _fearEffect;
    private GameObject _poisonIcon;
    private GameObject _poisonEffect;
    private GameObject _thirdHitEffect;
    private GameObject _slowIcon;
    private GameObject _attackSpeedEffect;
    private GameObject _moveSpeedEffect;
    
    #endregion

    #region InnerEffects

    private Transform EffectContainer => _feedbackContainer.Find("Effects");
    private GameObject _stunEffect => EffectContainer.Find("StunEffect").gameObject;
    private GameObject _chargeEffect => EffectContainer.Find("ChargeEffect").gameObject;
    private GameObject _enforcedEffect => EffectContainer.Find("VFXOrbital").gameObject;
    private GameObject _charmingEffect => EffectContainer.Find("CharmingEffect").gameObject;
    private GameObject _shieldEffect => EffectContainer.Find("ShieldEffect").gameObject;
    private GameObject _invincibleEffect => EffectContainer.Find("InvincibleEffect").gameObject;
    private GameObject _crownEffect => EffectContainer.Find("CrownEffect").gameObject;
    private GameObject _giantSkeletonShieldEffect => EffectContainer.Find("GiantSkeletonShieldEffect").gameObject;

    #endregion
    
    [ShowInInspector] private Material _material;
    [SerializeField] private List<FeedbackContainer> _feedbacksList = new List<FeedbackContainer>();

    private Coroutine _stunEffectRoutine;
    private Coroutine _chargeEffectRoutine;
    private Coroutine _fearEffectRoutine;
    private Coroutine _poisonEffectRoutine;
    private Coroutine _runAwayRoutine;
    private Coroutine _slowEffectRoutine;

    private Coroutine _attackSpeedEffectRoutine;
    private Coroutine _playerInvincibleCoroutine;


    public void Init()
    {
        _unit = TryGetComponent<Unit>(out Unit unit) ? unit : null;
        _rb = GetComponent<Rigidbody2D>();
        _modelContainer = gameObject.FindChild("ModelContainer").transform;

        if (_unit == null) // Player
        {
            _material = _modelContainer.Find("@SpriteModel").GetComponent<SpriteRenderer>().material;
            _outerEffectContainer = gameObject.FindChild("OuterEffects").transform;
            _slowIcon = _outerEffectContainer.Find("SlowIcon").gameObject;
        }
        else // Unit
        {
            if (_unit.CurrentFaction == Faction.Human)
            {
                _material = _modelContainer.Find("@HumanModel").GetComponent<SpriteRenderer>().material;
            }
            else
            {
                _material = _modelContainer.Find("@UndeadModel").GetComponent<SpriteRenderer>().material;
            }
            _outerEffectContainer = gameObject.FindChild("OuterEffects").transform;
            _fearEffect = _outerEffectContainer.Find("FearEffect").gameObject;
            _poisonEffect = _outerEffectContainer.Find("PoisonEffect").gameObject;
            _poisonIcon = _outerEffectContainer.Find("PoisonIcon").gameObject;
            _thirdHitEffect = _outerEffectContainer.Find("ThirdHitEffect").gameObject;
            _slowIcon = _outerEffectContainer.Find("SlowIcon").gameObject;
            _attackSpeedEffect = _outerEffectContainer.Find("AttackSpeedEffect").gameObject;
            _moveSpeedEffect = _outerEffectContainer.Find("MoveSpeedEffect").gameObject;
            ResetMaterials();
        }
        
    }

    public void ClearEffectCoroutines()
    {
        if (_stunEffectRoutine != null) StopCoroutine(_stunEffectRoutine);
        if (_chargeEffectRoutine != null) StopCoroutine(_chargeEffectRoutine);
        if (_fearEffectRoutine != null) StopCoroutine(_fearEffectRoutine);
        if (_poisonEffectRoutine != null) StopCoroutine(_poisonEffectRoutine);
        if (_runAwayRoutine != null) StopCoroutine(_runAwayRoutine);
        if (_slowEffectRoutine != null) StopCoroutine(_slowEffectRoutine);
        if (_attackSpeedEffectRoutine != null) StopCoroutine(_attackSpeedEffectRoutine);
        if (_playerInvincibleCoroutine != null) StopCoroutine(_playerInvincibleCoroutine);
    }
    
    public void SetMaterialHitEffect()
    {
        if (_unit == null) // Player
        {
            if (_material.HasProperty("_HitEffectBlend"))
            {
                _material.SetFloat("_HitEffectBlend", 1f);
            }
        }
        else
        {
            if (_unit.CurrentFaction == Faction.Undead)
            {
                if (_material.HasProperty("_HitEffectBlend"))
                {
                    _material.SetFloat("_HitEffectBlend", 1f);
                }
                else if (_material.HasProperty("_GlowGlobal"))
                {
                    _material.SetFloat("_GlowGlobal", 10f);
                }
            }
            else
            {
                if (_material.HasProperty("_HitEffectBlend"))
                {
                    _material.SetFloat("_HitEffectBlend", 1f);
                }
            }
        }
    }
    
    public void ApplyInvincibleEffect(float playerInvincibleTime)
    {
        if (_material.HasProperty("_FlickerFreq"))
        {
            _material.SetFloat("_FlickerFreq", 1.5f);
        }
        // if (_material.HasProperty("_ColorRampBlend"))
        // {
        //     _material.SetFloat("_ColorRampBlend", 0f);
        // }
        if (_playerInvincibleCoroutine != null) StopCoroutine(_playerInvincibleCoroutine);
        _playerInvincibleCoroutine = StartCoroutine(PlayerInvincibleCoroutine(playerInvincibleTime));
    }
    
    private IEnumerator PlayerInvincibleCoroutine(float playerInvincibleTime)
    {
        yield return new WaitForSeconds(playerInvincibleTime);
        if (_material.HasProperty("_FlickerFreq"))
        {
            _material.SetFloat("_FlickerFreq", 0f);
        }
        // if (_material.HasProperty("_ColorRampBlend"))
        // {
        //     _material.SetFloat("_ColorRampBlend", 0f);
        // }
    }
    
    public void ApplyAttackerHit(float mass, Vector3 originPos, float power = 20f)
    {
        Vector2 direction = transform.position - originPos;
        _rb.mass = mass;
        _rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);
        PlayFeedback("Hit");
        SetMaterialHitEffect();
    }
    
    public void ApplyAttackerKnockback(float mass, Vector3 originPos, float power = 20f)
    {
        Vector2 direction = transform.position - originPos;
        _rb.mass = mass;
        _rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);
        PlayFeedback("Knockback");
        SetMaterialHitEffect();
    }
    
    public void ApplyAttackerKnockdown(float mass, Vector3 originPos, float power = 30f, float duration = 2f)
    {
        Vector2 direction = transform.position - originPos;
        _rb.mass = mass;
        _rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);
        MMF_Rotation knockdownRotation = GetFeedback("Knockdown").FeedbacksList[0] as MMF_Rotation;
        //knockdownRotation.RemapCurveOne = -90f;
        
        if (transform.position.x < originPos.x) //Way 1
        {
            knockdownRotation.RemapCurveOne = -90f;
            AnimationClip clip = new AnimationClip();
        }
        else
        {
            knockdownRotation.RemapCurveOne = 90f;
        }
        
        //knockdownRotation.RemapCurveOne = _unit.GetFlipableLocalScaleX() > 0f ? -90f : 90f; // Way 2
        PlayFeedback("Knockdown");
        knockdownRotation.AnimateRotationDuration = duration;
        if (_stunEffect != null)
        {
            _stunEffect.SetActive(true);
            //float offsetX = _unit.GetFlipableLocalScaleX() > 0f ? -0.5f : 0.5f;
            //_stunEffect.transform.localPosition = new Vector3(offsetX, 0.7f, 0f);
            if (_stunEffectRoutine != null) StopCoroutine(_stunEffectRoutine);
            _stunEffectRoutine = StartCoroutine(StunEffectRoutine(duration));
        }
        SetMaterialHitEffect();
    }

    public void ApplyAttackerFear(float duration = 2f)
    {
        if (_unit.IsDead) return;
        if (_fearEffect != null)
        {
            _fearEffect.SetActive(false);
            _fearEffect.GetComponent<FearEffectController>().Duration = duration;
            _fearEffect.SetActive(true);
            if (_fearEffectRoutine != null) StopCoroutine(_fearEffectRoutine);
            _fearEffectRoutine = StartCoroutine(FearEffectRoutine(duration));
        }
    }
    
    public void ApplyAttackerPoison(float duration = 2f)
    {
        if (_unit.IsDead) return;
        if (_poisonEffect != null)
        {
            _poisonEffect.SetActive(true);
            if (_poisonEffectRoutine != null) StopCoroutine(_poisonEffectRoutine);
            _poisonEffectRoutine = StartCoroutine(PoisonEffectRoutine(duration));
        }
        if (_poisonIcon != null)
        {
            _poisonIcon.SetActive(true);
        }
    }
    
    public void ApplyShieldEffect(bool isOn)
    {
        if (_unit.IsDead) return;
        if (_shieldEffect != null)
        {
            _shieldEffect.SetActive(isOn);
        }
    }
    
    public void ApplyGiantSkeletonShieldEffect(bool isOn)
    {
        if (_unit.IsDead) return;
        if (_giantSkeletonShieldEffect != null)
        {
            _giantSkeletonShieldEffect.SetActive(isOn);
        }
        else
        {
            Debug.Log("_giantSkeletonShieldEffect is null");
        }
    }
    
    public void ApplyAttackerSlow(float duration = 2f)
    {
        if (_unit.IsDead) return;
        if (_slowEffectRoutine != null) StopCoroutine(_slowEffectRoutine);
        _slowEffectRoutine = StartCoroutine(SlowEffectRoutine(duration));
        
        if (_slowIcon != null)
        {
            _slowIcon.SetActive(true);
        }
    }
    
    public void SetChargeEffect(float duration)
    {
        if (_unit.IsDead) return;
        if (_chargeEffect != null)
        {
            _chargeEffect.SetActive(true);
            if (_unit.CurrentFaction == Faction.Undead)
            {
                _chargeEffect.GetComponent<Animator>().Play("ChargeEffectUndead");
            }
            else
            {
                _chargeEffect.GetComponent<Animator>().Play("ChargeEffect");
            }
            if (_chargeEffectRoutine != null) StopCoroutine(_chargeEffectRoutine);
            _chargeEffectRoutine = StartCoroutine(ChargeEffectRoutine(duration));
        }
    }
    
    public void SetCrownEffect(bool isOn)
    {
        if (_unit.IsDead && isOn) return;
        if (_crownEffect != null)
        {
            _crownEffect.SetActive(isOn);
        }
    }
    
    public void ControlThirdHitEffect(int thirdHitCount)
    {
        if (_thirdHitEffect != null)
        {
            if (thirdHitCount == 0)
            {
                _thirdHitEffect.SetActive(false);
            }
            else if (thirdHitCount == 1)
            {
                _thirdHitEffect.SetActive(true);
                _thirdHitEffect.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Effects/ThirdHitEffect1");
            }
            else if (thirdHitCount == 2)
            {
                _thirdHitEffect.SetActive(true);
                _thirdHitEffect.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Effects/ThirdHitEffect2");
            }
        }
    }
    
    public void StartAttackSpeedEffectCoroutine(bool isUp, float duration)
    {
        if (_attackSpeedEffectRoutine != null) StopCoroutine(_attackSpeedEffectRoutine);
        _attackSpeedEffectRoutine = StartCoroutine(ApplyAttackSpeedEffect(isUp, duration));
    }
    
    private IEnumerator ApplyAttackSpeedEffect(bool isUp, float duration)
    {
        ParticleSystem particle = _attackSpeedEffect.GetComponent<ParticleSystem>();
        if (isUp)
        {
            Vector3 scale = _attackSpeedEffect.transform.localScale;
            scale.y = Mathf.Abs(scale.y) * 1;
            Debug.Log(scale.y);
            _attackSpeedEffect.transform.localScale = scale;
            
            Vector3 rotation = _attackSpeedEffect.transform.localRotation.eulerAngles;
            rotation.x = -90f;
            _attackSpeedEffect.transform.localRotation = Quaternion.Euler(rotation);

            var colorOverLifetime = particle.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(new Color(255, 193, 0), 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1.0f, 1.0f) });
        }
        else
        {
            Vector3 scale = _attackSpeedEffect.transform.localScale;
            scale.y = Mathf.Abs(scale.y) * (-1);
            _attackSpeedEffect.transform.localScale = scale;
            
            Vector3 rotation = _attackSpeedEffect.transform.localRotation.eulerAngles;
            rotation.x = 90f;
            _attackSpeedEffect.transform.localRotation = Quaternion.Euler(rotation);
            var colorOverLifetime = particle.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(new Color(0, 193, 255), 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1.0f, 1.0f) });
        }
        
        if (_unit.IsDead == false) _attackSpeedEffect.SetActive(true);
        yield return new WaitForSeconds(duration);
        _attackSpeedEffect.SetActive(false);
    }

    public void SetMoveSpeedEffect(bool isOn)
    {
        if (_moveSpeedEffect != null)
        {
            _moveSpeedEffect.SetActive(isOn);
        }
    }

    public MMF_Player GetFeedback(string feedbackName)
    {
        return _feedbacksList.SingleOrDefault(container => container.name == feedbackName).feedback;
    }
    
    public bool PlayFeedback(string feedbackName)
    {
        var feedback = _feedbacksList.SingleOrDefault(container => container.name == feedbackName).feedback;
        if (feedback != null)
        {
            feedback.PlayFeedbacks();
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void ChangeMaterialBasedOnIsSelected(bool isSelected, bool isReviveReady = true)
    {
        //Selected
        const float OUTLINE_ALPHA = 1f;
        const float WAVE_STRENGTH = 5f;
        //ReviveReady
        Color readyOutlineColor = new Color(1, 1, 1);
        //Selected but not ReviveReady
        Color notReadyOutlineColor = new Color(0.35f, 0f, 0f);

        float outlineAlpha;
        float waveStrength;
        Color outlineColor;
        
        if (isSelected)
        {
            outlineAlpha = OUTLINE_ALPHA;
            waveStrength = WAVE_STRENGTH;
            outlineColor = isReviveReady ? readyOutlineColor : notReadyOutlineColor;
        }
        else
        {
            outlineAlpha = 0f;
            waveStrength = 0f;
            outlineColor = readyOutlineColor;
        }
        
        if (_material.HasProperty("_WaveStrength"))
        {
            _material.SetFloat("_WaveStrength", isSelected ? waveStrength : 0f);
        }

        if (_material.HasProperty("_OutlineAlpha"))
        {
            _material.SetFloat("_OutlineAlpha", isSelected ? outlineAlpha : 0f);
        }
        
        if (_material.HasProperty("_OutlineColor"))
        {
            _material.SetColor("_OutlineColor", outlineColor);
        }
    }

    public void SetOutlineWhenTargetPlayer(bool isOn)
    {
        if (_material.HasProperty("_OutlineAlpha"))
        {
            _material.SetFloat("_OutlineAlpha", isOn ? 1f : 0f);
        }
        if (_material.HasProperty("_OutlineColor"))
        {
            //lightred
            _material.SetColor("_OutlineColor", new Color(0.53f, 0f, 0f));
        }
    }
    
    public void ChangeMaterialGhostGlitch(bool isGhostGlitch)
    {
        if (_material.HasProperty("_GlitchAmount"))
        {
            _material.SetFloat("_GlitchAmount", isGhostGlitch ? 0.5f : 0f);
        }
    }

    public void ChangeMaterialGhostBlend(bool isGhostBlend)
    {
        if (_material.HasProperty("_HologramBlend"))
        {
            _material.SetFloat("_HologramBlend", isGhostBlend ? 1f : 0f);
        }
    }
    
    public void ChangeMaterialCharmingGlow(bool isGlow)
    {
        if (_material.HasProperty("_Glow"))
        {
            _material.SetColor("_GlowColor", new Color(1f, 0.1f, 1f));
            _material.SetFloat("_Glow", isGlow ? 1f : 0f);
        }

        if (isGlow)
        {
            _charmingEffect.SetActive(true);
        }
        else
        {
            _charmingEffect.SetActive(false);
        }
    }
    
    public void ChangeMaterialIsInvincibleGlow(bool isGlow)
    {
        if (_material.HasProperty("_Glow"))
        {
            _material.SetColor("_GlowColor", new Color(1f, 1f, 0f));
            _material.SetFloat("_Glow", isGlow ? 1f : 0f);
        }

        if (isGlow)
        {
            _invincibleEffect.SetActive(true);
        }
        else
        {
            _invincibleEffect.SetActive(false);
        }
    }
    
    public void ResetMaterials()
    {
        if (_material.HasProperty("_GlitchAmount"))
        {
            _material.SetFloat("_GlitchAmount", 0f);
        }
        if (_material.HasProperty("_GhostTransparency"))
        {
            _material.SetFloat("_GhostTransparency", 0f);
        }
        if (_material.HasProperty("_WaveStrength"))
        {
            _material.SetFloat("_WaveStrength", 0f);
        }
        if (_material.HasProperty("_OutlineAlpha"))
        {
            _material.SetFloat("_OutlineAlpha", 0f);
        }
        if (_material.HasProperty("_GreyScaleBlend"))
        {
            _material.SetFloat("_GreyScaleBlend", 0f);
        }
        if (_material.HasProperty("_HologramBlend"))
        {
            _material.SetFloat("_HologramBlend", 0f);
        }
        if (_material.HasProperty("_GhostBlend"))
        {
            _material.SetFloat("_GhostBlend", 0f);
        }
        if (_material.HasProperty("_Glow"))
        {
            _material.SetFloat("_Glow", 0f);
        }

        if (_material.HasProperty("_GlowColor"))
        {
            _material.SetColor("_GlowColor", new Color(1f, 1f, 1f));
        }
        if (_charmingEffect != null)
        {
            _charmingEffect.SetActive(false);
        }
        if (_stunEffect != null)
        {
            _stunEffect.SetActive(false);
        }
        if (_chargeEffect != null)
        {
            _chargeEffect.SetActive(false);
        }
        if (_fearEffect != null)
        {
            _fearEffect.SetActive(false);
        }
        if (_poisonEffect != null)
        {
            _poisonEffect.SetActive(false);
        }
        if (_poisonIcon != null)
        {
            _poisonIcon.SetActive(false);
        }
        if (_thirdHitEffect != null)
        {
            _thirdHitEffect.SetActive(false);
        }
        if (_slowIcon != null)
        {
            _slowIcon.SetActive(false);
        }
        if (_attackSpeedEffect != null)
        {
            if (_attackSpeedEffectRoutine != null) StopCoroutine(_attackSpeedEffectRoutine);
            _attackSpeedEffect.SetActive(false);
        }
        if (_moveSpeedEffect != null && _unit.IsDead)
        {
            _moveSpeedEffect.SetActive(false);
        }
        
        _modelContainer.transform.localPosition = Vector3.zero;
    }

    public Transform GetModelContainer()
    {
        return _modelContainer;
    }
    
    public Transform GetHumanModel()
    {
        return _modelContainer.Find("@HumanModel");
    }
    
    public void ChangeUnitColor(Color color)
    {
        _modelContainer.Find("@HumanModel").GetComponent<SpriteRenderer>().color = color;
        _modelContainer.Find("@UndeadModel").GetComponent<SpriteRenderer>().color = color;
    }
    
    public Transform GetUndeadModel()
    {
        return _modelContainer.Find("@UndeadModel");
    }
    
    void Update()
    {
        //check _material has _HitEffectBlend property
        if (_material.HasProperty("_HitEffectBlend"))
        {
            if (_material.GetFloat("_HitEffectBlend") > 0f)
                _material.SetFloat("_HitEffectBlend", Mathf.Lerp(_material.GetFloat("_HitEffectBlend"), 0f, Time.deltaTime * 10f));
        }

        if (_material.HasProperty("_GlowGlobal"))
        {
            if (_material.GetFloat("_GlowGlobal") > 1f)
                _material.SetFloat("_GlowGlobal", Mathf.Lerp(_material.GetFloat("_GlowGlobal"), 1f, Time.deltaTime * 20f));
        }
        
        if (_material.HasProperty("_GlitchAmount") && _unit != null && _unit.IsDead)
        {
            if (_material.GetFloat("_GlitchAmount") > 0f)
            {
                if (_material.HasProperty("_GhostBlend"))
                {
                    _material.SetFloat("_GhostBlend", Mathf.Lerp(_material.GetFloat("_GhostBlend"), 1f, Time.deltaTime));
                }
                
                if (_material.HasProperty("_GhostTransparency"))
                {
                    _material.SetFloat("_GhostTransparency", Mathf.Lerp(_material.GetFloat("_GhostTransparency"), 1f, Time.deltaTime));
                }
                float upwardSpeed = 0.5f;
                _modelContainer.transform.Translate(Vector3.up * upwardSpeed * Time.deltaTime);
                if (_modelContainer.Find("DeadWeapon") != null)
                {
                    //DeadWeapon Alpha Down
                    SpriteRenderer deadWeaponSpriteRenderer = _modelContainer.Find("DeadWeapon").GetComponent<SpriteRenderer>();
                    deadWeaponSpriteRenderer.color = new Color(deadWeaponSpriteRenderer.color.r, deadWeaponSpriteRenderer.color.g, deadWeaponSpriteRenderer.color.b, Mathf.Lerp(deadWeaponSpriteRenderer.color.a, 0f, Time.deltaTime));
                }
            }
        }
        
    }
    
    IEnumerator StunEffectRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_stunEffect != null)
            _stunEffect.SetActive(false);
    }

    IEnumerator ChargeEffectRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_chargeEffect != null)
            _chargeEffect.SetActive(false);
    }
    
    IEnumerator FearEffectRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_fearEffect != null)
            _fearEffect.SetActive(false);
    }
    
    IEnumerator PoisonEffectRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (_poisonEffect != null)
            _poisonEffect.SetActive(false);
        if (_poisonIcon != null)
            _poisonIcon.SetActive(false);
    }
    
    IEnumerator SlowEffectRoutine(float duration)
    {
        float elapsedTime = 0f;
        GameObject slowEffectGO = Resources.Load<GameObject>("Prefabs/Effects/SlowEffect");
        while (elapsedTime < duration)
        {
            GameObject slowEffect = Instantiate(slowEffectGO, transform.position, Quaternion.identity);
            ParticleSystem particleSystem = slowEffect.GetComponent<ParticleSystem>();
            
            var list = new List<Sprite>();
            list = Resources.LoadAll<Sprite>("Sprites/Effects/FrostEffect").ToList();
            var sprite = list[UnityEngine.Random.Range(0, list.Count)];
        
            var textureSheetAnimation = particleSystem.textureSheetAnimation;
            textureSheetAnimation.SetSprite(0, sprite);
            
            Destroy(slowEffect, 5f);
            var waitTime = UnityEngine.Random.Range(0.1f, 0.3f);
            
            yield return new WaitForSeconds(waitTime);
            elapsedTime += waitTime;
        }
        
        if (_slowIcon != null)
            _slowIcon.SetActive(false);
    }
    
    public GameObject GetEnforcedEffect()
    {
        return _enforcedEffect;
    }
    
    public void ResetAllFeedbacks()
    {
        // MMF_Rotation knockdownRotation = GetFeedback("Knockdown").FeedbacksList[0] as MMF_Rotation;
        // knockdownRotation.RemapCurveOne = 0f;
       
        foreach (var feedback in _feedbacksList)
        {
            feedback.feedback.ResetFeedbacks();
            feedback.feedback.StopFeedbacks();
        }
        
        //TODO: _ModelContainer의 localRotation을 초기화하는 코드를 하드코딩스럽지 않게 변경(Rotate 부분)
        _modelContainer.transform.localRotation = Quaternion.identity;
        _modelContainer.transform.localScale = Vector3.one;

        //EffectContainer 자식들 싹다 false
        foreach (Transform child in EffectContainer)
        {
            child.gameObject.SetActive(false);
        }
        
        //OuterEffectContainer 자식들 싹다 false
        foreach (Transform child in _outerEffectContainer)
        {
            child.gameObject.SetActive(false);
        }
        
        ResetMaterials();
    }
}


[System.Serializable]
public struct FeedbackContainer
{
    public string name;
    public MMF_Player feedback;
}



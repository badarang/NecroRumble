using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class BoneSpearProjectile : Projectile
{

	private bool _dealtFirstDamage;
	private float _originalDamage;
	private void OnEnable()
	{
		Init();
	}
	private void OnDisable()
	{
		_damage = _originalDamage;
	}
	private void Init()
	{
		_dealtFirstDamage = false;
		_originalDamage = _damage;
	}
	protected override void Update()
	{
		if (Vector3.Distance(_startPos, transform.position) > _destroyRange)
		{
			DestroyBoneSpear();
		}
	}

	void DestroyBoneSpear()
	{
		GetComponent<TrailRenderer>().Clear();
		Vector3 scale = transform.localScale;
		scale.x = Mathf.Abs(transform.localScale.x);
		scale.y = Mathf.Abs(transform.localScale.y);
		transform.localScale = scale;

		ManagerRoot.Resource.Release(gameObject);
	}

	protected override void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == Layers.UndeadUnit)
		{
			if (other.TryGetComponent(out Unit unit))
			{
				ManagerRoot.Event.onUnitSoulWhip?.Invoke(unit, new AttackInfo(_owner, _damage, attackingMedium: transform));
				if (ManagerRoot.Unit.CurCharacter == UnitManager.CharacterType.CutePlayer)
				{
					unit.TakeHeal(Player.CutePlayerFixedHealAmount + Player.CutePlayerBonusHeal, transform, true);
				}
			}
			
		}
		if (other.gameObject.layer == Layers.HumanUnit)
		{
			TryTakeDamage(other.transform);

			ManagerRoot.Sound.PlaySfx("Impact 2", 0.35f);
			// ManagerRoot.Input.Vibration(0.3f, 0.15f);

			//ManagerRoot.Sound.Play("Game Punch (1)", .5f);
			//첫 대상만 기존 대미지.
			if (!_dealtFirstDamage)
			{
				_damage = Mathf.RoundToInt(_damage * .3f); //TODO: 데미지 약화 비율 변수화 (스킬 데이터)
				_dealtFirstDamage = true;
			}
		}
		if (other.gameObject.layer == Layers.PlayerObstacle)
		{
			DestroyBoneSpear();
		}
	}

}

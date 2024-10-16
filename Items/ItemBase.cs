using UnityEngine;
using DG.Tweening;
using LOONACIA.Unity.Managers;
using Unity.VisualScripting;

public abstract class ItemBase : MonoBehaviour
{
	private float _moveSpeed = 10f;
	private float _attractRadius = 3f;
	private float _interactionRadius = 1f;

	[HideInInspector] public Transform player;
	private Vector3 offsetY = new Vector3(0, 1f, 0);

	private Tween floatTween;

	protected virtual void Start()
	{
		player = GameManager.Instance.GetPlayer().Transform;
		FloatAnimation();
	}

	protected virtual void SetValues(float moveSpeed, float attractRadius, float interactionRadius)
	{
		_moveSpeed = moveSpeed;
		_attractRadius = attractRadius;
		_interactionRadius = interactionRadius;
	}
	
	protected virtual void Update()
	{
		float distanceToPlayer = Vector3.Distance(transform.position, player.position + offsetY);

		if (distanceToPlayer <= _interactionRadius)
		{
			HandleCollectEvent();
		}
		else if (distanceToPlayer <= _attractRadius)
		{
			MoveTowardsPlayer();
		}
	}

	protected virtual void MoveTowardsPlayer()
	{
		KillFloatAnimation();

		Vector3 direction = (player.position + offsetY - transform.position).normalized;
		transform.Translate(direction * _moveSpeed * Time.deltaTime);
	}

	protected abstract void HandleCollectEvent();

	private void FloatAnimation()
	{
		floatTween = transform.DOMoveY(transform.position.y + 0.2f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
	}

	private void KillFloatAnimation()
	{
		if (floatTween != null && floatTween.IsActive())
		{
			floatTween.Kill();
		}
	}
}
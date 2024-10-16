using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using LOONACIA.Unity.Managers;

public class PortalSuckedInController : MonoBehaviour
{
	private Vector2 _portalPosition = new Vector2(0f, 0f);
	//private bool _isBeingSuckedIn = false;
	private bool _canSuckedOut = false;

	public  async Task SuckInCoroutine()
	//public IEnumerator SuckInCoroutine()
	{
		//_isBeingSuckedIn = true;

		// Vector3 originalScale = transform.localScale;
		// Quaternion originalRotation = transform.localRotation;

		Sequence sequence = DOTween.Sequence();
		float randomRotation = Random.Range(45f, 360f);
		if (Random.Range(0, 2) == 0) randomRotation *= -1f;
		sequence.Append(transform.DOJump(_portalPosition, 1f, 1, 2f).SetEase(Ease.InQuart));
		sequence.Insert(1f, transform.DOScale(0f, 1f).SetEase(Ease.InQuart));
		sequence.Insert(1f, transform.DORotate(new Vector3(0f, 0f, randomRotation), 1f).SetEase(Ease.OutQuad));
		_canSuckedOut = false;
		Debug.Log("PortalSuckedInController | SuckInCoroutine");

		//await WaitForTrigger();
		await Task.Delay(1000);
		ManagerRoot.Sound.PlaySfx("Access Granted 10", 0.5f);
		await Task.Delay(2000);

		// transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
		// transform.DORotate(originalRotation.eulerAngles, 0.2f).SetEase(Ease.OutQuad);

		// Vector3 outPosition = new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), 0);
		// transform.DOJump(outPosition, 3f, 1, 1f).SetEase(Ease.OutQuad);

		//_isBeingSuckedIn = false;
		// GameManager.Instance.ChangeState(GameManager.GameState.RoundEnd);

		return;
	}

	async Task WaitForTrigger()
	{
		Debug.Log("PortalSuckedInController | WaitForTrigger");

		while (!_canSuckedOut)
		{
			await Task.Yield();
		}
	}

	public void TurnSuckedOutTrue()
	{
		_canSuckedOut = true;
	}
}

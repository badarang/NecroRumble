using System;
using UnityEngine;
using DG.Tweening;
using LOONACIA.Unity.Managers;
using Random = UnityEngine.Random;

public class Crow : MonoBehaviour
{
    private enum CrowState
    {
        Fleeing,
        Normal,
        Jumping,
        GoToOriginalPos,
    }

    private float flySpeed;
    private float animationSetDelay;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Player player;
    private CrowState currentState = CrowState.Normal;
    private Transform circleShadow;
    private bool _isJumped = false;
    private float createdTime;
    private float fleeTime;
    private float randomX;
    
    private Sequence _fleeSequence = null;

    private void Start()
    {
        flySpeed = Random.Range(7f, 10f);
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        player = GameManager.Instance.GetPlayer();
        circleShadow = transform.Find("CircleShadow");
        if (SceneManagerEx.CurrentScene.SceneType is SceneType.Title)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            currentState = CrowState.GoToOriginalPos;
            fleeTime = Time.time + Random.Range(10f, 20f);
            randomX = Random.Range(0f, 1f);
            if (randomX < 0.5f) randomX = -1f;
            else randomX = 1f;
        }
    }

    private void Update()
    {
        if (SceneManagerEx.CurrentScene.SceneType is SceneType.Title)
        {
            float distance = Vector2.Distance(transform.position, Vector2.zero);
            if (distance > 16f)
            {
                Destroy(gameObject);
            }

            if (fleeTime < Time.time)
            {
                currentState = CrowState.Fleeing;
            }
            
            switch (currentState)
            {
                case CrowState.GoToOriginalPos:
                    if (!_isJumped)
                    {
                        _isJumped = true;
                        GoToOriginalPos();
                    }
                    break;
                case CrowState.Normal:
                    NormalState(distance);
                    break;

                case CrowState.Jumping:
                    JumpState(distance);
                    break;
                case CrowState.Fleeing:
                    FleeState(distance);
                    break;
            }
            
        }
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance > 50f)
            {
                Destroy(gameObject);
            }
            switch (currentState)
            {
                case CrowState.Fleeing:
                    FleeState(distance);
                    break;

                case CrowState.Normal:
                    NormalState(distance);
                    break;

                case CrowState.Jumping:
                    JumpState(distance);
                    break;
            }
        }
    }

    private void GoToOriginalPos()
    {
        animator.Play("CrowFly");
        var dirX = Mathf.Sign(transform.position.x) * (-1);
        if (dirX > 0) spriteRenderer.flipX = true;
        var dirY = Mathf.Sign(transform.position.y) * (-1);
        var randomX = Random.Range(4f, 8f);
        var randomY = Random.Range(3f, 5f);
        
        var ranPower = Random.Range(0.5f, 1f);
        var ranDuration = Random.Range(1f, 1.5f);
        
        transform.DOJump(transform.position + new Vector3(randomX * dirX, randomY * dirY, 0f), ranPower, 1, ranDuration).OnComplete(() =>
        {
            currentState = CrowState.Normal;
            animationSetDelay = -1;
        });
    }
    
    private void FleeState(float distance)
    {
        animator.Play("CrowFly");
        
        if (_fleeSequence == null)
        {
            _fleeSequence = DOTween.Sequence();
            _fleeSequence.Append(circleShadow.DOScale(new Vector2(0f, 0f), 0.5f).SetEase(Ease.OutBack));
            _fleeSequence.Play();

            var flag = false;
            
            if (SceneManagerEx.CurrentScene.SceneType is SceneType.Game)
            {
                flag = true;
            }
            else
            {
                if (distance < 8f) flag = true;
            }

            if (flag)
            {
				if (SceneManagerEx.CurrentScene.SceneType is SceneType.Game)
				{
					SteamUserData.CheckCrowNum();
				}
				var randomNum = Random.Range(0, 10);
                if ((randomNum <= 2))
                {
                    string soundName = "Crow call " + randomNum;
                    ManagerRoot.Sound.PlaySfx(soundName, distance / 15f);
                }
                else
                {
                    string soundName = "Bird flapping wings " + Random.Range(1, 6);
                    ManagerRoot.Sound.PlaySfx(soundName, .1f);
                }
            }
        }

        float dirX;
        if (SceneManagerEx.CurrentScene.SceneType is SceneType.Title)
        {
            dirX = randomX;
        }
        else
        {
            dirX = Mathf.Sign(transform.position.x - player.transform.position.x);
        }
        
        spriteRenderer.flipX = dirX > 0 ? true : false;

        float dirY = Random.Range(.9f, 1.1f);
        transform.Translate(new Vector2(dirX, dirY) * flySpeed * Time.deltaTime);
    }
    
    private void NormalState(float distance)
    {
        if (SceneManagerEx.CurrentScene.SceneType is SceneType.Game)
        {
            if (distance < 6f) currentState = CrowState.Fleeing;
        }
        animationSetDelay -= Time.deltaTime;
        if (animationSetDelay <= 0)
        {
            var randomNum = Random.Range(0, 20);
            if ((randomNum <= 2) && distance < 10f)
            {
                string soundName = "Crow call " + randomNum;
                ManagerRoot.Sound.PlaySfx(soundName, distance / 15f);
            }
            animationSetDelay = Random.Range(1.5f, 2f);

            int random = Random.Range(0, 4);
            
            if (random == 0)
            {
                animator.Play("CrowEat");
            }
            else if (random < 3)
            {
                animator.Play("CrowIdle");
            }
            else
            {
                currentState = CrowState.Jumping;
            }
        }
    }

    private void JumpState(float distance)
    {
        if (distance < 6f) currentState = CrowState.Fleeing;

        //transform.DORotate(new Vector3(0, 0, Random.Range(0, 360)), 0.5f);
        
        float direction = Random.Range(0, 2) == 0 ? -1f : 1f;
        spriteRenderer.flipX = direction > 0 ? true : false;
        
        circleShadow.DOScale(new Vector2(0.4f, .2f), 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            circleShadow.DOScale(new Vector2(1.5f, .75f), 0.25f).SetEase(Ease.OutBack);
        });
        
        transform.DOJump(transform.position + new Vector3(direction * 2f, 0.5f, 0f), 0.5f, 1, 0.5f);
        
        
        currentState = CrowState.Normal;
        animationSetDelay = Random.Range(1.5f, 2.5f);
    }
}

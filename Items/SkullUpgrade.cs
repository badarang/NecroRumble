using System.Collections;
using System.Collections.Generic;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class SkullUpgrade : ItemBase
{
    private bool isCollected = false;

    protected override void Start()
    {
        base.Start();
        SetValues(10f, 3f, 2f);
    }
    
    IEnumerator DestroyEvent()
    {
        yield return new WaitForSeconds(0.1f);
        ManagerRoot.Sound.PlaySfx("Error 2 Larger", 1f);
        
        yield return new WaitForSeconds(1.4f);
        
        ManagerRoot.Sound.PlaySfx("Impact Retro 2", .4f);
        ManagerRoot.Sound.PlaySfx("Heavy Game Punch 1", 1f);


        if (GameManager.Instance.MaxUndeadNum < GameManager.Instance.AttainableMaxUndeadNum)
        {
            GameManager.Instance.MaxUndeadNum++;
            var text = ManagerRoot.I18N.getValue("^text_popup_skull_Maxnum");
            GameUIManager.Instance.CreateNumberBalloon(text, transform.position, Color.white, 1.2f, 2.5f);
        }
        else if (ManagerRoot.Unit.CurCharacter == UnitManager.CharacterType.CutePlayer)
        {
            //TODO: 언데드 힐 실제로 강화
            Player.CutePlayerBonusHeal += Statics.CutePlayerHealGrowth;
            var text = ManagerRoot.I18N.getValue("^text_popup_skull_Heal");
            GameUIManager.Instance.CreateNumberBalloon(text, transform.position, new Color32(0, 200, 0, 255), 1f, 2.5f);
        }
        
        //게임 오브젝트 삭제
        gameObject.SetActive(false);
        ManagerRoot.Resource.Release(gameObject);
    }
    
    protected override void HandleCollectEvent()
    {
        if (isCollected) return;
        isCollected = true;

        ManagerRoot.Sound.PlaySfx("Fantasy click sound 2 Larger", 1f);
        
        GameObject effectGO = Resources.Load<GameObject>("Prefabs/Effects/VFXSkullUpgradeEffect");
        GameObject effect = ManagerRoot.Resource.Instantiate(effectGO, transform.position, Quaternion.identity);
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0);
        StartCoroutine(DestroyEvent());
    }
}

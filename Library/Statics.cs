using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public static class Statics
{
    //---------- AI ----------
    //언데드 감지 범위
    public static float NormalDetectRadius = 8f;
    public static float CampDetectRadius = 5f;
    //인간 감지 범위
    public static float HumanDetectRange = 10f;

    //---------- 네크로맨스 ----------
    //유닛 죽는 애니메이션 속도. (애니메이터 > 스테이트 스피드는 1로 고정하기 위함.)
    public static float UnitDyingSpeedMultiplier = .7f;
    //리바이브 완료까지 걸리는 시간
    public static float RevivingTime = 1f;
    
    //---------- 플레이어 ----------
    public static float PlayerDamageReduceRatio = .5f;
    public static float PlayerInvincibleTime = .7f;
    //public static float CutePlayerHealRatio = 0.1f;
    //public static float CutePlayerHealIncreaseRatio = 0.01f;
    public static int CutePlayerFixedHealAmount = 60;
    public static int CutePlayerHealGrowth = 85;
    
    public static float CutePlayerHpAdvantagePercentage = 50f;
    public static float CutePlayerAtkAdvantagePercentage = 50f;
    public static int CutePlayerMaxUndeadNumLimit = 6;
    public static int CutePlayerMaxHp = 200;
    
    //---------- 유닛 ----------
    public static float MagicCircleDuration = 3f;
    public static float CorpseDestroyTime = 20f;
    public static Color undeadShadowColor = new Color32(76, 69, 255, 40);
    public static Color humanShadowColor = new Color32(200, 50, 10, 40);

    //---------- 아이템 ----------
    public static float HealingPotionDropPercent = -1f;

    //---------- 포탈 ----------
    public static Vector2 PortalPosition = Vector2.zero;

    //---------- 컨트롤러 ----------
    public static float AimAssistDegree = 30f;

    //---------- 인게임 ------------
    public static float GameClearTime = 20;

    //---------- 난이도 ------------
    public static int Difficulty_1_EnemyAtkModValue = -20;
    public static int Difficulty_3_EliteEnemyHpModValue = 30;
    public static int Difficulty_4_EnemyHpModValue = 20;
    public static int Difficulty_5_EnemyMoveSpeedModValue = 20;
    
    //---------- UI ------------

    public static float MultishotAdditionalRange = 6f;
}

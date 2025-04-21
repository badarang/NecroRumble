# NecroRumble - 탑다운 전략 로그라이크 게임
![1](https://github.com/user-attachments/assets/ec2aa2bb-3e4e-4cf1-962b-0f8e6f995623)

## 프로젝트 개요
NecroRumble은 크래프톤 정글 게임랩에서 5인 1팀으로 3개월간 개발한 탑다운 전략 로그라이크 게임입니다. 플레이어는 네크로맨서가 되어 시체를 부활시켜 언데드 군단을 이끌고, 적들을 물리치며 생존하는 것이 목표입니다.

## 주요 기능

### 1. 네크로맨서 시스템
- 시체 부활 시스템: 적을 처치한 후 시체를 부활시켜 언데드 군단을 구성
- 스킬 강화: 상자 발견 혹은 시체를 부활시킬 때마다 얻는 경험치로 능력 업그레이드
  ![4](https://github.com/user-attachments/assets/016b17fc-ac14-4553-a6bb-e0120e480959)

### 2. 유닛 시스템
- 기본 유닛 구조:
  ```csharp
  public partial class Unit : MonoBehaviour, IDamageable, IAttacker
  {
      [SerializeField] private Faction _currentFaction;
      private PairUnitData _humanData;
      private PairUnitData _undeadData;
      protected FeedbackController _feedback;
      protected Animator _humanAnimator;
      protected Animator _undeadAnimator;
      // ...
  }
  ```
  - `IDamageable`, `IAttacker` 인터페이스 구현으로 확장성 확보
  - 인간/언데드 형태의 이중 데이터 구조
  - Feedback Controller를 통한 시각적 효과 관리(넉백, 기절 등)

- 유닛 타입별 구현:
  - 기본 유닛:
    - 검사(SwordMan): 
      ```csharp
      public class SwordManUnit : Unit
      {
          public bool isGiantUnit { get; set; } = false;
          public bool isHaveFuryHeart { get; set; } = true;
          // 거대화 및 분노 상태 관리
      }
      ```
    - 궁수(ArcherMan):
      ```csharp
      public class ArcherManUnit : Unit
      {
          private float _rapidShotCount = 0;
          public float ProjectileSpeedMultiplier => 
              Mathf.Lerp(1, instanceStats.FinalAttackPerSec, 0.5f);
          // 멀티샷 및 특수 화살 시스템
      }
      ```
    - 암살자(Assassin):
      ```csharp
      public class AssassinUnit : Unit
      {
          private Coroutine ghostStepLoopCo;
          private Coroutine ghostStepCo;
          bool isGhostStep = false;
          // 은신 및 기습 공격 시스템
      }
      ```

  - 엘리트 유닛 시스템:
    - 기마병(HorseMan):
      ```csharp
      public class HorseManUnit : Unit
      {
          private float _chargePower = 20f;
          private bool _isCharging = false;
          // 돌진 및 충격파 시스템
      }
      ```
    - 성녀/서큐버스(PriestSuccubus):
      ```csharp
      public class PriestSuccubusUnit : Unit
      {
          private float _healRadius = 5f;
          private float _charmDuration = 3f;
          // 광역 힐링 및 매혹 시스템
      }
      ```
    - 쌍검 암살자(DualBladeAssassin):
      
      ![2](https://github.com/user-attachments/assets/f1d7443c-324a-46d6-84b1-3c9b1641e53a)
      ```csharp
      public class DualBladeAssassinUnit : Unit
      {
          private Vector3 _bladeDanceCenter;
          private float _bladeDanceRadius = 3f;
          // 순간이동 및 칼춤 공격 시스템
      }
      ```
    - 마법사(FlameMagician):
      ```csharp
      public class FlameMagicianUnit : Unit
      {
          private GameObject _fireMagicCircle;
          private float _specialAttackCoolTime = 5f;
          private float _specialAttackCastingSpeed = 1f;
          // 마법진 및 화염구 시스템
      }
      ```
    - 골렘(Golem):
      
      ![3](https://github.com/user-attachments/assets/0674234b-6fce-4028-8324-b22504e067f3)
      ```csharp
      public class GolemUnit : Unit
      {
          private float _jumpPower = 15f;
          private float _slamRadius = 4f;
          // 점프 및 충격파 시스템
      }
      ```
    - 날개 전사(FlightSword):
      ```csharp
      public class FlightSwordUnit : Unit
      {
          private List<GameObject> _summonedSwords = new List<GameObject>();
          private float _swordSummonRadius = 6f;
          // 마법 검 소환 시스템
      }
      ```
      
- 유닛 AI 시스템:
  - Behavior Tree 기반 AI 구현
  - 유닛 타입별 특화된 행동 패턴
  - 상황별 전략적 의사결정
  ```csharp
  protected override void InitAI()
  {
      base.InitAI();
      if(CurrentFaction == Faction.Undead){
          _addMoveWeightBehaviorUndead = new CustomMoveWeightBehavior_U();
      }
  }
  ```

- 상태 관리 시스템:
  - 공포, 중독, 느려짐 등 다양한 상태 효과
  - 상태 효과의 중첩 및 지속 시간 관리
  - 상태 효과에 따른 행동 패턴 변경

### 3. 스킬 시스템
- 이벤트 기반 스킬 시스템:
  - `SkillBase` 추상 클래스를 상속받아 모든 스킬 구현
  - `OnSkillAttained`, `OnSkillUpgrade`, `OnBattleStart`, `OnBattleEnd` 등의 생명주기 메서드 제공
  - 이벤트 시스템을 통한 유연한 스킬 활성화/비활성화
    ```csharp
    ManagerRoot.Event.onUnitSpawn += OnUnitSpawn;
    ManagerRoot.Event.onUnitDeath += OnUnitDeath;
    ```

- 스킬 효과 계산 시스템:
  - 스탯 수정자(StatModifier) 시스템 구현
  - 곱연산과 합연산을 구분하여 스탯 계산
    ```csharp
    StatModifier mod = new StatModifier(
        StatType.MaxHp, 
        "BlackGrimoire", 
        (-1)*_data.downUnitHp, 
        StatModifierType.FinalPercentage, 
        true
    );
    ```

- 주요 스킬 구현:
  - ImmortalWarrior:
    - 검사 유닛 스폰/사망 시 네크로맨서 쿨타임 감소
    - 스킬 레벨에 따른 추가 효과 구현
  - LapelOfNightmare:
    - 공포 상태 효과 구현
    - 공격자/피격자 간 상호작용 처리
  - BlackGrimoire:
    - 언데드 유닛 최대 수 동적 조절
    - 유닛 스탯 수정자 시스템 활용
  - FearOfBlood:
    - 공포 상태의 피해량 증폭
    - 상태 효과 기반 추가 데미지 계산

- 스킬 데이터 관리:
  - ScriptableObject를 활용한 스킬 데이터 관리
  - 설정 파일 기반 밸런스 조정
  - 스킬 레벨별 효과 차등화

### 4. 전투 시스템
- 실시간 전투: 유닛들의 AI 기반 자동 전투
- 특수 효과:
  - 타겟 우선순위: 암살자 유닛의 경우 궁수 유닛을 우선 공격
  - 공포 상태: 적 유닛의 이동 제한
  - 중독 효과: 지속적인 피해
  - 느려짐 효과: 이동 속도 감소
  - 방패 효과: 피해 감소

### 5. 시각적 효과
- 애니메이션 시스템:
  - 유닛별 고유 애니메이션
  - 부활 시퀀스
  - 공격 및 피해 효과
- 피드백 시스템:
  - 피해 표시
  - 상태 효과 시각화
  - 특수 효과 표시

## 기술적 특징

### 1. 객체지향 설계
- 유닛 시스템의 상속 구조를 통한 확장성
- 컴포넌트 기반 설계로 유연한 기능 추가
- 이벤트 시스템을 통한 느슨한 결합

### 2. AI 시스템
- Behavior Tree를 활용한 유닛 AI 구현
- 상황별 전략적 의사결정
- 유닛 타입별 특화된 행동 패턴

### 3. 확장성
- 모듈화된 스킬 시스템
- 데이터 기반 설계로 새로운 유닛/스킬 추가 용이
- 설정 파일을 통한 밸런스 조정 용이성

## 개발 기여
- 유닛 시스템 설계 및 구현
- 스킬 시스템 개발
- 전투 시스템 최적화
- AI 시스템 개선
- 시각적 효과 구현

## 기술 스택
- Unity
- C#
- Behavior Tree
- DOTween

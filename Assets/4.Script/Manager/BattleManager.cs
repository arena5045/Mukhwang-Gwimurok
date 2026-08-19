using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance { get; private set; }

    public Monster_So test_so;

    public PlayerSetInfo currentPlayerInfo;
    public MonsterSetInfo currentMonsterInfo;

    public BattleUiManager buiManager;

    // 현재 전투에서 사용하는 플레이어 스킬 상태
    private readonly List<BattleSkillState> playerSkillStates = new();

    // 전투 중 실제로 시작된 행동 수만 기록한다. 다단히트의 개별 타격 수와는 구분하며,
    // 외부에서는 결과만 확인할 수 있도록 setter를 BattleManager 내부로 제한한다.
    public int PlayerActionCount { get; private set; }
    public int MonsterActionCount { get; private set; }

    //기습받음
    bool surprise = false;

    // 자동전투 코루틴의 단일 실행 여부를 추적한다. 버튼을 빠르게 반복 호출하거나
    // 여러 진입 경로가 겹쳐도 피해·로그·보상이 중복 처리되지 않게 한다.
    private Coroutine autoBattleCoroutine;

    public class MonsterSetInfo
    {
        public string name;
        public Sprite sprite;
        public int maxHp;
        public int currentHp;

        public float atk;
        public float buffatk=0f;
        public float multatk=1f;

        public float def;
        public float buffdef=0f;
        public float multdef=1f;

        public float speed;
        public float buffspeed=0f;
        public float multspeed=1f;

        public float imgsize;

        public float run_pro;
        public float nego_pro;

        public int reward_gold;
        public int reward_soul;
        public MonsterSetInfo(Monster_So monster_data)
        {
            name = monster_data.monsterName;
            sprite = monster_data.monsterSprite;
            imgsize = monster_data.scaleModifier;
            maxHp = monster_data.maxHp;
            currentHp = monster_data.maxHp;
            atk = monster_data.attackPower;
            def = monster_data.defencePower;
            speed = monster_data.attackSpeed;

            run_pro = monster_data.runProbability;
            nego_pro = monster_data.negoProbability;

            reward_gold = monster_data.gold_drop;
            reward_soul = monster_data.soul_drop;
        }
    }

    public class PlayerSetInfo
    {
        public int maxhp;
        public int currentHp;

        public int maxmp;
        public int currentmp;

        public float addmg;
        public float buffad;
        public float multad;

        public float apdmg;
        public float buffap;
        public float multap;

        public float defense;
        public float buffdef;
        public float multdef;

        public float speed;
        public float buffspeed;
        public float multspeed;

        public PlayerSetInfo(PlayerStats stats)
        {
            maxhp = stats.MaxHp;
            currentHp = stats.MaxHp; // 전투 시작 시 풀피로 설정

            maxmp = stats.MaxMp;
            currentmp = stats.MaxMp; // 전투 시작 시 풀마나로 설정

            addmg = stats.TotalAdAttack;
            buffad = 0;   // 버프는 전투 시작 시 0에서 시작
            multad = 1f;  // 곱연산은 1배에서 시작

            apdmg = stats.TotalApAttack;
            buffap = 0;
            multap = 1f;

            defense = stats.TotalDefense;
            buffdef = 0;
            multdef = 1f;

            speed = stats.TotalSpeed;
            buffspeed = 0;
            multspeed = 1f;
        }
    }

    private void Awake()
    {
        // BattleManager는 BattleUiManager를 직접 참조하므로 Ingame 씬과 수명을 같이한다.
        // 씬 전환 후에도 유지하면 새 UI가 아니라 파괴된 이전 UI를 참조하게 되므로
        // DontDestroyOnLoad를 사용하지 않고, 같은 씬 안의 중복 인스턴스만 제거한다.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 전투 UI 참조를 가진 매니저이므로 현재 게임 씬과 수명을 함께한다.
        instance = this;

        if(buiManager ==null)
        {
            buiManager = GetComponent<BattleUiManager>();
        }
    }

    private void OnDestroy()
    {
        // 다른 중복 객체가 파괴되는 상황에서 정상 인스턴스의 static 참조까지
        // 지우지 않도록 현재 소유자인지 확인한 뒤 해제한다.
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// 새 런 시작 시 이전 전투의 런타임 상태를 폐기한다.
    /// 진행 중인 턴 코루틴, 기습 여부, 플레이어·몬스터 전투 복사본을 모두 비워
    /// 이전 전투가 새 런의 UI나 보상 처리에 뒤늦게 개입하지 못하게 한다.
    /// </summary>
    public void ResetForNewRun()
    {
        // 이전 런의 전투 코루틴이 다음 런의 피해나 보상 처리에 개입하지 않게 한다.
        StopAllCoroutines();
        autoBattleCoroutine = null;
        surprise = false;
        PlayerActionCount = 0;
        MonsterActionCount = 0;
        currentPlayerInfo = null;
        currentMonsterInfo = null;


    }


    public void BattleSetting(Monster_So monster_data)
    {
        // 행동 카운트는 런 전체 누적값이 아니라 현재 전투 단위의 런타임 값이다.
        PlayerActionCount = 0;
        MonsterActionCount = 0;

        // 이전 전투의 스킬 상태를 버리고 이번 전투용 상태를 새로 만든다.
        playerSkillStates.Clear();


        foreach (OwnedSkill ownedSkill in GameManager.Instance.Context.player.skills)
        {
            playerSkillStates.Add(new BattleSkillState(ownedSkill));
        }


        // 새로운 전투용 데이터 생성 (자동으로 값 할당됨)
        currentPlayerInfo = new PlayerSetInfo(GameManager.Instance.Context.player.stats);

        // 아직 일차별 난이도 규칙은 사용하지 않는다. MonsterSetInfo가
        // Monster_So의 원본 전투 능력치를 그대로 복사하도록 유지한다.
        currentMonsterInfo = new MonsterSetInfo(monster_data);

        // UI 매니저에게 요청
        buiManager.BattleUiSetting(currentMonsterInfo);

    }

    /// <summary>
    /// 자동전투를 시작하는 유일한 진입점이다.
    /// 이미 실행 중이라면 새 코루틴을 만들지 않아 피해와 보상이 중복되지 않게 한다.
    /// </summary>
    private void StartAutoBattle(float waittime = 0f)
    {
        if (autoBattleCoroutine != null)
        {
            Debug.LogWarning("[BattleManager] 이미 자동전투가 진행 중이므로 중복 실행을 무시합니다.");
            return;
        }

        autoBattleCoroutine = StartCoroutine(AutoBattleRoutine(waittime));
    }

    IEnumerator AutoBattleRoutine(float waittime = 0f)
    {
        // 1. 전투 시작 알림
        //buiManager.AddLog($"{monsterData.monsterName}이(가) 나타났다!");
        //yield return new WaitForSeconds(1.0f);
        Debug.Log("배틀 시작");

        yield return new WaitForSeconds(waittime);

        // 2. 한 쪽이 죽을 때까지 반복
        while (currentPlayerInfo.currentHp > 0 && currentMonsterInfo.currentHp > 0)
        {
            // [기습 여부 우선 판정]
            // 기습(surprise) 상태면 무조건 몬스터가 먼저, 아니면 스피드 비교
            if (surprise)
            {
                buiManager.AddLog("<color=red>기습을 당했다!</color>"); // 기습 알림 로그
                surprise = false; // 한 번 효과를 봤으니 바로 해제
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(MonsterTurn());
                if (currentPlayerInfo.currentHp <= 0) break;

                yield return StartCoroutine(PlayerTurn());
            }
            else
            {
                // 일반적인 선후공 결정 (스피드 비교)
                bool isPlayerFaster = currentPlayerInfo.speed >= currentMonsterInfo.speed;

                if (isPlayerFaster)
                {
                    yield return StartCoroutine(PlayerTurn());
                    if (currentMonsterInfo.currentHp <= 0) break;

                    yield return StartCoroutine(MonsterTurn());
                }
                else
                {
                    yield return StartCoroutine(MonsterTurn());
                    if (currentPlayerInfo.currentHp <= 0) break;

                    yield return StartCoroutine(PlayerTurn());
                }
            }
            // 한 턴이 끝나고 잠시 대기 (가독성)
            buiManager.AddLog("------------------------");
            yield return new WaitForSeconds(0.8f);
        }

        // 3. 결과 처리
        // 종료와 보상 처리가 끝날 때까지 이 자동전투 코루틴이 실행 중인 것으로 유지한다.
        // 별도 코루틴으로 분리하지 않아 다음 전투가 보상 처리와 겹치는 상황을 막는다.
        yield return FinishBattle();
        Debug.Log("배틀 종료");
        autoBattleCoroutine = null;
    }

    private IEnumerator FinishBattle()
    {
        //이긴거
       if(currentPlayerInfo.currentHp>0)
        {
            buiManager.AddLog("전투에서 승리했다!");
            yield return new WaitForSeconds(0.5f);
            int reward_gold =Mathf.RoundToInt(currentMonsterInfo.reward_gold * Random.Range(0.8f, 1.2f));
            int reward_soul = Mathf.RoundToInt(currentMonsterInfo.reward_soul * Random.Range(0.8f, 1.2f));

            GameManager.Instance.ChangeGold(reward_gold);
            GameManager.Instance.ChangeSoul(reward_soul);

            if(reward_gold >0)
            {
                buiManager.AddLog($"금화 {reward_gold}개를 얻었다.");
                yield return new WaitForSeconds(0.5f);
            }
            if (reward_soul > 0)
            {
                buiManager.AddLog($"혼백 {reward_soul}개를 수급했다.");
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.5f);
            buiManager.BattleEndUi_Open();
        }
        //진거
        else
        {
            buiManager.AddLog("당신은 쓰러지고 말았다...!");
            yield return new WaitForSeconds(0.5f);

            GameManager.Instance.isGameOver = true;
            buiManager.BattleEndUi_Open();
        }
    }

    private IEnumerator FinishBattle(float waittime)
    {
            yield return new WaitForSeconds(waittime);

            buiManager.BattleEndUi_Open();
    }

    IEnumerator PlayerTurn()
    {
        // 상대가 앞선 행동에서 이미 쓰러졌다면 행동 자체가 시작되지 않은 것이므로
        // 카운트를 올리거나 공격 로그를 출력하지 않는다.
        if (currentPlayerInfo == null || currentMonsterInfo == null ||
            currentPlayerInfo.currentHp <= 0 || currentMonsterInfo.currentHp <= 0)
        {
            yield break;
        }

        PlayerActionCount++;
        Debug.Log($"플레이어 행동 횟수 : {PlayerActionCount}");

        // 새 행동이 시작됐으므로 각 스킬의 행동 단위 발동 횟수를 초기화
        foreach (BattleSkillState state in playerSkillStates)
        {
            state.ResetAction();
        }


        // 1. 턴 시작시 스킬 실행타이밍
        List<BattleSkillState> turnStartSkills =
            GetTriggeredSkills(SkillTrigger.TurnStart);
        foreach (BattleSkillState state in turnStartSkills)
        {
            yield return ExecuteSkill(state, SkillTrigger.TurnStart);

            // 턴 시작 스킬로 적이 죽었다면 이후 공격은 하지 않음
            if (currentMonsterInfo.currentHp <= 0)
            {
                yield break;
            }

        }



        // 2. 기존 기본공격 타이밍 처리
        List<BattleSkillState> triggeredSkills =
            GetTriggeredSkills(SkillTrigger.BasicAttack);

        if (triggeredSkills.Count == 0)
        {
            yield return ExecutePlayerAttack("<color=#99FF99>당신</color>의 공격!", 1f, 1);

        }
        else
        {
            foreach (BattleSkillState state in triggeredSkills)
            {
                yield return ExecuteSkill(state, SkillTrigger.BasicAttack);

                if (currentMonsterInfo.currentHp <= 0)
                {
                    break;
                }
            }
        }
    }

    IEnumerator MonsterTurn()
    {
        // 플레이어가 이미 쓰러졌거나 몬스터가 행동할 수 없는 상태라면
        // 실제 행동이 아니므로 카운트에 포함하지 않는다.
        if (currentPlayerInfo == null || currentMonsterInfo == null ||
            currentPlayerInfo.currentHp <= 0 || currentMonsterInfo.currentHp <= 0)
        {
            yield break;
        }

        MonsterActionCount++;
        yield return ExecuteMonsterAttack($"<color=red>{currentMonsterInfo.name}</color>의 공격!", 1f, 1);
    }

    /// <summary>
    /// 플레이어와 몬스터가 함께 사용하는 방어력 기반 피해 공식이다.
    /// 공격력이 없거나 배율 적용 후 피해가 양수가 아니면 0을 허용하며,
    /// 양수 피해는 방어력 적용 및 반올림 후에도 최소 1을 보장한다.
    /// </summary>
    private int CalculateDamage(float attack, float defense, float damageMultiplier)
    {
        if (attack <= 0f)
        {
            return 0;
        }

        float rawDamage = attack * damageMultiplier;
        if (rawDamage <= 0f)
        {
            return 0;
        }

        float appliedDefense = Mathf.Max(0f, defense);
        float finalDamage = rawDamage / (1f + appliedDefense * 0.05f);
        return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
    }

    /// <summary>
    /// 플레이어 공격 한 행동을 실행한다. 행동 로그는 한 번만 출력하고,
    /// 각 타격마다 피해 계산·HP 반영·UI 갱신·피격 연출을 수행한다.
    /// </summary>
    private IEnumerator ExecutePlayerAttack(
        string actionName,
        float damageMultiplier,
        int hitCount,
        float beforeHitDelay = 0.5f,
        float afterAttackDelay = 0.8f,
        BattleSkillState sourceSkill = null,
        SkillChainContext chain = null)
    {
        buiManager.AddLog(actionName);
        yield return new WaitForSeconds(beforeHitDelay);

        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            float attack = (currentPlayerInfo.addmg + currentPlayerInfo.buffad) * currentPlayerInfo.multad;
            float defense = (currentMonsterInfo.def + currentMonsterInfo.buffdef) * currentMonsterInfo.multdef;
            int damage = CalculateDamage(attack, defense, damageMultiplier);

            currentMonsterInfo.currentHp = Mathf.Max(0, currentMonsterInfo.currentHp - damage);
            buiManager.AddLog($"<color=red>{currentMonsterInfo.name}</color>에게 <color=red>{damage}</color>의 피해를 입혔다!");
            buiManager.UpdateMonsterHP(currentMonsterInfo.currentHp);
            buiManager.MonsterDamageEffect();

            // 이 타격으로 적이 살아있다면 타격 후 스킬을 검사
            if (currentMonsterInfo.currentHp > 0)
            {
                // 일반 공격에서 시작된 타격이면 새 연쇄를 만든다.
                // 추가타가 만든 타격이면 기존 연쇄를 계속 사용한다.
                SkillChainContext hitChain =
                    chain ?? new SkillChainContext();

                yield return ExecuteAfterHitChain(
                    sourceSkill,
                    hitChain);
            }

            // 대상이 쓰러지면 다단히트의 남은 타격과 타격 사이 대기를 생략한다.
            if (currentMonsterInfo.currentHp <= 0)
            {
                break;
            }

            if (hitIndex < hitCount - 1)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        // 기본 공격 1타의 기존 전투 템포를 유지한다.
        yield return new WaitForSeconds(afterAttackDelay);
    }

    public IEnumerator ExecuteSkillAttack(
        string skillName,
        float damageMultiplier,
        int hitCount,
        SkillTrigger trigger,
        BattleSkillState sourceSkill,
        SkillChainContext chain)
    {
        bool isChainAttack =
            trigger == SkillTrigger.AfterHit;

        float beforeHitDelay = isChainAttack ? 0.15f : 0.5f;
        float afterAttackDelay = isChainAttack ? 0.1f : 0.8f;

        yield return ExecutePlayerAttack(
            $"<color=#99FF99>{skillName}</color> 발동!",
            damageMultiplier,
            hitCount,
            beforeHitDelay,
            afterAttackDelay,
            sourceSkill,
            chain);
    }


    /// <summary>
    /// 몬스터 공격 한 행동을 실행한다. 플레이어 공격과 같은 피해 공식과
    /// 다단히트 중단 규칙을 사용하되 기존 플레이어 HP UI와 공격 연출을 유지한다.
    /// </summary>
    private IEnumerator ExecuteMonsterAttack(string actionName, float damageMultiplier, int hitCount)
    {
        buiManager.AddLog(actionName);
        yield return new WaitForSeconds(0.5f);

        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            float attack = (currentMonsterInfo.atk + currentMonsterInfo.buffatk) * currentMonsterInfo.multatk;
            float defense = (currentPlayerInfo.defense + currentPlayerInfo.buffdef) * currentPlayerInfo.multdef;
            int damage = CalculateDamage(attack, defense, damageMultiplier);

            currentPlayerInfo.currentHp = Mathf.Max(0, currentPlayerInfo.currentHp - damage);
            buiManager.AddLog($"<color=#99FF99>당신</color>에게 <color=red>{damage}</color>의 피해를 입혔다!");
            GameUiManager.Instance.UpdatePlayerHPUI_battle(currentPlayerInfo.currentHp);
            buiManager.MonsterAttackEffect();

            // 플레이어가 쓰러졌다면 남은 타격을 실행하지 않아 로그와 연출도 중복되지 않는다.
            if (currentPlayerInfo.currentHp <= 0)
            {
                break;
            }

            if (hitIndex < hitCount - 1)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        yield return new WaitForSeconds(0.8f);
    }

    public void BattleStart(Monster_So monster_data)
    {
        BattleSetting(monster_data);
    }

    public void BattleBtn()
    {
        Debug.Log("배틀 할당");
        StartAutoBattle();
        buiManager.BattleBtnsOff();
    }

    public void NegoBtn()
    {
        float negovalue = Random.Range(0f, 100f);
        if(negovalue <= currentMonsterInfo.nego_pro)
        {
            buiManager.AddLog($"");
            buiManager.AddLog($"");
            buiManager.AddLog($"무의미한 전투를 회피 할 수 있었다!");
            StartCoroutine(FinishBattle(1f));
        }
        else
        {
            buiManager.AddLog($"말이 통하는 상태가 아닌 것 같다..! \n적이 더 날뛰기 시작했다!");
            EnemyBurst(new Vector3(1.2f, 1.2f, 1.2f));
            StartAutoBattle(1f);
        }

        buiManager.BattleBtnsOff();
    }

    public void RunBtn()
    {
        // 협상 확률(nego_pro)과 도주 확률(run_pro)은 몬스터 데이터에서 별도로 설정된다.
        // 도주 버튼은 반드시 run_pro만 사용해야 두 행동의 밸런스 값을 독립적으로 조정할 수 있다.
        float negovalue = Random.Range(0f, 100f);
        if (negovalue <= currentMonsterInfo.run_pro)
        {
            buiManager.AddLog($"");
            buiManager.AddLog($"");
            buiManager.AddLog($"도주에 성공했다..!");
            StartCoroutine(FinishBattle(1f));
        }
        else
        {
            buiManager.AddLog($"전투를 피할 순 없을 것 같다!");
            surprise = true;
            StartAutoBattle(1f);
        }

        buiManager.BattleBtnsOff();
    }

    public void EnemyBurst(Vector3 value)
    {
        //공 방 체 순으로 뻥튀기
        currentMonsterInfo.atk = currentMonsterInfo.atk * value.x;
        currentMonsterInfo.def = currentMonsterInfo.def * value.y;
        currentMonsterInfo.maxHp = Mathf.RoundToInt(currentMonsterInfo.maxHp * value.z);
        currentMonsterInfo.currentHp = currentMonsterInfo.maxHp;

        buiManager.BattleUiRefresh();
    }

    /// <summary>
    /// 현재 타이밍에서 조건을 만족한 스킬들을 찾는다.
    /// 조건 판정은 여기서 한 번만 수행한다.
    /// </summary>
    private List<BattleSkillState> GetTriggeredSkills(SkillTrigger trigger)
    {
        List<BattleSkillState> triggeredSkills = new();

        foreach (BattleSkillState state in playerSkillStates)
        {
            SkillData skill = state.Data;

            // 현재 발생한 타이밍의 스킬만 검사
            if (skill.trigger != trigger)
            {
                continue;
            }

            // 이 스킬의 행동당 최대 발동 횟수를 이미 채웠다면 제외
            if (!state.CanActivateThisAction())
            {
                continue;
            }

            SkillContext context = new SkillContext(this, state, trigger);

            // 레벨에 설정된 조건을 모두 검사
            if (!skill.CanActive(context))
            {
                continue;
            }

            // 조건을 통과한 시점에 발동 횟수를 먼저 예약
            state.ReserveActivation();

            triggeredSkills.Add(state);
        }

        return triggeredSkills;
    }

    private IEnumerator ExecuteSkill(
        BattleSkillState state,
        SkillTrigger trigger,
        BattleSkillState sourceSkill = null,
        SkillChainContext chain = null)
    {
        SkillData skill = state.Data;
        // 현재 스킬 레벨에 맞는 조건/효과 데이터를 가져온다.
        SkillLevelData levelData = skill.GetLevelData(state.Level);

        if (levelData == null)
        {
            yield break;
        }

        SkillContext context =
         new SkillContext(
             this,
             state,
             trigger,
             sourceSkill,
             chain);


        // 현재 레벨에 등록된 효과를 순서대로 실행
        foreach (SkillEffectBase effect in levelData.effects)
        {
            if (effect != null)
            {
                yield return effect.Execute(context);
            }
        }
        state.MarkActivated();

    }


    private IEnumerator ExecuteAfterHitChain(
    BattleSkillState sourceSkill,
    SkillChainContext chain)
    {
        foreach (BattleSkillState state in playerSkillStates)
        {
            SkillData skill = state.Data;

            if (skill.trigger != SkillTrigger.AfterHit)
            {
                continue;
            }

            // 이번 한 타격 연쇄에서 이미 등장한 스킬은 다시 등장 불가
            if (chain.HasUsed(state))
            {
                continue;
            }

            // 행동 전체의 최대 발동 횟수도 유지
            if (!state.CanActivateThisAction())
            {
                continue;
            }

            SkillContext context = new SkillContext(
                this,
                state,
                SkillTrigger.AfterHit,
                sourceSkill,
                chain);

            if (!skill.CanActive(context))
            {
                continue;
            }

            // 미리 전체를 예약하지 않고,
            // 실제로 발동할 스킬 하나만 그 순간 기록한다.
            chain.MarkUsed(state);
            state.ReserveActivation();

            yield return ExecuteSkill(
                state,
                SkillTrigger.AfterHit,
                sourceSkill,
                chain);

            if (currentMonsterInfo.currentHp <= 0)
            {
                yield break;
            }
        }
    }

}

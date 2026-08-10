using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance { get; private set; }

    public Monster_So test_so;
    [Header("전투 배율")]
    public float statMultiplier = 0.025f;

    public PlayerSetInfo currentPlayerInfo;
    public MonsterSetInfo currentMonsterInfo;

    public BattleUiManager buiManager;

    //기습받음
    bool surprise = false;

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
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        if(buiManager ==null)
        {
            buiManager = GetComponent<BattleUiManager>();
        }
    }


    public void BattleSetting(Monster_So monster_data)
    {
        // 새로운 전투용 데이터 생성 (자동으로 값 할당됨)
        currentPlayerInfo = new PlayerSetInfo(GameManager.Instance.Context.player.stats);

        //여기부터 적
        //배율계산
        float dmgCorrection = GameManager.Instance.Context.currentDay * statMultiplier;
        // 둘 중 큰 값을 선택 (최솟값 1 보장)
        dmgCorrection = Mathf.Max(1f, dmgCorrection);

        // 1. 배율이 적용된 실제 데이터 계산
        int finalHp = Mathf.RoundToInt(monster_data.maxHp * dmgCorrection);

        // 2. UI에 전달할 정보 객체 생성
        currentMonsterInfo = new MonsterSetInfo(monster_data);

        // 3. UI 매니저에게 요청
        buiManager.BattleUiSetting(currentMonsterInfo);

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
      StartCoroutine(FinishBattle());
        Debug.Log("배틀 종료");
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
        buiManager.AddLog($"<color=#99FF99>당신</color>의 공격!");
        yield return new WaitForSeconds(0.5f);
        // 데미지 계산 및 적용
        //int damage = CalculateDamage(playerAtk);
        int damage = Mathf.RoundToInt((currentPlayerInfo.addmg + currentPlayerInfo.buffad) * currentPlayerInfo.multad);

        currentMonsterInfo.currentHp = Mathf.Max(0, currentMonsterInfo.currentHp - damage);

        buiManager.AddLog($"<color=red>{currentMonsterInfo.name}</color>에게 <color=red>{damage}</color>의 피해를 입혔다!");

        // UI 업데이트 (DOTween 사용 권장)
        buiManager.UpdateMonsterHP(currentMonsterInfo.currentHp);
        buiManager.MonsterDamageEffect();

        yield return new WaitForSeconds(0.8f);
    }

    IEnumerator MonsterTurn()
    {
        buiManager.AddLog($"<color=red>{currentMonsterInfo.name}</color>의 공격!");
        yield return new WaitForSeconds(0.5f);
        // 데미지 계산 및 적용
        //int damage = CalculateDamage(playerAtk);
        int damage = Mathf.RoundToInt((currentMonsterInfo.atk + currentMonsterInfo.buffatk) * currentMonsterInfo.multatk);

        currentPlayerInfo.currentHp -= damage;

        buiManager.AddLog($"<color=#99FF99>당신</color>에게 <color=red>{damage}</color>의 피해를 입혔다!");
        // UI 업데이트
        GameUiManager.Instance.UpdatePlayerHPUI_battle(currentPlayerInfo.currentHp);
        buiManager.MonsterAttackEffect();

        yield return new WaitForSeconds(0.8f);
    }

    public void BattleStart(Monster_So monster_data)
    {
        BattleSetting(monster_data);
    }

    public void BattleBtn()
    {
        Debug.Log("배틀 할당");
        StartCoroutine(AutoBattleRoutine());
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
            StartCoroutine(AutoBattleRoutine(1f));
        }

        buiManager.BattleBtnsOff();
    }

    public void RunBtn()
    {
        float negovalue = Random.Range(0f, 100f);
        if (negovalue <= currentMonsterInfo.nego_pro)
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
            StartCoroutine(AutoBattleRoutine(1f));
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

}

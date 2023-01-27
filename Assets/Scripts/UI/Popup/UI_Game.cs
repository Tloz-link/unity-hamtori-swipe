using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class UI_Game : UI_Popup
{
    public enum GameObjects
    {
        BackGround,
        StarBG,  // 15점 이후 표시
        CloudBG,
        Moon,    // 100점 이후 표시

        // Header
        Score,
        Power,
        PauseButton,

        // Game Board
        GameBoard,
        ControlPad,
        Hamster,
        ArrowGroup1,
        ArrowGroup2,
        ArrowGroup3,
        ShootBallGroup,
        WaitBallGroup,
        BlockGroup,
        NuclearSkill,
        LeftSkill,
        RightSkill,
        NuclearEmpty,
        NuclearFull,
        NuclearFullStar,
        UITextGroup
    }

    enum Texts
    {
        ScoreText,
        HighScoreText,
        PowerUpCooltimeText,
        GlassesCooltimeText,
        NuclearStackText,
        PlusPowerText,
        BallCountText,
        NewRecordText
    }

    enum Images
    {
        PowerUpSkill,
        GlassesSkill,
        BackGround
    }

    GameManagerEX _game;
    StartData _startData;

    Queue<UI_Ball> _waitBalls = new Queue<UI_Ball>();
    Queue<UI_Ball> _shootBalls = new Queue<UI_Ball>();
    List<UI_Block> _blocks = new List<UI_Block>();
    List<UI_Item_Star> _items = new List<UI_Item_Star>();

    // 타겟팅 라인
    List<GameObject> _arrowStars = new List<GameObject>();
    GameObject _arrowMoon;

    #region 초기화
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Clear();
        _game = Managers.Game;
        _startData = Managers.Data.Start;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        // 해상도 별 UI 조정
        AdjustUIByResolution();

        GetObject((int)GameObjects.NuclearSkill).SetActive(false);
        GetObject((int)GameObjects.StarBG).SetActive(false);
        GetObject((int)GameObjects.Moon).SetActive(false);
        GetText((int)Texts.PlusPowerText).gameObject.SetActive(false);
        GetText((int)Texts.NewRecordText).gameObject.SetActive(false);

        Vector3 hamsterStartPos = GetObject((int)GameObjects.Hamster).transform.localPosition;
        hamsterStartPos.x = _game.HamsterPos.x;
        _game.HamsterPos = hamsterStartPos;
        GetObject((int)GameObjects.Hamster).transform.localPosition = _game.HamsterPos;
        GetObject((int)GameObjects.Hamster).GetOrAddComponent<UI_Spine>();
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPressed, Define.UIEvent.Pressed);
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPointerUp, Define.UIEvent.PointerUp);
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPointerDown, Define.UIEvent.PointerDown);
        GetImage((int)Images.PowerUpSkill).gameObject.BindEvent(OnPowerUpSkill);
        GetImage((int)Images.GlassesSkill).gameObject.BindEvent(OnGlassesSkill);
        GetObject((int)GameObjects.NuclearSkill).BindEvent(OnNuclearSkill);
        GetObject((int)GameObjects.PauseButton).BindEvent(OnPauseButton);

        // Ball 생성
        int count = Mathf.Min(_game.FullBallCount, Define.MAX_VISIBLE_BALL_COUNT);
        for (int i = 0; i < count; ++i)
        {
            UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>(GetObject((int)GameObjects.WaitBallGroup).transform);
            ball.SetInfo(GetObject((int)GameObjects.ControlPad).transform.localPosition, OnBallReachCallBack);
            _waitBalls.Enqueue(ball);
        }

        // BGM 실행
        if (_game.Score < 10000)
            Managers.Sound.Play(Define.Sound.Bgm, "gameBGM");
        else
            Managers.Sound.Play(Define.Sound.Bgm, "gameHiddenBGM");

        // UI 초기화
        RefreshUI();

        // 핵 게이지 초기화
        FillNuclearGauge(0.5f);

        return true;
    }

    void AdjustUIByResolution()
    {
        float scaleHeight = ((float)Screen.width / Screen.height) / ((float)9 / 16);
        Vector2 canvasSize;
        GetObject((int)GameObjects.CloudBG).SetActive(false);

        if (scaleHeight <= 1)
        {
            // 세로로 김
            canvasSize = new Vector2(1080, Screen.height / transform.localScale.x);
            float deltaY = Mathf.Abs((1080 * (Screen.height * 1080 / Screen.width)) - (1080 * 1920)) / 2 / 1080;
            float scoreDeltaY = GetObject((int)(GameObjects.PauseButton)).GetComponent<RectTransform>().sizeDelta.y;

            GetObject((int)(GameObjects.StarBG)).transform.localPosition += new Vector3(0, deltaY, 0);
            GetObject((int)(GameObjects.PauseButton)).transform.localPosition += new Vector3(0, deltaY, 0);
            GetObject((int)(GameObjects.Score)).transform.localPosition += new Vector3(0, deltaY, 0);
            if (deltaY > scoreDeltaY)
            {
                GetObject((int)(GameObjects.Score)).transform.localPosition -= new Vector3(0, scoreDeltaY, 0);
            }

            GetObject((int)(GameObjects.Power)).transform.localPosition -= new Vector3(0, deltaY * 0.4f, 0);
            GetObject((int)(GameObjects.GameBoard)).transform.localPosition -= new Vector3(0, deltaY * 0.4f, 0);
        }
        else
        {
            // 가로로 김
            canvasSize = new Vector2(2463, 1920);
            canvasSize.x = Math.Min(canvasSize.x, Screen.width / transform.localScale.x);
            float deltaX = Mathf.Abs((1920 * (canvasSize.x * 1920 / canvasSize.y)) - (1080 * 1920)) / 2 / 1920;

            GetObject((int)(GameObjects.PauseButton)).transform.localPosition += new Vector3(deltaX, 0, 0);
            GetObject((int)(GameObjects.RightSkill)).transform.localPosition += new Vector3(deltaX * 0.1f, 0, 0);
            GetObject((int)(GameObjects.LeftSkill)).transform.localPosition -= new Vector3(deltaX * 0.1f, 0, 0);
            GetObject((int)(GameObjects.Power)).transform.localPosition += new Vector3(deltaX * 0.2f, 0, 0);
            GetObject((int)(GameObjects.Score)).transform.localPosition -= new Vector3(deltaX * 0.2f, 0, 0);

            if (deltaX >= 480)
            {
                GetObject((int)GameObjects.CloudBG).SetActive(true);
            }
        }

        GetObject((int)GameObjects.BackGround).GetComponent<RectTransform>().sizeDelta = canvasSize;
    }
    #endregion

    #region 게임판 업데이트
    IEnumerator RefreshBoard(float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnBlockAndItem();

        yield return new WaitForSeconds(0.3f);

        foreach (var block in _blocks)
            block.MoveNext();

        foreach (var item in _items)
            item.MoveNext();

        yield return new WaitForSeconds(0.2f);

        if (CheckGameOver() == true)
            yield break;

        // 별을 획득했으면 텍스트 출력
        if (_createBallCount > 0)
        {
            UI_Text text = Managers.UI.makeSubItem<UI_Text>(GetObject((int)GameObjects.UITextGroup).transform);
            text.transform.position = GetText((int)Texts.BallCountText).transform.position;
            text.SetInfo($"+{_createBallCount}", 50f);
        }

        // 10000점이 됐으면 히든 브금 온
        if (_game.Score == 10000)
            Managers.Sound.Play(Define.Sound.Bgm, "gameHiddenBGM");

        // 현재 볼 개수 출력
        _currentBall = _game.FullBallCount;
        RefreshUI();

        GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterIdle);
        _game.State = GameState.idle;

        SaveGame();
    }

    void SpawnBlockAndItem()
    {
        int count = UnityEngine.Random.Range(4, 7);
        if (_game.Score < 5)
            count = _game.Score + 1;

        List<int> spawnList = new List<int>();

        for (int i = 0; i < 7; ++i)
            spawnList.Add(i);

        for (int i = 0; i < count - 1; ++i)
        {
            int rand = UnityEngine.Random.Range(0, spawnList.Count);
            BlockInfo info = new BlockInfo
            {
                x = spawnList[rand],
                y = 0,
                hp = _game.Score
            };
            UI_Block block = Managers.UI.makeSubItem<UI_Block>(GetObject((int)GameObjects.BlockGroup).transform);
            block.SetInfo(info, OnDestroyBlock);
            _blocks.Add(block);
            spawnList.RemoveAt(rand);
        }

        ItemInfo itemInfo = new ItemInfo
        {
            x = spawnList[0],
            y = 0,
        };
        UI_Item_Star star = Managers.UI.makeSubItem<UI_Item_Star>(GetObject((int)GameObjects.BlockGroup).transform);
        star.SetInfo(itemInfo, OnCollisionStar);
        _items.Add(star);
    }

    bool CheckGameOver()
    {
        foreach (var block in _blocks)
        {
            BlockInfo info = block.GetInfo();
            if (info.y >= 8)
            {
                UI_GameOver ui = Managers.UI.ShowPopupUI<UI_GameOver>();
                ui.SetInfo(GetObject((int)GameObjects.Hamster).transform.position, () =>
                {
                    Clear();
                    Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

                    Managers.UI.ClosePopupUI(ui);
                    Managers.UI.ClosePopupUI(this);

                    Managers.Game.Init();
                    Managers.UI.ShowPopupUI<UI_Game>().NewGame();
                });
                GetObject((int)GameObjects.Hamster).SetActive(false);

                Managers.Sound.Play(Define.Sound.Effect, "gameover");
                Managers.Game.Init();
                Managers.Game.SaveGame();
                return true;
            }
        }
        return false;
    }
    #endregion

    #region 컨트롤러
    void OnPadPointerDown()
    {
        if (_game.State != GameState.idle)
            return;

        Managers.Sound.Play(Define.Sound.Effect, "target");
    }

    void OnPadPressed()
    {
        if (_game.State != GameState.idle)
            return;

        ClearArrow();

        Vector3 mousePos = Input.mousePosition;
        mousePos -= GetObject((int)GameObjects.GameBoard).transform.position;
        mousePos /= transform.localScale.x;

        Vector3 dir;
        if (CalcShootDir(out dir, mousePos) == false)
            return;

        // 타켓 화살표 생성
        GenerateArrow(dir);
    }

    void OnPadPointerUp()
    {
        if (_game.State != GameState.idle)
            return;

        ClearArrow();

        // 블럭 애니메이션 초기화
        foreach (var block in _blocks)
            block.PlayAnimation(Managers.Data.Spine.blockIdle);

        Vector3 mousePos = Input.mousePosition;
        mousePos -= GetObject((int)GameObjects.GameBoard).transform.position;
        mousePos /= transform.localScale.x;

        Vector3 dir;
        if (CalcShootDir(out dir, mousePos) == false)
            return;

        _game.ShootDir = dir;
        _game.State = GameState.shoot;
        SaveGame();

        // 공 발사
        StartCoroutine(ShootBalls());
    }

    public bool CalcShootDir(out Vector3 dir, Vector3 mousePos)
    {
        Vector2 padSize = GetObject((int)GameObjects.ControlPad).GetComponent<RectTransform>().sizeDelta;
        float upLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.y - 10;
        float leftLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.x - (padSize.x / 2) + 10;
        float rightLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.x + (padSize.x / 2) - 10;

        float ratio = (mousePos.x - leftLimit) / (padSize.x - 2 * 10);
        float angle = 10f + (160f * ratio);
        angle = Math.Clamp(angle, 10f, 170f);

        dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        if (mousePos.x < leftLimit || mousePos.x > rightLimit || mousePos.y > upLimit)
        {
            GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterIdle);
            return false;
        }
        return true;
    }

    public void ClearArrow()
    {
        Init();

        foreach (GameObject star in _arrowStars)
        {
            Color color = star.GetComponent<Image>().color;
            color.a = 1f;
            star.GetComponent<Image>().color = color;
            Managers.Resource.Destroy(star);
        }
        _arrowStars.Clear();

        if (_arrowMoon != null)
        {
            Managers.Resource.Destroy(_arrowMoon);
            _arrowMoon = null;
        }
    }

    public void GenerateArrow(Vector3 dir)
    {
        GameObject hamster = GetObject((int)GameObjects.Hamster);
        hamster.GetComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterCharge);
        RaycastHit2D hit = Physics2D.CircleCast(hamster.transform.position, 50 * 0.8f * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
        Vector3 src = hamster.transform.position;
        Vector3 dest = hit.centroid;

        for (int i = 0; i < _game.LineCount; ++i)
        {
            CreateOneLine(src, dest, dir, i);
            if (hit.transform.tag == "Floor" || i == _game.LineCount - 1)
                break;

            src = dest;
            dir = Vector3.Reflect(dir, hit.normal);
            dir = Utils.ClampDir(dir);
            hit = Physics2D.CircleCast(src + dir, 50 * 0.8f * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
            dest = hit.centroid;
        }

        // 화살표에 닿은 블럭 공포 애니메이션
        UI_Block target = hit.collider.GetComponent<UI_Block>();
        if (target != null)
            target.PlayAnimation(Managers.Data.Spine.blockTarget);

        foreach (var block in _blocks)
        {
            if (target == block)
                continue;
            block.PlayAnimation(Managers.Data.Spine.blockIdle);
        }

        // 마지막 별을 없에고 달로 변경
        GameObject lastStar = _arrowStars[_arrowStars.Count - 1];
        _arrowMoon = Managers.Resource.Instantiate("Contents/ArrowMoon");

        _arrowMoon.transform.SetParent(lastStar.transform.parent);
        _arrowMoon.transform.position = lastStar.transform.position;
        _arrowMoon.transform.localRotation = lastStar.transform.localRotation;
        _arrowMoon.transform.localScale = lastStar.transform.localScale;

        _arrowStars.RemoveAt(_arrowStars.Count - 1);
        Managers.Resource.Destroy(lastStar);
    }

    void CreateOneLine(Vector3 src, Vector3 dest, Vector3 dir, int index)
    {
        GameObject arrowGroup = GetObject((int)GameObjects.ArrowGroup1 + index);
        arrowGroup.transform.position = src;

        float dist = (dest - src).magnitude / transform.localScale.x;
        int need = (int)dist / 40;

        for (int i = 0; i <= need; ++i)
        {
            GameObject star = Managers.Resource.Instantiate("Contents/ArrowStar", arrowGroup.transform);
            star.transform.localPosition = new Vector3(0, dist - (need - i) * 40, 0);
            star.transform.localRotation = Quaternion.identity;
            star.transform.localScale = new Vector3(1f, 1f, 1f);

            if (i <= 2 && index == 0)
            {
                Color color = star.GetComponent<Image>().color;
                color.a = 0.4f;
                star.GetComponent<Image>().color = color;
            }
            _arrowStars.Add(star);
        }

        float angle = 360 - Quaternion.FromToRotation(Vector3.left, dir).eulerAngles.z;
        arrowGroup.transform.rotation = Quaternion.AngleAxis(90 - angle, Vector3.forward);
    }
    #endregion

    #region 발사
    private float _tick = 0f;
    private bool _shoot = false;
    private void FixedUpdate()
    {
        if (_shoot == false)
            return;

        _tick += Time.deltaTime;
        if (_tick >= Define.SHOOT_INTERVAL)
        {
            _tick = 0f;

            if (_loadedBalls.Count <= 0)
            {
                GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>().PlayAnimationForce(Managers.Data.Spine.hamsterWait);
                _loadedBalls.Clear();
                _shoot = false;
                return;
            }

            Managers.Sound.Play(Define.Sound.Effect, "shoot");

            UI_Ball ball = _loadedBalls.Pop();
            ball.gameObject.SetActive(true);
            ball.transform.SetParent(GetObject((int)GameObjects.ShootBallGroup).transform);
            ball.transform.localPosition = GetObject((int)GameObjects.Hamster).transform.localPosition;
            if (_game.BallDamage != 1)
                ball.ChangeSkin("B");
            ball.Shoot(GetObject((int)GameObjects.GameBoard), _game.ShootDir, transform.localScale.x);
            _currentBall -= ball.Attack;
            RefreshUI();
            _shootBalls.Enqueue(ball);

            GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>().PlayAnimationForce(Managers.Data.Spine.hamsterShoot);
        }
    }

    int _returnBallCount = 0;
    int _createBallCount = 0;
    IEnumerator ShootBalls(bool load = false)
    {
        if (load == true)
            yield return new WaitForSeconds(0.5f);

        // 발사할 공 로딩
        // 공은 항상 최대 100개로 고정, 공이 100개를 넘어가면 초과량을 100개의 공이 나눠가짐
        LoadBalls();

        _returnBallCount = 0;
        _createBallCount = 0;
        _shoot = true;
    }

    Stack<UI_Ball> _loadedBalls = new Stack<UI_Ball>();
    void LoadBalls()
    {
        _loadedBalls.Clear();

        int remainAttack = _game.FullBallCount;
        for (int i = 0; i < Define.MAX_BALL_COUNT; ++i)
        {
            if (i >= _game.FullBallCount)
                break;

            UI_Ball ball;
            if (_waitBalls.Count > 0)
                ball = _waitBalls.Dequeue();
            else
            {
                ball = Managers.UI.makeSubItem<UI_Ball>(GetObject((int)GameObjects.ShootBallGroup).transform);
                ball.gameObject.SetActive(false);
                ball.SetInfo(GetObject((int)GameObjects.ControlPad).transform.localPosition, OnBallReachCallBack);
            }

            int attack = 1;
            if (Define.MAX_BALL_COUNT < _game.FullBallCount)
            {
                int remainBall = Define.MAX_BALL_COUNT - i;
                int ratio = _game.FullBallCount / Define.MAX_BALL_COUNT;
                if (remainBall == 1)
                    attack = remainAttack;
                else
                    attack = (remainBall < remainAttack / ratio) ? ratio + 1 : ratio;
            }

            ball.Attack = attack;
            remainAttack -= attack;
            _loadedBalls.Push(ball);
        }
    }
    #endregion

    #region 발사 결과
    // 별 먹고 생성된 공
    void OnBallCreateCallBack(UI_Ball ball)
    {
        _createBallCount++;
        UI_Text text = Managers.UI.makeSubItem<UI_Text>(GetObject((int)GameObjects.UITextGroup).transform);
        text.transform.localPosition = ball.transform.localPosition;
        text.SetInfo("+ZZI", 100f);

        Managers.Sound.Play(Define.Sound.Effect, "addBall");

        CountReachedBall(ball);
    }

    // 발사 후 돌아온 공
    void OnBallReachCallBack(UI_Ball ball)
    {
        _returnBallCount++;
        if (_returnBallCount == 1)
        {
            Vector3 hamDest = _game.HamsterPos;
            hamDest.x = ball.transform.localPosition.x;
            hamDest.x = Mathf.Clamp(hamDest.x, -380f, 380f);
            _game.HamsterPos = hamDest;
        }
        CountReachedBall(ball);
    }

    void CountReachedBall(UI_Ball ball)
    {
        ball.transform.SetParent(GetObject((int)GameObjects.WaitBallGroup).transform);
        ball.ChangeSkin("A");

        if (_returnBallCount <= Define.MAX_VISIBLE_BALL_COUNT)
            _waitBalls.Enqueue(ball);
        else
            Managers.Resource.Destroy(ball.gameObject);

        int max = Mathf.Min(_game.FullBallCount, Define.MAX_BALL_COUNT + _createBallCount);

        // 모든 볼들이 땅에 돌아오면 후처리
        if (_returnBallCount + _createBallCount == max)
        {
            PostProcess();
        }
    }

    void PostProcess()
    {
        float delay = 0.2f;
        GameObject hamster = GetObject((int)GameObjects.Hamster);

        // 가장 밑줄에 Star가 있는지 체크
        CheckStar();

        // 햄 위치 이동
        Sequence _hamsterMove = DOTween.Sequence()
            .Append(hamster.transform.DOLocalMove(_game.HamsterPos, delay).SetEase(Ease.Linear));
        _hamsterMove.Restart();

        // 한 턴만에 모든 블럭을 부수면 핵 게이지 상승
        delay = Mathf.Max(delay, CheckAchievement());

        // 결과 값 갱신
        UpdateValue();

        // 위 함수들의 실행시간동안 대기 후 게임판 업데이트
        StartCoroutine(RefreshBoard(delay));
    }

    void UpdateValue()
    {
        _game.Score++;
        if (_game.Score >= _game.Highscore)
            _game.Highscore = _game.Score;
        _game.LineCount = 1;
        _game.BallDamage = 1;
        if (_game.GlassesCooltime > 0)
            _game.GlassesCooltime--;
        if (_game.PowerUpCooltime > 0)
            _game.PowerUpCooltime--;
    }

    void CheckStar()
    {
        List<UI_Item_Star> removeList = new List<UI_Item_Star>();

        foreach (var item in _items)
        {
            ItemInfo info = item.GetInfo();
            if (info.y >= 7)
            {
                UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>(GetObject((int)GameObjects.WaitBallGroup).transform);
                ball.SetInfo(item.transform.localPosition, OnBallReachCallBack);
                ball.Create(0.2f, GetObject((int)GameObjects.ControlPad).transform.localPosition.y, OnBallCreateCallBack);
                Managers.Sound.Play(Define.Sound.Effect, "getStar");

                _game.FullBallCount++;
                _returnBallCount = -1;

                removeList.Add(item);
            }
        }

        foreach (var item in removeList)
        {
            _items.Remove(item);
            Managers.Resource.Destroy(item.gameObject);
        }
    }

    float CheckAchievement()
    {
        Sequence text = DOTween.Sequence();
        float interval = 0.0f;

        if (_blocks.Count == 0)
        {
            text.Insert(interval, AchieveBlockClear());
            interval += 0.5f;
        }

        if (_game.Score == _game.Highscore && _game.NewGame == true)
        {
            _game.NewGame = false;
            text.Insert(interval, AchieveNewRecord());
            interval += 0.5f;
        }

        return text.Duration();
    }

    Sequence AchieveBlockClear()
    {
        _game.NuclearDivisionCount++;
        FillNuclearGauge(0.5f);

        GameObject text = GetText((int)Texts.PlusPowerText).gameObject;
        Sequence textSequence = Utils.MakeIncreaseTextSequence(text, 200f);
        textSequence.OnStart(() =>
        {
            text.SetActive(true);
            Managers.Sound.Play(Define.Sound.Effect, "nuclearCharge");
        });
        textSequence.AppendCallback(() => { text.SetActive(false); });
        return textSequence;
    }

    Sequence AchieveNewRecord()
    {
        GameObject text = GetText((int)Texts.NewRecordText).gameObject;
        Sequence textSequence = Utils.MakeIncreaseTextSequence(text, 200f);
        textSequence.OnStart(() =>
        {
            text.SetActive(true);
            Managers.Sound.Play(Define.Sound.Effect, "highscore");
        });
        textSequence.AppendCallback(() => { text.SetActive(false); });
        return textSequence;
    }
    #endregion

    #region 스킬
    void OnNuclearSkill()
    {
        if (_game.State != GameState.idle)
            return;
        _game.State = GameState.skill;

        Managers.Sound.Play(Define.Sound.Effect, "useGlasses");
        GameObject nuclear = GetObject((int)GameObjects.NuclearSkill);
        GameObject board = GetObject((int)GameObjects.GameBoard);
        Vector3 defaultPos = nuclear.transform.localPosition;
        GetText((int)Texts.NuclearStackText).gameObject.SetActive(false);

        Sequence sequence = DOTween.Sequence()
            .Append(nuclear.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.Linear))
            .Join(nuclear.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear))
            .Join(nuclear.transform.DOScale(1.5f, 0.5f).SetEase(Ease.Linear))
            .Append(nuclear.transform.DOScale(2.5f, 1.0f).SetEase(Ease.InElastic))
            .Append(nuclear.transform.DOScale(2.5f, 0).SetEase(Ease.Linear))
            .AppendCallback(() =>
            {
                foreach (var block in _blocks)
                    Managers.Resource.Destroy(block.gameObject);
                _blocks.Clear();

                _game.Score++;
                _game.NuclearStack--;
                Managers.Sound.Play(Define.Sound.Effect, "nuclearAttack");
                Utils.Vibrate();
            })
            .Join(board.transform.DOShakePosition(0.5f, 100))
            .AppendInterval(0.5f)
            .Append(nuclear.transform.DOScale(0f, 0.5f).SetEase(Ease.Linear))
            .OnComplete(() =>
            {
                nuclear.transform.localPosition = defaultPos;
                RefreshUI();
                StartCoroutine(RefreshBoard(0f));
            });

        if (_game.NuclearStack != 1)
        {
            sequence.Append(nuclear.transform.DOLocalMove(defaultPos, 0).SetEase(Ease.Linear))
                .Append(nuclear.transform.DOScale(1.0f, 0.5f).SetEase(Ease.Linear));
        }
    }

    void OnPowerUpSkill()
    {
        if (_game.PowerUpCooltime > 0 || _game.State != GameState.idle)
            return;

        Managers.Sound.Play(Define.Sound.Effect, "usePowerUp");
        _game.PowerUpCooltime = _startData.powerUpFullCooltime;
        _game.BallDamage = _startData.powerUpValue;
        SaveGame();

        StartCoroutine(CoSkillAnimation(Managers.Data.Spine.hamsterSeedEat));

        RefreshUI();
    }

    void OnGlassesSkill()
    {
        if (_game.GlassesCooltime > 0 || _game.State != GameState.idle)
            return;

        Managers.Sound.Play(Define.Sound.Effect, "useGlasses");
        _game.GlassesCooltime = _startData.glassesFullColltime;
        _game.LineCount = _startData.glassesValue;
        SaveGame();

        StartCoroutine(CoSkillAnimation(Managers.Data.Spine.hamsterSeedAfter));

        RefreshUI();
    }

    IEnumerator CoSkillAnimation(string name)
    {
        UI_Spine hamAnim = GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>();
        hamAnim.PlayAnimation(name);
        hamAnim.SetCustomEvent((trackEntry, e) =>
        {
            Managers.Sound.Play(Define.Sound.Effect, "useGlasses");
            foreach (var ball in _waitBalls)
                ball.ChangeSkin("B");
        });

        _game.State = GameState.skill;

        float length = hamAnim.GetAnimationLength(name);
        yield return new WaitForSeconds(length);

        _game.State = GameState.idle;
        hamAnim.PlayAnimation(Managers.Data.Spine.hamsterIdle);
    }

    void FillNuclearGauge(float duration)
    {
        Vector2 nuclearDest = GetObject((int)GameObjects.NuclearEmpty).GetComponent<RectTransform>().sizeDelta;
        float ratio = _startData.nuclearStartRatio + (1 - _startData.nuclearStartRatio) / _startData.nuclearDivisionFullCount * _game.NuclearDivisionCount;
        ratio = Mathf.Clamp(ratio, 0, 1);
        nuclearDest.x *= ratio;

        RectTransform full = GetObject((int)GameObjects.NuclearFull).GetComponent<RectTransform>();
        RectTransform fullStar = GetObject((int)GameObjects.NuclearFullStar).GetComponent<RectTransform>();

        Sequence _nuclearSliderSequence = DOTween.Sequence()
            .Append(full.DOSizeDelta(nuclearDest, duration).SetEase(Ease.Linear))
            .Join(fullStar.DOSizeDelta(nuclearDest, duration).SetEase(Ease.Linear));

        if (ratio == 1)
        {
            _nuclearSliderSequence.OnComplete(() =>
            {
                Managers.Sound.Play(Define.Sound.Effect, "nuclearDone");
                _game.NuclearDivisionCount = 0;
                FillNuclearGauge(1.0f);
                PlayNuclearInstantiateSequence();
                _game.NuclearStack++;
            });
        }
        _nuclearSliderSequence.Restart();
    }

    void PlayNuclearInstantiateSequence()
    {
        GameObject nuclear = GetObject((int)GameObjects.NuclearSkill);
        Vector3 defaultPos = nuclear.transform.localPosition;
        nuclear.SetActive(true);

        Sequence _sequence = DOTween.Sequence();

        if (_game.NuclearStack == 0)
        {
            _sequence.Append(nuclear.transform.DOLocalMove(Vector3.zero, 0))
                .Append(nuclear.transform.DOScale(0, 0))
                .Append(nuclear.transform.DOScale(2.5f, 0.5f).SetEase(Ease.OutBack))
                .Append(nuclear.transform.DOScale(1f, 0.5f).SetEase(Ease.Linear))
                .Join(nuclear.transform.DOLocalMove(defaultPos, 0.5f).SetEase(Ease.Linear))
                .Join(nuclear.transform.DORotate(new Vector3(0, 0, -360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        }
        else
        {
            _sequence.Append(nuclear.transform.DORotate(new Vector3(0, 360, 0), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        }

        _sequence.Restart();
    }
    #endregion

    #region 콜백
    void OnCollisionStar(UI_Item_Star star)
    {
        if (_items.Contains(star) == false)
            return;

        UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>(GetObject((int)GameObjects.ShootBallGroup).transform);
        ball.SetInfo(star.transform.localPosition, OnBallReachCallBack);
        ball.Create((8 - star.GetInfo().y) * 0.2f, GetObject((int)GameObjects.ControlPad).transform.localPosition.y, OnBallCreateCallBack);
        _game.FullBallCount++;

        _items.Remove(star);
        Managers.Resource.Destroy(star.gameObject);
    }

    void OnDestroyBlock(UI_Block block)
    {
        if (_blocks.Contains(block) == false)
            return;

        _blocks.Remove(block);
        foreach (var ball in _shootBalls)
            ball.CalcLine();
    }

    void OnPauseButton()
    {
        if (_game.State == GameState.skill)
            return;

        Managers.Sound.Play(Define.Sound.Effect, "popup");
        Managers.UI.ShowPopupUI<UI_Pause>();
        Time.timeScale = 0;
    }
    #endregion

    private int _currentBall = 0;
    void RefreshUI()
    {
        GetText((int)Texts.ScoreText).text = $"{_game.Score}";
        GetText((int)Texts.HighScoreText).text = $"{_game.Highscore}";
        GetText((int)Texts.PowerUpCooltimeText).text = (_game.PowerUpCooltime == 0) ? "" : $"{_game.PowerUpCooltime}";
        GetText((int)Texts.GlassesCooltimeText).text = (_game.GlassesCooltime == 0) ? "" : $"{_game.GlassesCooltime}";
        GetText((int)Texts.NuclearStackText).text = $"{_game.NuclearStack}";
        GetText((int)Texts.BallCountText).text = (_currentBall <= 0) ? "" : $"x{_currentBall}";

        // 점수별 장식 on/off
        if (_game.Score >= 15)
            GetObject((int)GameObjects.StarBG).SetActive(true);
        if (_game.Score >= 100)
            GetObject((int)GameObjects.Moon).SetActive(true);

        // 점수별 배경
        if (_game.Score < 15)
            GetImage((int)Images.BackGround).sprite = Managers.Resource.Load<Sprite>(_startData.backgroundFirstPath);
        else if (_game.Score < 10000)
            GetImage((int)Images.BackGround).sprite = Managers.Resource.Load<Sprite>(_startData.backgroundsecondPath);
        else
            GetImage((int)Images.BackGround).sprite = Managers.Resource.Load<Sprite>(_startData.backgroundLastPath);

        if (_game.NuclearStack == 0)
        {
            GetObject((int)GameObjects.NuclearSkill).SetActive(false);
            GetText((int)Texts.NuclearStackText).gameObject.SetActive(false);
        }
        else
        {
            GetObject((int)GameObjects.NuclearSkill).SetActive(true);
            GetText((int)Texts.NuclearStackText).gameObject.SetActive(true);
        }

        if (_game.GlassesCooltime == 0)
            GetImage((int)Images.GlassesSkill).sprite = Managers.Resource.Load<Sprite>(_startData.glassesOnSpritePath);
        else
            GetImage((int)Images.GlassesSkill).sprite = Managers.Resource.Load<Sprite>(_startData.glassesOffSpritePath);

        if (_game.PowerUpCooltime == 0)
            GetImage((int)Images.PowerUpSkill).sprite = Managers.Resource.Load<Sprite>(_startData.powerUpOnSpritePath);
        else
            GetImage((int)Images.PowerUpSkill).sprite = Managers.Resource.Load<Sprite>(_startData.powerUpOffSpritePath);
    }

    public void LoadGame()
    {
        Init();

        foreach (var info in _game.BlockList)
        {
            if (info.y == 0)
                continue;

            UI_Block block = Managers.UI.makeSubItem<UI_Block>(GetObject((int)GameObjects.BlockGroup).transform);
            block.SetInfo(info, OnDestroyBlock);
            _blocks.Add(block);
        }

        foreach (var info in _game.ItemList)
        {
            if (info.y == 0)
                continue;

            UI_Item_Star star = Managers.UI.makeSubItem<UI_Item_Star>(GetObject((int)GameObjects.BlockGroup).transform);
            star.SetInfo(info, OnCollisionStar);
            _items.Add(star);
        }

        _currentBall = _game.FullBallCount;
        RefreshUI();
        GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterIdle);

        // 쏘는 도중에 게임을 껐으면 Load시에 지난번 각도로 자동발사됨
        if (_game.State == GameState.shoot)
            StartCoroutine(ShootBalls(true));
    }

    public void NewGame()
    {
        Init();

        StartCoroutine(RefreshBoard(0f));
    }

    public void Clear()
    {
        Managers.Resource.Destroy(_arrowMoon);
        _arrowMoon = null;

        foreach (GameObject star in _arrowStars)
            Managers.Resource.Destroy(star);
        _arrowStars.Clear();

        foreach (var ball in _waitBalls)
            Managers.Resource.Destroy(ball.gameObject);
        _waitBalls.Clear();

        foreach (var ball in _shootBalls)
            Managers.Resource.Destroy(ball.gameObject);
        _shootBalls.Clear();

        foreach (var blcok in _blocks)
            Managers.Resource.Destroy(blcok.gameObject);
        _blocks.Clear();

        foreach (var item in _items)
            Managers.Resource.Destroy(item.gameObject);
        _items.Clear();
    }

    public void SaveGame()
    {
        BlockInfo[] blockInfos = new BlockInfo[Define.MAX_BLOCK_COUNT];
        for (int i = 0; i < _blocks.Count; ++i)
            blockInfos[i] = _blocks[i].GetInfo();
        _game.BlockList = blockInfos;

        ItemInfo[] itemInfos = new ItemInfo[Define.MAX_BLOCK_COUNT];
        for (int i = 0; i < _items.Count; ++i)
            itemInfos[i] = _items[i].GetInfo();
        _game.ItemList = itemInfos;

        Managers.Game.SaveGame();
    }
}

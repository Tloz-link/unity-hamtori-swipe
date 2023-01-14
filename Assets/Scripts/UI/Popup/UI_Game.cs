using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UI_Game : UI_Popup
{
    enum GameObjects
    {
        BackGround,
        StarBG,
        CloudBG,

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
        NuclearFullStar
    }

    enum Texts
    {
        ScoreText,
        PowerUpCooltimeText,
        GlassesCooltimeText,
        NuclearStackText,
        PlusPowerText,
        BallCountText
    }

    enum Images
    {
        PowerUpSkill,
        GlassesSkill
    }

    GameManagerEX _game;
    StartData _startData;

    Queue<UI_Ball> _waitBalls = new Queue<UI_Ball>();
    Queue<UI_Ball> _shootBalls = new Queue<UI_Ball>();
    List<UI_Block> _blocks = new List<UI_Block>();
    List<UI_Item_Star> _items = new List<UI_Item_Star>();
    List<GameObject> _arrowStars = new List<GameObject>();
    GameObject _arrowMoon;

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
        GetText((int)Texts.PlusPowerText).gameObject.SetActive(false);

        Vector3 hamsterStartPos = GetObject((int)GameObjects.Hamster).transform.localPosition;
        hamsterStartPos.x = _game.HamsterPosX;
        GetObject((int)GameObjects.Hamster).transform.localPosition = hamsterStartPos;
        GetObject((int)GameObjects.Hamster).GetOrAddComponent<UI_Spine>();
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPressed, Define.UIEvent.Pressed);
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPointerUp, Define.UIEvent.PointerUp);
        GetImage((int)Images.PowerUpSkill).gameObject.BindEvent(OnPowerUpSkill);
        GetImage((int)Images.GlassesSkill).gameObject.BindEvent(OnGlassesSkill);
        GetObject((int)GameObjects.NuclearSkill).BindEvent(OnNuclearSkill);
        GetObject((int)GameObjects.PauseButton).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_Pause>();
            Time.timeScale = 0;
        });

        int count = Mathf.Min(_game.FullBallCount, Define.MAX_VISIBLE_BALL_COUNT);
        for (int i = 0; i < count; ++i)
        {
            UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>(GetObject((int)GameObjects.WaitBallGroup).transform);
            ball.SetInfo(GetObject((int)GameObjects.ControlPad).transform.localPosition, GetObject((int)GameObjects.ControlPad).transform.localPosition.y, OnBallCallBack);
            _waitBalls.Enqueue(ball);
        }

        CheckBlock();
        StartCoroutine(RefreshBoard(0));
        RefreshUI();
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

            GetObject((int)(GameObjects.Power)).transform.localPosition -= new Vector3(0, deltaY * 0.8f, 0);
            GetObject((int)(GameObjects.GameBoard)).transform.localPosition -= new Vector3(0, deltaY * 0.8f, 0);
        }
        else
        {
            // 가로로 김
            canvasSize = new Vector2(2048, 1920);
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

    private int _currentBall = 0;
    void RefreshUI()
    {
        GetText((int)Texts.ScoreText).text = $"{_game.Score}";
        GetText((int)Texts.PowerUpCooltimeText).text = (_game.PowerUpCooltime == 0) ? "" : $"{_game.PowerUpCooltime}";
        GetText((int)Texts.GlassesCooltimeText).text = (_game.GlassesCooltime == 0) ? "" : $"{_game.GlassesCooltime}";
        GetText((int)Texts.NuclearStackText).text = $"{_game.NuclearStack}";
        GetText((int)Texts.BallCountText).text = (_currentBall <= 0) ? "" : $"x{_currentBall}";

        if (_game.NuclearStack == 0)
            GetObject((int)GameObjects.NuclearSkill).SetActive(false);
        else
            GetObject((int)GameObjects.NuclearSkill).SetActive(true);

        if (_game.GlassesCooltime == 0)
            GetImage((int)Images.GlassesSkill).sprite = Managers.Resource.Load<Sprite>(_startData.glassesOnSpritePath);
        else
            GetImage((int)Images.GlassesSkill).sprite = Managers.Resource.Load<Sprite>(_startData.glassesOffSpritePath);

        if (_game.PowerUpCooltime == 0)
            GetImage((int)Images.PowerUpSkill).sprite = Managers.Resource.Load<Sprite>(_startData.powerUpOnSpritePath);
        else
            GetImage((int)Images.PowerUpSkill).sprite = Managers.Resource.Load<Sprite>(_startData.powerUpOffSpritePath);
    }

    private bool _nuclear = false;
    private Vector2 _nuclearDest;
    private float _nuclearTextTick;
    private const float NUCLEAR_TICK = 0.8f;
    protected override void Update()
    {
        if (_nuclear == true)
        {
            Vector2 size = GetObject((int)GameObjects.NuclearFull).GetComponent<RectTransform>().sizeDelta;
            size = Vector2.MoveTowards(size, _nuclearDest, 500f * Time.deltaTime);
            if (Mathf.Abs(size.x - _nuclearDest.x) < 0.0001f)
            {
                size = _nuclearDest;
                Vector2 fullsize = GetObject((int)GameObjects.NuclearEmpty).GetComponent<RectTransform>().sizeDelta;
                if (size == fullsize)
                {
                    _game.NuclearDivisionCount = 0;
                    float ratio = _startData.nuclearStartRatio + (1 - _startData.nuclearStartRatio) / _startData.nuclearDivisionFullCount * _game.NuclearDivisionCount;
                    fullsize.x *= ratio;
                    _nuclearDest = fullsize;
                    _game.NuclearStack++;
                    RefreshUI();
                    return;
                }

                _nuclear = false;
            }
            GetObject((int)GameObjects.NuclearFull).GetComponent<RectTransform>().sizeDelta = size;
            GetObject((int)GameObjects.NuclearFullStar).GetComponent<RectTransform>().sizeDelta = size;
        }

        _nuclearTextTick += Time.deltaTime;
        if (_nuclearTextTick >= NUCLEAR_TICK)
        {
            GetText((int)Texts.PlusPowerText).gameObject.SetActive(false);
        }
    }

    IEnumerator RefreshBoard(float interval)
    {
        yield return new WaitForSeconds(interval);

        SpawnBlockAndItem();
        foreach (var block in _blocks)
            block.MoveNext();

        foreach (var item in _items)
            item.MoveNext();

        yield return new WaitForSeconds(0.2f);

        if (CheckGameOver() == true)
            yield break;

        _currentBall = _game.FullBallCount;
        RefreshUI();
        GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterIdle);
        _game.State = GameState.idle;
    }

    public void SpawnBlockAndItem()
    {
        int count = UnityEngine.Random.Range(4, 7);
        if (_game.Score < 5)
        {
            count = _game.Score + 1;
        }

        List<int> spawnList = new List<int>();

        for (int i = 0; i < 7; ++i)
        {
            spawnList.Add(i);
        }

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
            block.SetInfo(info, BlockDestroyCallBack);
            _blocks.Add(block);
            spawnList.RemoveAt(rand);
        }

        ItemInfo itemInfo = new ItemInfo
        {
            x = spawnList[0],
            y = 0,
        };
        UI_Item_Star star = Managers.UI.makeSubItem<UI_Item_Star>(GetObject((int)GameObjects.BlockGroup).transform);
        star.SetInfo(itemInfo, AcquireStarCallBack);
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
                    Managers.UI.ClosePopupUI(ui);
                    Managers.UI.ClosePopupUI(this);
                    Managers.Game.Init();
                    Managers.UI.ShowPopupUI<UI_Game>();
                });
                GetObject((int)GameObjects.Hamster).SetActive(false);
                return true;
            }
        }
        return false;
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
                ball.SetInfo(item.transform.localPosition, GetObject((int)GameObjects.ControlPad).transform.localPosition.y, OnBallCallBack);
                ball.Create();
                _waitBalls.Enqueue(ball);
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

    bool CheckBlock()
    {
        if (_blocks.Count != 0)
            return false;

        if (_game.Score != 1)
        {
            _game.NuclearDivisionCount++;
            GetText((int)Texts.PlusPowerText).gameObject.SetActive(true);
        }

        Vector2 nuclearDest = GetObject((int)GameObjects.NuclearEmpty).GetComponent<RectTransform>().sizeDelta;
        float ratio = _startData.nuclearStartRatio + (1 - _startData.nuclearStartRatio) / _startData.nuclearDivisionFullCount * _game.NuclearDivisionCount;
        ratio = Mathf.Clamp(ratio, 0, 1);
        nuclearDest.x *= ratio;
        _nuclearDest = nuclearDest;

        _nuclear = true;
        _nuclearTextTick = 0;

        return _game.Score != 1;
    }

    void OnPadPointerUp()
    {
        if (_game.State != GameState.idle)
            return;

        ClearLine();

        Vector3 dir;
        if (GetShootDir(out dir) == false)
            return;

        _game.State = GameState.shoot;

        StartCoroutine(ShootBalls(dir, 0.08f));
    }

    int _returnBallCount = 0;
    IEnumerator ShootBalls(Vector3 dir, float interval)
    {
        GameObject hamster = GetObject((int)GameObjects.Hamster);
        GameObject board = GetObject((int)GameObjects.GameBoard);
        GameObject shootParent = GetObject((int)GameObjects.ShootBallGroup);
        float startLine = GetObject((int)GameObjects.ControlPad).transform.localPosition.y;

        int count = _game.FullBallCount;
        Vector3 shootPos = hamster.transform.localPosition;
        _returnBallCount = 0;
        yield return null;

        for (int i = count; i > 0; --i)
        {
            hamster.GetComponent<UI_Spine>().PlayAnimationForce(Managers.Data.Spine.hamsterShoot);

            UI_Ball ball;
            if (i > Define.MAX_VISIBLE_BALL_COUNT)
            {
                ball = Managers.UI.makeSubItem<UI_Ball>();
                ball.SetInfo(Vector3.zero, startLine, OnBallCallBack);
            }
            else
            {
                ball = _waitBalls.Dequeue();
            }

            ball.transform.SetParent(shootParent.transform);
            ball.transform.localPosition = shootPos;
            ball.Shoot(board, dir, transform.localScale.x);
            _currentBall--;
            RefreshUI();

            _shootBalls.Enqueue(ball);

            yield return new WaitForSeconds(interval);
        }
        hamster.GetComponent<UI_Spine>().PlayAnimationForce(Managers.Data.Spine.hamsterWait);
    }

    void AcquireStarCallBack(UI_Item_Star star)
    {
        if (_items.Contains(star) == false)
            return;

        UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>(GetObject((int)GameObjects.ShootBallGroup).transform);
        ball.SetInfo(star.transform.localPosition, GetObject((int)GameObjects.ControlPad).transform.localPosition.y, OnBallCallBack);
        ball.Create();
        _game.FullBallCount++;

        _items.Remove(star);
        Managers.Resource.Destroy(star.gameObject);
    }

    void BlockDestroyCallBack(UI_Block block)
    {
        if (_blocks.Contains(block) == false)
            return;

        _blocks.Remove(block);
        Managers.Resource.Destroy(block.gameObject);

        foreach (var ball in _shootBalls)
            ball.CalcLine();
    }

    Vector3 _hamDest;
    void OnBallCallBack(UI_Ball ball)
    {
        GameObject hamster = GetObject((int)GameObjects.Hamster);

        _returnBallCount++;
        if (_returnBallCount == 1)
        {
            _hamDest = new Vector3(ball.transform.localPosition.x, hamster.transform.localPosition.y, 0);
            _hamDest.x = Mathf.Clamp(_hamDest.x, -380f, 380f);
        }

        ball.transform.SetParent(GetObject((int)GameObjects.WaitBallGroup).transform);
        if (_returnBallCount <= Define.MAX_VISIBLE_BALL_COUNT)
            _waitBalls.Enqueue(ball);
        else
            Managers.Resource.Destroy(ball.gameObject);

        if (_returnBallCount == _game.FullBallCount)
        {
            UpdateValue();
            CheckStar();
            hamster.GetComponent<UI_Spine>().Move(_hamDest);
            float interval = 0.4f;
            if (CheckBlock() == true)
                interval = 1.0f;
            StartCoroutine(RefreshBoard(interval));
        }
    }

    void UpdateValue()
    {
        _game.Score++;
        _game.LineCount = 1;
        _game.BallDamage = 1;
        if (_game.GlassesCooltime > 0)
            _game.GlassesCooltime--;
        if (_game.PowerUpCooltime > 0)
            _game.PowerUpCooltime--;
    }

    void OnNuclearSkill()
    {
        if (_game.PowerUpCooltime > 0 || _game.State != GameState.idle)
            return;

        _game.State = GameState.skill;

        foreach (var block in _blocks)
            Managers.Resource.Destroy(block.gameObject);
        _blocks.Clear();

        _game.Score++;
        _game.NuclearStack--;
        RefreshUI();
        StartCoroutine(RefreshBoard(0.1f));
    }

    void OnPowerUpSkill()
    {
        if (_game.PowerUpCooltime > 0 || _game.State != GameState.idle)
            return;

        _game.PowerUpCooltime = _startData.powerUpFullCooltime;
        _game.BallDamage = _startData.powerUpValue;
        StartCoroutine(CoSkillAnimation(Managers.Data.Spine.hamsterSeedEat));

        RefreshUI();
    }

    void OnGlassesSkill()
    {
        if (_game.GlassesCooltime > 0 || _game.State != GameState.idle)
            return;

        _game.GlassesCooltime = _startData.glassesFullColltime;
        _game.LineCount = _startData.glassesValue;
        StartCoroutine(CoSkillAnimation(Managers.Data.Spine.hamsterSeedAfter));

        RefreshUI();
    }

    IEnumerator CoSkillAnimation(string name)
    {
        UI_Spine hamAnim = GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>();
        hamAnim.PlayAnimation(name);
        _game.State = GameState.skill;

        float length = hamAnim.GetAnimationLength(name);
        yield return new WaitForSeconds(length);

        _game.State = GameState.idle;
        hamAnim.PlayAnimation(Managers.Data.Spine.hamsterIdle);
    }

    void OnPadPressed()
    {
        if (_game.State != GameState.idle)
            return;

        ClearLine();

        Vector3 dir;
        if (GetShootDir(out dir) == false)
            return;

        GameObject hamster = GetObject((int)GameObjects.Hamster);
        hamster.GetComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterCharge);
        RaycastHit2D hit = Physics2D.CircleCast(hamster.transform.position, 50 * 0.8f * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
        Vector3 src = hamster.transform.position;
        Vector3 dest = hit.centroid;

        for (int i = 0; i <_game.LineCount; ++i)
        {
            GenerateStar(src, dest, dir, i);
            if (hit.transform.tag == "Floor")
                break;

            src = dest;
            dir = Vector3.Reflect(dir, hit.normal);
            hit = Physics2D.CircleCast(src + dir, 50 * 0.8f * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
            dest = hit.centroid;
        }

        // 마지막 별을 없에고 달로 변경
        GameObject lastStar = _arrowStars[_arrowStars.Count - 1];
        _arrowMoon = Managers.Resource.Instantiate("Contents/ArrowMoon");
        CopyObjectStatus(lastStar, _arrowMoon);
        _arrowStars.RemoveAt(_arrowStars.Count - 1);
        Managers.Resource.Destroy(lastStar);
    }

    public void CopyObjectStatus(GameObject src, GameObject dest)
    {
        dest.transform.SetParent(src.transform.parent);
        dest.transform.position = src.transform.position;
        dest.transform.localRotation = src.transform.localRotation;
        dest.transform.localScale = src.transform.localScale;
    }

    #region Line
    const float DELTA = 10;
    bool GetShootDir(out Vector3 dir)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos -= GetObject((int)GameObjects.GameBoard).transform.position;
        mousePos /= transform.localScale.x;

        Vector2 padSize = GetObject((int)GameObjects.ControlPad).GetComponent<RectTransform>().sizeDelta;
        float upLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.y - DELTA;
        float leftLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.x - (padSize.x / 2) + DELTA;
        float rightLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.x + (padSize.x / 2) - DELTA;

        float ratio = (mousePos.x - leftLimit) / (padSize.x - 2 * DELTA);
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

    void ClearLine()
    {
        Init();

        foreach(GameObject star in _arrowStars)
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

    void GenerateStar(Vector3 src, Vector3 dest, Vector3 dir, int index)
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
}

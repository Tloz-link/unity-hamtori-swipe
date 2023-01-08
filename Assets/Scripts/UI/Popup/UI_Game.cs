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
        ControlPad,
        Hamster,
        ArrowGroup1,
        ArrowGroup2,
        ArrowGroup3,
        ShootBallGroup,
        WaitBallGroup,
        BlockGroup,
        Floor
    }

    GameManagerEX _game;

    Queue<UI_Ball> _waitBalls = new Queue<UI_Ball>();
    Queue<UI_Ball> _shootBalls = new Queue<UI_Ball>();
    List<UI_Block> _blocks = new List<UI_Block>();
    List<GameObject> _arrowStars = new List<GameObject>();
    GameObject _arrowMoon;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _game = Managers.Game;

        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.Hamster).GetOrAddComponent<UI_Spine>();
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPressed, Define.UIEvent.Pressed);
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPointerUp, Define.UIEvent.PointerUp);

        for (int i = 0; i < _game.FullBallCount; ++i)
        {
            UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>(GetObject((int)GameObjects.WaitBallGroup).transform);
            ball.SetInfo(GetObject((int)GameObjects.ControlPad).transform.localPosition.y, ShootBallCallBack);
            _waitBalls.Enqueue(ball);
        }

        StartCoroutine(RefreshUI(0));
        return true;
    }

    IEnumerator RefreshUI(float interval)
    {
        yield return new WaitForSeconds(interval);
        RefreshBall();
        RefreshBlock();

        yield return new WaitForSeconds(0.1f);
        GetObject((int)GameObjects.ControlPad).SetActive(true);
        GetObject((int)GameObjects.Floor).SetActive(true);
        GetObject((int)GameObjects.Hamster).GetComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterIdle);
    }

    public void RefreshBall()
    {
        GameObject hamster = GetObject((int)GameObjects.Hamster);

        int dir = (hamster.transform.localPosition.x >= 0) ? -1 : 1;
        int delta = 50;
        float maxX = hamster.transform.localPosition.x + (50 * dir) + (delta * (_game.FullBallCount - 1) * dir);
        if (maxX < -450f || maxX > 450f)
        {
            float range = (dir == 1) ? 450f - (hamster.transform.localPosition.x + delta) : 450f + (hamster.transform.localPosition.x - delta);
            delta = (int)range / _game.FullBallCount;
        }

        int idx = 0;
        foreach (var ball in _waitBalls)
        {
            float posX = hamster.transform.localPosition.x + (50 * dir) + (delta * idx * dir);
            float posY = GetObject((int)GameObjects.ControlPad).transform.localPosition.y;
            ball.Move(new Vector3(posX, posY, 0));
            idx++;
        }
    }

    public void RefreshBlock()
    {
        int count = UnityEngine.Random.Range(1, 5);
        List<int> spawnList = new List<int>();

        for (int i = 0; i < 7; ++i)
        {
            spawnList.Add(i);
        }

        for (int i = 0; i < count; ++i)
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

        foreach (var block in _blocks)
        {
            BlockInfo info = block.GetInfo();
            info.y += 1;
            Vector3 dest = new Vector3(_game.BlockStartX + (info.x * _game.BlockGapX), _game.BlockStartY - (info.y * _game.BlockGapY), 0);
            block.Move(dest);
        }
    }

    void OnPadPointerUp()
    {
        ClearLine();

        Vector3 dir;
        if (GetShootDir(out dir) == false)
            return;

        GetObject((int)GameObjects.ControlPad).SetActive(false);
        GetObject((int)GameObjects.Floor).SetActive(false);

        StartCoroutine(ShootBalls(dir, 0.1f));
    }

    int _returnBallCount;
    IEnumerator ShootBalls(Vector3 dir, float interval)
    {
        int count = _waitBalls.Count;
        GameObject hamster = GetObject((int)GameObjects.Hamster);
        Vector3 shootPos = hamster.transform.localPosition;
        _returnBallCount = 0;

        for (int i = 0; i < count; ++i)
        {
            hamster.GetComponent<UI_Spine>().PlayAnimationForce(Managers.Data.Spine.hamsterShoot);

            var ball = _waitBalls.Dequeue();
            ball.transform.SetParent(GetObject((int)GameObjects.ShootBallGroup).transform);
            ball.transform.localPosition = shootPos;
            ball.Shoot(dir, transform.localScale.x);
            RefreshBall();
            _shootBalls.Enqueue(ball);

            yield return new WaitForSeconds(interval);
        }
        hamster.GetComponent<UI_Spine>().PlayAnimationForce(Managers.Data.Spine.hamsterWait);
    }

    void BlockDestroyCallBack(UI_Block block)
    {
        if (_blocks.Contains(block) == false)
            return;

        _blocks.Remove(block);
        Managers.Resource.Destroy(block.gameObject);
    }

    void ShootBallCallBack(UI_Ball ball)
    {
        ball.transform.SetParent(GetObject((int)GameObjects.WaitBallGroup).transform);
        _waitBalls.Enqueue(ball);
        _returnBallCount++;

        if (_returnBallCount == 1)
        {
            GameObject hamster = GetObject((int)GameObjects.Hamster);
            Vector3 dest = new Vector3(ball.transform.localPosition.x, hamster.transform.localPosition.y, 0);
            dest.x = Mathf.Clamp(dest.x, -380f, 380f);
            hamster.GetComponent<UI_Spine>().Move(dest);
        }

        if (_returnBallCount == _game.FullBallCount)
        {
            _game.Score++;
            StartCoroutine(RefreshUI(0.4f));
        }
    }

    //TODO 나중에 멀티 터치 테스트 해봐야함
    void OnPadPressed()
    {
        ClearLine();

        Vector3 dir;
        if (GetShootDir(out dir) == false)
            return;

        GameObject hamster = GetObject((int)GameObjects.Hamster);
        hamster.GetComponent<UI_Spine>().PlayAnimation(Managers.Data.Spine.hamsterCharge);
        RaycastHit2D hit = Physics2D.CircleCast(hamster.transform.position, 40 * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
        Vector3 src = hamster.transform.position;
        Vector3 dest = hit.centroid;

        for (int i = 0; i < 1; ++i)
        {
            GenerateStar(src, dest, dir, i);
            if (hit.transform.tag == "Floor")
                break;

            src = dest;
            dir = Vector3.Reflect(dir, hit.normal);
            hit = Physics2D.CircleCast(src + dir, 40 * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
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
        mousePos -= transform.position;
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
}

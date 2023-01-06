using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using static Define;
using System.Reflection;

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
        BlockGroup
    }

    GameManagerEX _game;

    Queue<UI_Ball> _waitBalls = new Queue<UI_Ball>();
    Queue<UI_Ball> _shootBalls = new Queue<UI_Ball>();
    List<GameObject> _arrowStars = new List<GameObject>();
    List<GameObject> _arrowMoons = new List<GameObject>();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _game = Managers.Game;

        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.Hamster).GetOrAddComponent<BaseController>();
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPressed, Define.UIEvent.Pressed);
        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPointerUp, Define.UIEvent.PointerUp);

        for (int i = 0; i < _game.FullBallCount; ++i)
        {
            UI_Ball ball = Managers.UI.makeSubItem<UI_Ball>(GetObject((int)GameObjects.WaitBallGroup).transform);
            ball.transform.localScale *= 0.8f;
            _waitBalls.Enqueue(ball);
        }

        RefreshUI();
        return true;
    }

    public void RefreshUI()
    {
        if (_init == false)
            return;

        GetObject((int)GameObjects.ControlPad).SetActive(true);
        RefreshBall();
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

    void OnPadPointerUp()
    {
        ClearLine();

        Vector3 dir;
        if (GetShootDir(out dir) == false)
            return;

        GetObject((int)GameObjects.ControlPad).SetActive(false);
        StartCoroutine(ShootBalls(dir, 0.1f));
    }

    IEnumerator ShootBalls(Vector3 dir, float interval)
    {
        int count = _waitBalls.Count;
        GameObject hamster = GetObject((int)GameObjects.Hamster);

        for (int i = 0; i < count; ++i)
        {
            var ball = _waitBalls.Dequeue();
            ball.transform.SetParent(GetObject((int)GameObjects.ShootBallGroup).transform);
            ball.transform.localPosition = hamster.transform.localPosition;
            ball.Shoot(dir, transform.localScale.x, GetObject((int)GameObjects.ControlPad).transform.localPosition.y, ShootBallCallBack);
            RefreshBall();
            _shootBalls.Enqueue(ball);

            yield return new WaitForSeconds(interval);
        }
    }

    void ShootBallCallBack(UI_Ball ball)
    {
        ball.transform.SetParent(GetObject((int)GameObjects.WaitBallGroup).transform);
        _waitBalls.Enqueue(ball);

        if (_waitBalls.Count == 1)
        {
            GameObject hamster = GetObject((int)GameObjects.Hamster);
            Vector3 dest = new Vector3(ball.transform.localPosition.x, hamster.transform.localPosition.y, 0);
            dest.x = Mathf.Clamp(dest.x, -380f, 380f);
            hamster.GetComponent<BaseController>().Move(dest);
        }
        else if (_waitBalls.Count == _game.FullBallCount)
        {
            RefreshUI();
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
        RaycastHit2D hit = Physics2D.CircleCast(hamster.transform.position, 40 * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
        Vector3 src = hamster.transform.position;
        Vector3 dest = hit.centroid;
        
        for (int i = 0; i < 3; ++i)
        {
            GenerateLine(src, dest, dir, i);
            src = dest;
            dir = Vector3.Reflect(dir, hit.normal);
            hit = Physics2D.CircleCast(src, 40 * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall"));
            dest = hit.centroid;
        }
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

        dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        if (mousePos.x < leftLimit || mousePos.x > rightLimit || mousePos.y > upLimit)
        {
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

        foreach (GameObject moon in _arrowMoons)
        {
            Managers.Resource.Destroy(moon);
        }
        _arrowMoons.Clear();

        for (int i = 0; i < 3; ++i)
        {
            GameObject arrowGroup = GetObject((int)GameObjects.ArrowGroup1 + i);
            arrowGroup.transform.rotation = Quaternion.identity;
        }    
    }

    void GenerateLine(Vector3 src, Vector3 dest, Vector3 dir, int index)
    {
        GameObject arrowGroup = GetObject((int)GameObjects.ArrowGroup1 + index);
        arrowGroup.transform.position = src;

        float dist = (dest - src).magnitude / transform.localScale.x;
        int need = (int)dist / 40;

        for (int i = 0; i < need; ++i)
        {
            GameObject star = Managers.Resource.Instantiate("Contents/ArrowStar", arrowGroup.transform);
            star.transform.localPosition = new Vector3(0, dist - (need - i) * 40, 0);
            star.transform.localRotation = Quaternion.identity;

            if (i <= 2 && index == 0)
            {
                Color color = star.GetComponent<Image>().color;
                color.a = 0.4f;
                star.GetComponent<Image>().color = color;
            }
            _arrowStars.Add(star);
        }

        GameObject moon = Managers.Resource.Instantiate("Contents/ArrowMoon", arrowGroup.transform);
        moon.transform.localPosition = new Vector3(0, dist, 0);
        _arrowMoons.Add(moon);

        float angle = Vector3.Angle(dir, Vector3.left);
        arrowGroup.transform.rotation = Quaternion.AngleAxis(90 - angle, Vector3.forward);
    }
    #endregion
}

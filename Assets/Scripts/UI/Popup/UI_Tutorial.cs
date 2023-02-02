using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tutorial : UI_Popup
{
    enum GameObjects
    {
        BackGround,
        Pad,
        Cursor
    }

    enum Images
    {
        Cursor
    }

    Vector3 _dir;
    GameObject _pad;
    UI_Game _board;
    bool _target = false;

    Action _endCallback;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));

        GetObject((int)GameObjects.BackGround).gameObject.BindEvent(OnClickTutorial);

        _board = GameObject.Find("UI_Game").GetComponent<UI_Game>();

        _pad = Instantiate(_board.GetObject((int)UI_Game.GameObjects.ControlPad), GetObject((int)GameObjects.Pad).transform);
        _pad.transform.position = _board.GetObject((int)UI_Game.GameObjects.ControlPad).transform.position;
        _pad.GetComponent<Image>().color = new Color(162.0f / 255.0f, 255.0f / 255.0f, 250.0f / 255.0f, 0.0f / 255.0f);

        GameObject cursor = GetObject((int)GameObjects.Cursor);
        GetImage((int)Images.Cursor).sprite = Managers.Resource.Load<Sprite>("Sprites/UI/Tutorial/tutorial_ham_up");

        cursor.transform.position = _board.GetObject((int)UI_Game.GameObjects.ControlPad).transform.position;
        cursor.transform.localPosition += new Vector3(-290f, -200f, 0f);

        ViewPad();
        Target();

        return true;
    }

    public void SetInfo(Action callback)
    {
        _endCallback = callback;
    }

    void Update()
    {
        _board.ClearArrow();

        UpdateTarget();
    }

    void UpdateTarget()
    {
        if (_target == false)
            return;

        Vector3 cursorPos = GetObject((int)GameObjects.Cursor).transform.position;
        cursorPos -= _board.GetObject((int)UI_Game.GameObjects.GameBoard).transform.position;
        cursorPos /= _board.gameObject.transform.localScale.x;

        if (_board.CalcShootDir(out _dir, cursorPos) == false)
            return;

        _board.GenerateArrow(_dir);
    }

    void Target()
    {
        GameObject cursor = GetObject((int)GameObjects.Cursor);

        Sequence target = DOTween.Sequence()
            .AppendInterval(2.5f)
            .AppendCallback(() =>
            {
                GetImage((int)Images.Cursor).sprite = Managers.Resource.Load<Sprite>("Sprites/UI/Tutorial/tutorial_ham_down");
                _target = true;
            })
            .Append(cursor.transform.DOLocalMoveX(300f, 1.0f).SetRelative().SetEase(Ease.Linear))
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                GetImage((int)Images.Cursor).sprite = Managers.Resource.Load<Sprite>("Sprites/UI/Tutorial/tutorial_ham_up");
                _target = false;

                Managers.Game.ShootDir = _dir;
                _board.LoadBalls();
                _board._shoot = true;
                Managers.Game.FullBallCount = 0;
            })
            .AppendInterval(2.5f)
            .AppendCallback(() =>
            {
                Managers.UI.ClosePopupUI(this);
                _endCallback.Invoke();
            });
        target.Restart();
    }

    void ViewPad()
    {
        Sequence pad = DOTween.Sequence()
            .Append(_pad.GetComponent<Image>().DOFade(0.9f, 0.5f))
            .Append(_pad.GetComponent<Image>().DOFade(0f, 0.5f))
            .Append(_pad.GetComponent<Image>().DOFade(0.9f, 0.5f))
            .Append(_pad.GetComponent<Image>().DOFade(0f, 0.5f))
            .Append(_pad.GetComponent<Image>().DOFade(0.6f, 0.5f));

        pad.Restart();
    }

    void OnClickTutorial()
    {

    }
}

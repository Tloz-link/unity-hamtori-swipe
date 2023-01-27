using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tutorial : UI_Popup
{
    enum GameObjects
    {
        Panel,
        Pad,
        Cursor
    }

    enum Images
    {
        Cursor
    }

    GameObject _pad;
    UI_Game _board;
    bool _target = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        _board = GameObject.Find("UI_Game").GetComponent<UI_Game>();

        _pad = Instantiate(_board.GetObject((int)UI_Game.GameObjects.ControlPad), GetObject((int)GameObjects.Pad).transform);
        _pad.transform.position = _board.GetObject((int)UI_Game.GameObjects.ControlPad).transform.position;
        _pad.GetComponent<Image>().color = new Color(162.0f / 255.0f, 255.0f / 255.0f, 250.0f / 255.0f, 150.0f / 255.0f);

        GameObject cursor = GetObject((int)GameObjects.Cursor);
        GetImage((int)Images.Cursor).sprite = Managers.Resource.Load<Sprite>("Sprites/UI/Tutorial/tutorial_ham_up");

        cursor.transform.position = _board.GetObject((int)UI_Game.GameObjects.ControlPad).transform.position;
        cursor.transform.localPosition += new Vector3(-290f, -200f, 0f);

        Sequence target = DOTween.Sequence()
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                GetImage((int)Images.Cursor).sprite = Managers.Resource.Load<Sprite>("Sprites/UI/Tutorial/tutorial_ham_down");
                _target = true;
            })
            .Append(cursor.transform.DOLocalMoveX(580f, 5.0f).SetRelative().SetEase(Ease.Linear))
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                GetImage((int)Images.Cursor).sprite = Managers.Resource.Load<Sprite>("Sprites/UI/Tutorial/tutorial_ham_up");
                _target = false;
            });
        target.Restart();

        GetObject((int)GameObjects.Panel).gameObject.BindEvent(OnClickTutorial);
        return true;
    }

    void Update()
    {
        _board.ClearArrow();

        Target();
    }

    void Target()
    {
        if (_target == false)
            return;

        Vector3 cursorPos = GetObject((int)GameObjects.Cursor).transform.position;
        cursorPos -= _board.GetObject((int)UI_Game.GameObjects.GameBoard).transform.position;
        cursorPos /= _board.gameObject.transform.localScale.x;

        Vector3 dir;
        if (_board.CalcShootDir(out dir, cursorPos) == false)
            return;

        _board.GenerateArrow(dir);
    }

    void OnClickTutorial()
    {
        Managers.UI.ClosePopupUI(this);
    }
}

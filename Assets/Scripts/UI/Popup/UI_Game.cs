using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using static Define;

public class UI_Game : UI_Popup
{
    enum GameObjects
    {
        ControlPad,
        Hamster,
        ArrowGroup,
        ShootBallGroup,
        WaitBallGroup,
        BlockGroup
    }

    List<UI_ArrowStar> _arrowStars = new List<UI_ArrowStar>();
    UI_ArrowMoon _arrowMoon;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));

        GetObject((int)GameObjects.ControlPad).BindEvent(OnPadPressed, Define.UIEvent.Pressed);

        _arrowMoon = Managers.UI.makeSubItem<UI_ArrowMoon>(GetObject((int)GameObjects.ArrowGroup).transform);
        return true;
    }

    void OnPadPressed()
    {
        ClearLine();

        //TODO 나중에 멀티 터치 테스트 해봐야함
        Vector3 mousePos = Input.mousePosition;
        mousePos -= transform.position;
        mousePos /= transform.localScale.x;

        const float DELTA = 10;
        Vector2 padSize = GetObject((int)GameObjects.ControlPad).GetComponent<RectTransform>().sizeDelta;
        float upLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.y - DELTA;
        float leftLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.x - (padSize.x / 2) + DELTA;
        float rightLimit = GetObject((int)GameObjects.ControlPad).transform.localPosition.x + (padSize.x / 2) - DELTA;
        if (mousePos.x < leftLimit || mousePos.x > rightLimit || mousePos.y > upLimit)
        {
            Debug.Log("Arrow Cancel~");
            return;
        }

        GameObject hamster = GetObject((int)GameObjects.Hamster);
        float ratio = (mousePos.x - leftLimit) / (padSize.x - 2 * DELTA);
        float angle = 10f + (160f * ratio);

        Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
        Vector3 dest = Physics2D.CircleCast(hamster.transform.position, 40 * transform.localScale.x, dir, 10000, 1 << LayerMask.NameToLayer("Wall")).centroid;
        GenerateLine(dest - hamster.transform.position, angle);
    }

    void ClearLine()
    {
        Init();

        for (int i = 0; i < _arrowStars.Count; ++i)
        {
            _arrowStars[i].gameObject.SetActive(false);
        }
        _arrowMoon.gameObject.SetActive(false);
    }

    void GenerateLine(Vector3 line, float angle)
    {
        GameObject arrowGroup = GetObject((int)GameObjects.ArrowGroup);
        float dist = line.magnitude / transform.localScale.x;
        int need = (int)dist / 40;
        int lack = need - _arrowStars.Count;
        for (int i = 0; i < lack; ++i)
        {
            UI_ArrowStar star = Managers.UI.makeSubItem<UI_ArrowStar>(arrowGroup.transform);
            _arrowStars.Add(star);
            star.gameObject.SetActive(false);
        }

        Debug.Assert(need <= _arrowStars.Count, $"need : {need}   _starts.Count : {_arrowStars.Count}");

        for (int i = 0; i < need; ++i)
        {
            _arrowStars[i].transform.localPosition = new Vector3(0, dist - (need - i) * 40, 0);
            _arrowStars[i].transform.localRotation = Quaternion.identity;
            _arrowStars[i].gameObject.SetActive(true);

            if (i <= 2)
            {
                Color color = _arrowStars[i].GetComponent<Image>().color;
                color.a = 0.4f;
                _arrowStars[i].GetComponent<Image>().color = color;
            }
        }
        _arrowMoon.transform.localPosition = new Vector3(0, dist, 0);
        _arrowMoon.gameObject.SetActive(true);

        arrowGroup.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }
}

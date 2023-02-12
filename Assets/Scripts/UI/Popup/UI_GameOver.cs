using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameOver : UI_Popup
{
    enum GameObjects
    {
        Background,
        Bubble
    }

    enum Texts
    {
        ScoreText,
        HighScoreText
    }

    UI_Game _game;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _game = GameObject.Find("UI_Game").GetComponent<UI_Game>();

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));

        Vector2 size = new Vector2(Screen.width, 295);
        GetObject((int)GameObjects.Background).GetComponent<RectTransform>().sizeDelta = size;

        float bubbleLocalX = Math.Clamp(Managers.Game.HamsterPos.x, -330f, 330f);
        GetObject((int)GameObjects.Bubble).transform.DOLocalMoveX(bubbleLocalX, 0f);

        float scaleHeight = ((float)Screen.width / Screen.height) / ((float)9 / 16);
        if (scaleHeight <= 1)
        {
            float deltaY = Mathf.Abs((1080 * (Screen.height * 1080 / Screen.width)) - (1080 * 1920)) / 2 / 1080;
            GetObject((int)(GameObjects.Bubble)).transform.localPosition -= new Vector3(0, deltaY * 0.4f, 0);
        }

        GetText((int)Texts.ScoreText).text = $"{Managers.Game.Score}";
        GetText((int)Texts.HighScoreText).text = $"Best {Managers.Game.Highscore}";

        return true;
    }

    public void SetInfo(Action onRestartCallBack)
    {
        Init();

        _game.GetObject((int)UI_Game.GameObjects.Hamster).BindEvent(onRestartCallBack);
    }
}

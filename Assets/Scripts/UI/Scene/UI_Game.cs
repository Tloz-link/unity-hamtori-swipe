using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Game : UI_Scene
{
    enum Texts
    {
        BallCount
    }

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        Bind<Text>(typeof(Texts));
    }

    void Update()
    {
        GetText((int)Texts.BallCount).text = $"{Managers.Game.CurrentBallCount}";
    }
}

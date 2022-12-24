using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GameOver : UI_Popup
{
    enum GameObjects
    {
        BlurPanel,
        Panel
    }

    enum Buttons
    {
        RestartButton
    }

    public override void Init()
    {
        base.Init();

        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));
        Managers.Game.Hamster.transform.SetParent(Get<GameObject>((int)GameObjects.Panel).transform);

        Get<Button>((int)Buttons.RestartButton).gameObject.BindEvent(OnButtonClicked);
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        
    }

    public void OnButtonClicked(PointerEventData data)
    {
        Managers.Game.Restart();
        Managers.UI.ClosePopupUI();
    }
}

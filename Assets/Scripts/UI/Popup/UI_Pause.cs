using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Pause : UI_Popup
{
    enum GameObjects
    {
        bg,
        CanselButton,
        SoundButton,
        SoundLeftButton,
        SoundRightButton,
        MainScreenButton
    }

    enum Images
    {
        SoundImage,
        SoundStack1,
        SoundStack2,
        SoundStack3,
        SoundStack4,
        SoundStack5,
        SoundStack6,
        VibrationImage,
        VibrationStack
    }

    Sprite _stackOn;
    Sprite _stackOff;

    Sprite _soundOn;
    Sprite _soundOff;

    Sprite _vibrateOn;
    Sprite _vibrateOff;


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));

        _stackOn = Managers.Resource.Load<Sprite>("Sprites/UI/Pause/stack_on");
        _stackOff = Managers.Resource.Load<Sprite>("Sprites/UI/Pause/stack_off");

        _soundOn = Managers.Resource.Load<Sprite>("Sprites/UI/Pause/sound_on");
        _soundOff = Managers.Resource.Load<Sprite>("Sprites/UI/Pause/sound_off");

        _vibrateOn = Managers.Resource.Load<Sprite>("Sprites/UI/Pause/vibrate_on");
        _vibrateOff = Managers.Resource.Load<Sprite>("Sprites/UI/Pause/vibrate_off");

        Sequence open = Utils.MakePopupOpenSequence(GetObject((int)GameObjects.bg));
        open.SetUpdate(true);
        open.OnComplete(() =>
        {
            GetObject((int)GameObjects.CanselButton).BindEvent(OnCanselButton);
            GetObject((int)GameObjects.MainScreenButton).BindEvent(OnMainScreenButton);
            GetObject((int)GameObjects.SoundButton).BindEvent(OnSoundButtonPressed, Define.UIEvent.Pressed);
            GetObject((int)GameObjects.SoundLeftButton).BindEvent(OnSoundLeftButton);
            GetObject((int)GameObjects.SoundRightButton).BindEvent(OnSoundRightButton);
            GetImage((int)Images.VibrationStack).gameObject.BindEvent(OnVibrationButton);
        });
        open.Restart();


        RefreshUI();
        return true;
    }

    void RefreshUI()
    {
        int currentVolume = (int)(Managers.Game.Volume / Define.MAX_VOLUME * Define.MAX_VOLUME_COUNT);

        GetImage((int)Images.SoundImage).sprite = (currentVolume > 0) ? _soundOn : _soundOff;
        GetImage((int)Images.VibrationImage).sprite = (Managers.Game.Vibrate) ? _vibrateOn : _vibrateOff;
        for (int i = 0; i < Define.MAX_VOLUME_COUNT; ++i)
            GetImage((int)Images.SoundStack1 + i).sprite = (i < currentVolume) ? _stackOn : _stackOff;
        GetImage((int)Images.VibrationStack).sprite = (Managers.Game.Vibrate) ? _stackOn : _stackOff;
    }

    void OnCanselButton()
    {
        Destroy(GetObject((int)GameObjects.CanselButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.MainScreenButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.SoundButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.SoundLeftButton).GetComponent<UI_EventHandler>());
        Destroy(GetObject((int)GameObjects.SoundRightButton).GetComponent<UI_EventHandler>());
        Destroy(GetImage((int)Images.VibrationStack).gameObject.GetComponent<UI_EventHandler>());

        Managers.Sound.Play(Define.Sound.Effect, "popup");

        Sequence close = Utils.MakePopupCloseSequence(GetObject((int)GameObjects.bg));
        close.SetUpdate(true);
        close.OnComplete(() =>
        {
            Managers.UI.ClosePopupUI(this);
            Time.timeScale = 1;
        });
    }

    void OnMainScreenButton()
    {
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");

        UI_Confirm confirm = Managers.UI.ShowPopupUI<UI_Confirm>();
        confirm.SetInfo("메인 화면으로 갈까요?", () =>
        {
            Managers.Sound.Play(Define.Sound.Effect, "uiTouch");
            Managers.UI.CloseAllPopupUI();

            Time.timeScale = 1;
            DOTween.KillAll();
            Managers.UI.ShowPopupUI<UI_Title>();
        });
    }

    void OnVibrationButton()
    {
        if (Managers.Game.Vibrate)
            Managers.Game.Vibrate = false;
        else
            Managers.Game.Vibrate = true;

        RefreshUI();
        Utils.Vibrate();
        Managers.Game.SaveGame();
    }

    void OnSoundButtonPressed()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos -= GetObject((int)GameObjects.bg).transform.position;
        mousePos /= transform.localScale.x;

        float width = GetObject((int)GameObjects.SoundButton).GetComponent<RectTransform>().sizeDelta.x;
        float leftLimit = GetObject((int)GameObjects.SoundButton).transform.localPosition.x - (width / 2);

        float ratio = (mousePos.x - leftLimit) / width * Define.MAX_VOLUME;

        int nextVolume = (int)(ratio / Define.MAX_VOLUME * Define.MAX_VOLUME_COUNT) + 1;
        if (ratio < 0)
            nextVolume = 0;

        int currentVolume = (int)(Managers.Game.Volume / Define.MAX_VOLUME * Define.MAX_VOLUME_COUNT);
        if (nextVolume == currentVolume || nextVolume > Define.MAX_VOLUME_COUNT)
            return;

        ChangeVolume(nextVolume);
    }

    void OnSoundLeftButton()
    {
        int currentVolume = (int)(Managers.Game.Volume / Define.MAX_VOLUME * Define.MAX_VOLUME_COUNT);
        if (currentVolume <= 0)
            return;

        ChangeVolume(currentVolume - 1);
    }

    void OnSoundRightButton()
    {
        int currentVolume = (int)(Managers.Game.Volume / Define.MAX_VOLUME * Define.MAX_VOLUME_COUNT);
        if (currentVolume >= Define.MAX_VOLUME_COUNT)
            return;

        ChangeVolume(currentVolume + 1);
    }

    void ChangeVolume(int volume)
    {
        Managers.Game.Volume = volume;
        Managers.Sound.SetVolume(Managers.Game.Volume);
        RefreshUI();
        Managers.Sound.Play(Define.Sound.Effect, "uiTouch");
        Managers.Game.SaveGame();
    }
}

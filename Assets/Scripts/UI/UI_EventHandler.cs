using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Action OnClickHandler = null;
    public Action OnPressedHandler = null;
    public Action OnPointerDownHandler = null;
    public Action OnPointerUpHandler = null;
    public Action OnPressedLongHandler = null;
    public Action OnPointerExitHandler = null;

    bool _pressed = false;
    bool _usePressedLong = true;

    float _tick = 0.0f;
    private void Update()
    {
        if (_pressed)
        {
            if (CheckMousePos() == true)
                OnPressedHandler?.Invoke();
            else
            {
                if (OnPointerExitHandler != null)
                {
                    OnPointerExitHandler.Invoke();
                    _pressed = false;
                    return;
                }

                OnPointerUpHandler?.Invoke();
            }
        }

        if (OnPressedLongHandler == null || _usePressedLong)
            return;

        _tick += Time.deltaTime;
        if (_tick > 0.5f)
        {
            OnPressedLongHandler.Invoke();
            _usePressedLong = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickHandler?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        _usePressedLong = false;
        _tick = 0.0f;
        OnPointerDownHandler?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        if (CheckMousePos() == false)
            return;

        if (_usePressedLong == true && OnPointerExitHandler != null)
        {
            OnPointerExitHandler.Invoke();
            return;
        }

        _usePressedLong = true;
        OnPointerUpHandler?.Invoke();
    }

    bool CheckMousePos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos -= transform.position;
        mousePos /= Managers.Game.CanvasScale;

        Vector2 padSize = GetComponent<RectTransform>().sizeDelta;
        float upLimit = padSize.y / 2;
        float downLimit = upLimit * -1;
        float rightLimit = padSize.x / 2;
        float leftLimit = rightLimit * -1;

        if (mousePos.x < leftLimit || mousePos.x > rightLimit || mousePos.y > upLimit || mousePos.y < downLimit)
        {
            _usePressedLong = true;
            return false;
        }
        return true;
    }
}

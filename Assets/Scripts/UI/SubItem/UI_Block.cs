using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Block : UI_Spine
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        return true;
    }
}

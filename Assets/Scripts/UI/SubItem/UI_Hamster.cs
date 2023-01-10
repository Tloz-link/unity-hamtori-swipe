using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Hamster : UI_Spine
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class SpineData
{
    //block
    [XmlAttribute]
    public string blockIdle;
    [XmlAttribute]
    public string blockTarget;
    [XmlAttribute]
    public string blockDamaged;
    [XmlAttribute]
    public string blockDestory;
    [XmlAttribute]
    public string blockPrologue;

    //ball
    [XmlAttribute]
    public string ballIdle;
    [XmlAttribute]
    public string ballJump;
    [XmlAttribute]
    public string ballGameover;

    //hamster
    [XmlAttribute]
    public string hamsterIdle;
    [XmlAttribute]
    public string hamsterCharge;
    [XmlAttribute]
    public string hamsterShoot;
    [XmlAttribute]
    public string hamsterWait;
    [XmlAttribute]
    public string hamsterGameover;
    [XmlAttribute]
    public string hamsterSeedAfter;
    [XmlAttribute]
    public string hamsterSeedEat;
    [XmlAttribute]
    public string hamsterPrologue;

    //purple
    [XmlAttribute]
    public string purpleIdle;
    [XmlAttribute]
    public string purpleCreate;

    //starlight
    [XmlAttribute]
    public string starlightIdle;
    [XmlAttribute]
    public string starlightPlus;

    //tutoHamster
    [XmlAttribute]
    public string tutoHamsterIdle;
    [XmlAttribute]
    public string tutoHamsterTouch;
}

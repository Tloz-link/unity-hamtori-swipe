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

    //ball
    [XmlAttribute]
    public string ballIdle;
    [XmlAttribute]
    public string ballRIghtRoll;
    [XmlAttribute]
    public string ballLeftRoll;
    [XmlAttribute]
    public string ballJump;

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

}

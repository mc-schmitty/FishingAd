using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishShotShield : MonoBehaviour
{
    [SerializeField]
    private Image fishTankShield;
    [SerializeField]
    private float ftShieldMaxHP;
    private float ftShieldHP;

    [SerializeField]
    private Image goldMenuShield;
    [SerializeField]
    private float gmShieldMaxHP;
    private float gmShieldHP;

    [SerializeField]
    private Image personalShield;
    [SerializeField]
    private float psRadius = 1f;
    [SerializeField]
    private float personalShieldMaxHP;
    private float psHP;

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishShotShield : MonoBehaviour
{
    public static FishShotShield Manager;

    [SerializeField]
    private Image fishTankShield;
    [SerializeField]
    private float ftShieldMaxHP;
    private float ftShieldHP;
    [SerializeField]
    private float ftShieldRechargeTime = 10f;
    [SerializeField]
    private Image fishTankIconRecharge;

    [SerializeField]
    private Image shopMenuShield;
    [SerializeField]
    private float smShieldMaxHP;
    private float smShieldHP;
    [SerializeField]
    private float smShieldRechargeTime = 10f;
    [SerializeField]
    private Image shopMenuIconRecharge;

    [SerializeField]
    private Image personalShield;               // Central shield object, also displays shield hp radially
    [SerializeField]
    private Image personalShieldRadiusImage;    // currently used for detecting shots fired in radius
    [SerializeField]
    private Gradient shieldDamageColor;         // color shield will change to as health lowers
    [SerializeField]
    private Image personalShieldDamageColor;    // image who's color will be manipulated by above
    [SerializeField]
    private AnimationCurve shieldDamageFlashRate;       // how much shield will flicker as health lowers
    [SerializeField]
    private UIRadarPing shieldFlashObject;              // image which will flicker according to curve
    [SerializeField]
    private float personalShieldMaxHP;
    private float psHP;
    [SerializeField]
    private float personalShieldRechargeTime = 5f;
    [SerializeField]
    [Range(0, 1)]
    private float psChargeTime = 0.25f;
    private float psRampUp;
    private bool psActivated;

    [SerializeField]
    [Tooltip("If true, menus close when they are broken.")]
    private bool weakMenus = true;

    [SerializeField]
    private Sprite[] damageSprites;
    [SerializeField]
    private ParticleSystem pShieldDamagePE;
    [SerializeField]
    private ParticleSystem ftShieldDamagePE;
    [SerializeField]
    private ParticleSystem smShieldDamagePE;

    private void Awake()
    {
        if (Manager == null)
            Manager = this;
        else
            this.enabled = false;       // disable shield checking and response
    }

    private void OnEnable()
    {
        FishBounty.FishShotBlock += BlockDamage;
    }

    private void OnDisable()
    {
        FishBounty.FishShotBlock -= BlockDamage;
    }

    private void Start()
    {
        // refresh shield health
        ftShieldHP = ftShieldMaxHP;
        smShieldHP = smShieldMaxHP;
        psHP = personalShieldMaxHP;     // reset shield health

        shieldFlashObject.PingRate = shieldDamageFlashRate.Evaluate(0);         // reset shield effect
        personalShieldDamageColor.color = shieldDamageColor.Evaluate(0);

        personalShield.gameObject.SetActive(false);     // disable shield (if not already disabled)
    }

    private void Update()
    {
        // Mouse down, charge up (and activate) pshield
        if (Input.GetMouseButton(0) && GetActiveMenu() == 0 && psHP > 0)
        {
            psRampUp = Mathf.Min(psRampUp + Time.deltaTime, 1);
            if(psRampUp >= psChargeTime)
            {
                ActivatePShield();
            }
        }
        else {
            psRampUp = 0;
            DeactivatePShield();
        }
    }

    public bool IsBlocking()
    {
        return GetActiveShield() > 0;
    }

    public bool IsBlocking(Vector3 damageLocation)
    {
        int active = GetActiveShield();
        if(active == 3)
        {
            // need to actually check if shield radius covers shooting location
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, damageLocation);
            return Vector2.Distance(screenPoint, personalShield.rectTransform.position) <= personalShieldRadiusImage.rectTransform.rect.width;
        }
        else
        {
            return GetActiveShield() > 0;
        }
    }

    // Subtracts the damage from active shield, and deactivates it if necessary
    private void BlockDamage(Fish fish, float fishDamageBlocked)
    {
        int activeShieldID = GetActiveShield();     // So i really hate how i coded this, this whole id thing is adding a lot of overhead
        switch (activeShieldID)
        {
            case 1:
                smShieldHP = Mathf.Max(0, smShieldHP - fishDamageBlocked);
                shopMenuShield.sprite = damageSprites[(int)Mathf.Lerp(damageSprites.Length - 1, 0, smShieldHP / smShieldMaxHP)];        // Set sprite to damaged value
                smShieldDamagePE.Play();                // play particle effect

                if(smShieldHP <= 0)
                {
                    // deactivate shield
                    DeactivateShopMenuShield();
                    shopMenuShield.color *= new Color(1, 1, 1, 0.1f);
                    if(weakMenus)
                        shopMenuShield.gameObject.SetActive(false);             // Kick you out of the menu
                    shopMenuShield.sprite = damageSprites[damageSprites.Length - 1];
                    StartCoroutine(RechargeShopMenuShield());
                }
                break;
            case 2:
                ftShieldHP = Mathf.Max(0, ftShieldHP - fishDamageBlocked);
                fishTankShield.sprite = damageSprites[(int)Mathf.Lerp(damageSprites.Length - 1, 0, ftShieldHP / ftShieldMaxHP)];        // Set sprite to damaged value
                ftShieldDamagePE.Play();        // play particle effect

                if (ftShieldHP <= 0)
                {
                    // deactivate shield
                    DeactivateFishMenuShield();
                    fishTankShield.color *= new Color(1, 1, 1, 0.1f);
                    if (weakMenus)
                        fishTankShield.gameObject.SetActive(false);
                    fishTankShield.sprite = damageSprites[damageSprites.Length - 1];
                    StartCoroutine(RechargeFishMenuShield());
                }
                break;
            case 3:
                psHP = Mathf.Max(0, psHP - fishDamageBlocked);

                // This plays the particle effect at the shield's location
                Ray r = RectTransformUtility.ScreenPointToRay(Camera.main, personalShield.rectTransform.position);      // need ray from screen to world
                pShieldDamagePE.transform.position = r.origin + (r.direction.normalized * 2);                      // move along ray by set amount
                pShieldDamagePE.Play();

                // Now update the pshield graphic depending on damage taken
                float shieldHP = 1 - psHP / personalShieldMaxHP;    // invert damage evaluation
                shieldFlashObject.PingRate = shieldDamageFlashRate.Evaluate(shieldHP);
                personalShieldDamageColor.color = shieldDamageColor.Evaluate(shieldHP);

                if (psHP <= 0)
                {
                    DeactivatePShield();
                    StartCoroutine(RechargePersonalShield());
                }
                break;
        }
    }


    // activates shield if shield inactive, otherwise updates shield graphic
    private void ActivatePShield()
    {
        // rising edge detection
        if (!psActivated)
        {
            psActivated = true;
            personalShield.gameObject.SetActive(true);
        }

        personalShield.rectTransform.position = Input.mousePosition;
        personalShield.fillAmount = psHP / personalShieldMaxHP;
    }


    // Deactivates shield when called if shield is active
    private void DeactivatePShield()
    {
        // falling edge detection (?)
        if (psActivated)
        {
            psActivated = false;

            personalShield.gameObject.SetActive(false);
        }
        
    }

    public void ActivateShopMenuShield()
    {
        DeactivatePShield();     // Deactivate personal shield first

        if (smShieldHP <= 0)
            return;             // Cancel shield activation if health is too low

        // todo: Graphical stuff
    }

    public void DeactivateShopMenuShield()
    {
        // todo: Graphical stuff
    }

    public void ActivateFishMenuShield()
    {
        DeactivatePShield();        // Deactivate personal shield first, let it do its graphical changes then do this shields

        if (ftShieldHP <= 0)
            return;

        // todo: Graphical stuff
    }

    public void DeactivateFishMenuShield()
    {
        // todo: Graphical stuff
    }

    // Returns int representing active menu
    // 0 is nothing, 1 is shopMenu, 2 is tankMenu
    private int GetActiveMenu()
    {
        bool gmenu = shopMenuShield.gameObject.activeInHierarchy;       // Since this is kinda called every frame, i really hope activeInHierarchy is fast
        bool fmenu = fishTankShield.gameObject.activeInHierarchy;

        // dont think theres any benefit checking or returning if both are true at same time
        /*if (gmenu && fmenu)
            return 3;*/
        if (gmenu)
            return 1;
        else if (fmenu)
            return 2;
        else
            return 0;
    }

    // ok this code seems kinda inconsistent but 1 is shop, 2 is fmenu, 3 is personal shield, and 0 is nothing active
    private int GetActiveShield()
    {
        int activeMenu = GetActiveMenu();

        // Personal shield case, no menus active and ps is active and healthy
        if(activeMenu == 0 && psActivated && psHP > 0)
        {
            return 3;
        }
        // Menu shield case, possible menu active and said menu has health
        else if(GetShieldHealthFromId(activeMenu) > 0)
        {
            return activeMenu;
        }

        return 0;
    }

    private float GetShieldHealthFromId(int id)
    {
        // shield priority is shop, tank, then personal
        if(id == 1)
        {
            return smShieldHP;
        }
        else if(id == 2)
        {
            return ftShieldHP;
        }
        else if(id == 3)
        {
            return psHP;
        }

        return -1;      // invalid id, so health is negative (not 0)
    }


    // Reset shield health
    private IEnumerator RechargePersonalShield()
    {
        yield return new WaitForSeconds(personalShieldRechargeTime);

        psHP = personalShieldMaxHP;
        shieldFlashObject.PingRate = shieldDamageFlashRate.Evaluate(0);
        personalShieldDamageColor.color = shieldDamageColor.Evaluate(0);
    }

    private IEnumerator RechargeFishMenuShield()
    {
        float timer = 0;
        fishTankIconRecharge.fillAmount = 1f;

        while(timer < ftShieldRechargeTime)     // Deplete a darkened version of the icon
        {
            timer += Time.deltaTime;
            fishTankIconRecharge.fillAmount = Mathf.InverseLerp(ftShieldRechargeTime, 0, timer);
            yield return null;
        }

        fishTankIconRecharge.fillAmount = 0f;
        ftShieldHP = ftShieldMaxHP;
        fishTankShield.color *= new Color(1, 1, 1, 10);         // Restore color to what it was before breaking
        fishTankShield.sprite = damageSprites[0];

        if (GetActiveMenu() == 2)               // If menu is open, activate it
            ActivateFishMenuShield();
    }

    private IEnumerator RechargeShopMenuShield()
    {
        float timer = 0;
        shopMenuIconRecharge.fillAmount = 1f;

        while (timer < smShieldRechargeTime)
        {
            timer += Time.deltaTime;
            shopMenuIconRecharge.fillAmount = Mathf.InverseLerp(smShieldRechargeTime, 0, timer);
            yield return null;
        }

        shopMenuIconRecharge.fillAmount = 0f;
        smShieldHP = smShieldMaxHP;
        shopMenuShield.color *= new Color(1, 1, 1, 10);
        shopMenuShield.sprite = damageSprites[0];

        if (GetActiveMenu() == 1)       // reactivate shield even if menu is open
            ActivateShopMenuShield();
    }
}

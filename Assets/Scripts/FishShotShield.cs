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
    private Image shopMenuShield;
    [SerializeField]
    private float smShieldMaxHP;
    private float smShieldHP;

    [SerializeField]
    private Image personalShield;
    [SerializeField]
    private float psRadius = 1f;
    [SerializeField]
    private float personalShieldMaxHP;
    private float psHP;
    [SerializeField]
    [Range(0, 1)]
    private float psChargeTime = 0.25f;
    private float psRampUp;
    private bool psActivated;

    [SerializeField]
    private ParticleSystem[] regDamageSubPE;
    [SerializeField]
    private ParticleSystem[] pShieldDamageSubPE;
    [SerializeField]
    private ParticleSystem[] ftShieldDamageSubPE;
    [SerializeField]
    private ParticleSystem[] smShieldDamageSubPE;

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
        psHP = personalShieldMaxHP;

        personalShield.gameObject.SetActive(false);
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
            psRampUp = Mathf.Max(psRampUp - Time.deltaTime, 0);

            if(psRampUp <= 0)
            {
                DeactivatePShield();
            }
        }
    }

    public bool IsBlocking()
    {
        return GetActiveShield() > 0;
    }

    // Subtracts the damage from active shield, and deactivates it if necessary
    private void BlockDamage(Fish fish, float fishDamageBlocked)
    {
        int activeShieldID = GetActiveShield();     // So i really hate how i coded this, this whole id thing is adding a lot of overhead
        switch (activeShieldID)
        {
            case 1:
                smShieldHP = Mathf.Max(0, smShieldHP - fishDamageBlocked);
                // Play particle effect or smth

                if(smShieldHP <= 0)
                {
                    // deactivate shield
                    DeactivateShopMenuShield();
                    shopMenuShield.color *= new Color(1, 1, 1, 0.1f);
                }
                break;
            case 2:
                ftShieldHP = Mathf.Max(0, ftShieldHP - fishDamageBlocked);

                if(ftShieldHP <= 0)
                {
                    // deactivate shield
                    DeactivateFishMenuShield();
                    fishTankShield.color *= new Color(1, 1, 1, 0.1f);
                }
                break;
            case 3:
                psHP = Mathf.Max(0, psHP - fishDamageBlocked);

                if(psHP <= 0)
                {
                    DeactivatePShield();
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

            // disable regular shot particle effects, and enable shield pe
            foreach (ParticleSystem p in regDamageSubPE)
            {
                p.gameObject.SetActive(false);
            }

            foreach (ParticleSystem p2 in pShieldDamageSubPE)
            {
                p2.gameObject.SetActive(true);
            }
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

            // go back to regular particles
            foreach (ParticleSystem p in regDamageSubPE)
            {
                p.gameObject.SetActive(true);
            }

            foreach (ParticleSystem p2 in pShieldDamageSubPE)
            {
                p2.gameObject.SetActive(false);
            }

            personalShield.gameObject.SetActive(false);
        }
        
    }

    public void ActivateShopMenuShield()
    {
        DeactivatePShield();     // Deactivate personal shield first

        if (smShieldHP <= 0)
            return;             // Cancel shield activation if health is too low

        foreach(ParticleSystem p in regDamageSubPE)
        {
            p.gameObject.SetActive(false);
        }

        foreach(ParticleSystem p2 in smShieldDamageSubPE)
        {
            p2.gameObject.SetActive(true);
        }
    }

    public void DeactivateShopMenuShield()
    {
        foreach (ParticleSystem p in regDamageSubPE)
        {
            p.gameObject.SetActive(true);
        }

        foreach (ParticleSystem p2 in smShieldDamageSubPE)
        {
            p2.gameObject.SetActive(false);
        }
    }

    public void ActivateFishMenuShield()
    {
        DeactivatePShield();        // Deactivate personal shield first, let it do its graphical changes then do this shields

        if (ftShieldHP <= 0)
            return;

        foreach (ParticleSystem p in regDamageSubPE)
        {
            p.gameObject.SetActive(false);
        }

        foreach (ParticleSystem p2 in ftShieldDamageSubPE)
        {
            p2.gameObject.SetActive(true);
        }
    }

    public void DeactivateFishMenuShield()
    {
        foreach (ParticleSystem p in regDamageSubPE)
        {
            p.gameObject.SetActive(true);
        }

        foreach (ParticleSystem p2 in ftShieldDamageSubPE)
        {
            p2.gameObject.SetActive(false);
        }
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
}

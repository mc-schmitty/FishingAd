using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FishingFrenzyUIDial : MonoBehaviour
{
    [SerializeField]
    private Image fishIcon;
    [SerializeField]
    private Image colorImage;
    [SerializeField]
    private AnimationCurve animCurve;
    private Color defaultColor;
    private bool isFrenzy = false;

    private void OnEnable()
    {
        FishTank.TriggerFishFrenzy += SetFrenzy;
    }

    private void OnDisable()
    {
        FishTank.TriggerFishFrenzy -= SetFrenzy;
    }

    private void Start()
    {
        defaultColor = colorImage.color;
    }

    private void SetFrenzy(bool frenzy)
    {
        isFrenzy = frenzy;
        colorImage.color = defaultColor;
    }

    private void Update()
    {
        if (isFrenzy)
        {
            float s, v, curve;
            fishIcon.fillAmount = 1;
            curve = animCurve.Evaluate(Time.fixedTime);
            Color.RGBToHSV(colorImage.color, out _, out s, out v);

            colorImage.color = Color.HSVToRGB(curve, s, v);
            colorImage.color = new Color(colorImage.color.r, colorImage.color.g, colorImage.color.b, curve*0.5f+0.25f);
        }
        else
        {
            fishIcon.fillAmount = FishTank.fishTank.fishFrenzyMeter / FishTank.fishTank.fishToTriggerFrenzy;
        }
    }
    
}

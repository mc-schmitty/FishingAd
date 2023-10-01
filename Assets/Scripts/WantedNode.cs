using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WantedNode : MonoBehaviour
{
    [SerializeField]
    private Image fishSprite;      // Fish sprite
    [SerializeField]
    private TextMeshProUGUI wantedText;     // Holds bounty amount
    [SerializeField]
    private Image lineSprite;      // line to cross off node when deleting
    [SerializeField]
    private float animationTimeSeconds = 1f;         // Not sure to use this or rely on TimingInfo
    [SerializeField]
    private AnimationCurve curve;

    private float bountyAmount;

    private void Start()
    {
        StartCoroutine(NodeFadein());
    }

    /// <summary>
    /// Add a fish's sprite and bounty to the UI node.
    /// </summary>
    /// <param name="fish">Fish to be added.</param>
    public void AddFish(Fish fish)
    {
        fishSprite.sprite = fish.GetComponent<SpriteRenderer>().sprite;
        fishSprite.color = Color.black;
        bountyAmount = fish.Bounty;
        StartCoroutine(UpdateTextValue(0, bountyAmount));
    }

    /// <summary>
    /// Update the bounty number displayed by an amount.
    /// </summary>
    /// <param name="amount">Update bounty by this amount.</param>
    public void UpdateBounty(float amount)
    {
        float oldAmount = bountyAmount;
        bountyAmount += amount;
        StartCoroutine(UpdateTextValue(oldAmount, bountyAmount));
    }

    /// <summary>
    /// Display animation and delete node.
    /// </summary>
    public void RemoveNode()
    {
        StartCoroutine(NodeFadeout());
    }

    /// <summary>
    /// Move the node to the specified position.
    /// </summary>
    /// <param name="position">Position in screen coordinates (maybe?)</param>
    public void MoveNode(Vector3 position, bool delay)
    {
        StartCoroutine(SmoothMoveNode(transform.localPosition, position, delay));
    }

    IEnumerator UpdateTextValue(float oldAmount, float newAmount)
    {
        float timer = 0;
        while(timer < animationTimeSeconds)
        {
            wantedText.text = Mathf.Lerp(oldAmount, newAmount, Mathf.InverseLerp(0, animationTimeSeconds, timer)).ToString("n0");
            timer += Time.deltaTime;
            yield return null;
        }

        wantedText.text = newAmount.ToString("n0");
    }

    IEnumerator SmoothMoveNode(Vector3 currentPos, Vector3 intentedPos, bool delayAnimation)
    {
        if (delayAnimation) 
            yield return new WaitForSeconds(animationTimeSeconds);      // Only here to allow time for other actions to occur (ex: crossing off list)

        float timer = 0;
        while(timer < animationTimeSeconds)
        {
            transform.localPosition = Vector3.Lerp(currentPos, intentedPos, Mathf.InverseLerp(0, animationTimeSeconds, timer));
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = intentedPos;
    }

    IEnumerator NodeFadein()
    {
        float timer = 0;
        while (timer < animationTimeSeconds)
        {
            float alphaLerp = Mathf.InverseLerp(0, animationTimeSeconds, timer);
            // update the color of everything
            fishSprite.color = new Color(fishSprite.color.r, fishSprite.color.g, fishSprite.color.b, alphaLerp);
            wantedText.color = new Color(wantedText.color.r, wantedText.color.g, wantedText.color.b, alphaLerp);
            lineSprite.color = new Color(lineSprite.color.r, lineSprite.color.g, lineSprite.color.b, alphaLerp);
            timer += Time.deltaTime;
            yield return null;
        }

        fishSprite.color = new Color(fishSprite.color.r, fishSprite.color.g, fishSprite.color.b, 1);
        wantedText.color = new Color(wantedText.color.r, wantedText.color.g, wantedText.color.b, 1);
        lineSprite.color = new Color(lineSprite.color.r, lineSprite.color.g, lineSprite.color.b, 1);
    }

    IEnumerator NodeFadeout()
    {
        float halfTime = animationTimeSeconds / 2;
        float timer = 0;
        fishSprite.color = Color.white;

        // First draw line
        Transform line = lineSprite.transform;
        while(timer < animationTimeSeconds)
        {
            line.localScale = new Vector3(curve.Evaluate(Mathf.InverseLerp(0, animationTimeSeconds, timer)), line.localScale.y, line.localScale.z);
            timer += Time.deltaTime;
            yield return null;
        }

        // Next quickly fade out the list item
        timer = 0;
        while(timer < halfTime)
        {
            float alphaLerp = Mathf.InverseLerp(0, halfTime, timer);
            // update the color of everything
            fishSprite.color = new Color(fishSprite.color.r, fishSprite.color.g, fishSprite.color.b, Mathf.Lerp(1, 0, alphaLerp));
            wantedText.color = new Color(wantedText.color.r, wantedText.color.g, wantedText.color.b, Mathf.Lerp(1, 0, alphaLerp));
            lineSprite.color = new Color(lineSprite.color.r, lineSprite.color.g, lineSprite.color.b, Mathf.Lerp(1, 0, alphaLerp));
            timer += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);            // Destroy ourself
    }
}

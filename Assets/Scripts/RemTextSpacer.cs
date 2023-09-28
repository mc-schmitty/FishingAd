using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class RemTextSpacer : MonoBehaviour
{
    TextMeshProUGUI self;
    [SerializeField]
    TextMeshProUGUI space1;
    [SerializeField]
    TextMeshProUGUI space2;
    [SerializeField]
    float spacingAmount = -40;

    private Vector3 startPos;
    private Vector3 spacingVector;

    private void Start()
    {
        startPos = transform.position;
        self = GetComponent<TextMeshProUGUI>();
        spacingVector = new Vector3(0, spacingAmount, 0);
    }

    private void Update()
    {
        if(self.color.a > 0)    // check if its visible
        {
            // if other text objects are visible, move this text down by each visible instance * spacing amount
            transform.position = startPos + (space1.color.a > 0 ? spacingVector : Vector3.zero) + (space2.color.a > 0 ? spacingVector : Vector3.zero);
        }
    }
}

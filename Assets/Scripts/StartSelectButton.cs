using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSelectButton : MonoBehaviour
{
    // Select button at start of game
    void Start()
    {
        GetComponent<Button>().Select();
    }
}

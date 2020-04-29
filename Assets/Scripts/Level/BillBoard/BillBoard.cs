using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BillBoard : MonoBehaviour
{
    protected Text text;
    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponentInChildren<Text>();
    }
}

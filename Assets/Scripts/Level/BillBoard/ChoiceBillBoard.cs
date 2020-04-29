using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBillBoard : BillBoard
{
    int count = 0;
    private void Start()
    {
        SetText();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Player")
            count++;
        SetText();
    }
    private void SetText()
    {
        switch(count)
        {
            case 0: text.text = "做个选择吧";
                break;
            case 1:
                text.text = "或许应该选另一个？";
                break;
            case 2:text.text = "或许有第三个选择？";
                break;
        }
    }
}

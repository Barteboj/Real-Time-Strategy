using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatisticGUI : MonoBehaviour
{
    public Image bar;
    public Text amountText;
    
    public void Set(int amount, int maxAmount)
    {
        amountText.text = amount.ToString();
        bar.fillAmount = (float)amount / maxAmount;
    }
}

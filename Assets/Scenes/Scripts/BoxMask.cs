using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoxMask : MonoBehaviour
{
    public int _NumberMask;
    public TextMeshProUGUI _NumberMaskUI;
    private Collider2D Box;
    private void Awake()
    {
        Box = GetComponent<Collider2D>();
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        RessultManagerUI.Instance.Update_NumberMask(this, 1); 
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        RessultManagerUI.Instance.Update_NumberMask(this, -1);
    }
}

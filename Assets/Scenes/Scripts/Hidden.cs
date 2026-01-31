using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hidden : MonoBehaviour
{
    [SerializeField]private Button hidden;

    private void Awake()
    {
        hidden = GetComponent<Button>();
        hidden.onClick.AddListener(OnClick);
    }
    public void OnClick()
    {
        HiddenBoxManager.Instance.Choice(this);
    }
    public void SetFlip(bool isBool)
    {
        hidden.gameObject.SetActive(isBool);
    }
    
}

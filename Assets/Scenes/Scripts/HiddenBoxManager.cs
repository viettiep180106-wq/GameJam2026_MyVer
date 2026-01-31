using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HiddenBoxManager : Singleton<HiddenBoxManager>
{
    public GameObject _OriginalBox;
    [SerializeField]private List<GameObject> _HiddenBox =new List<GameObject>();
    public List<Hidden> _HiddenNoSet = new List<Hidden>();
    public int Turn;
    [SerializeField]private float time, TimeDelay;
    private void Awake()
    {
        TimeDelay = time = 3f;
    }
    private void Start()
    {
        SetUp_LineBox();Turn = 0;
    }
    public void SetUp_LineBox() { 
        for(int i = 0; i < 4; i++)
        {
            GameObject Prefab= Instantiate(_OriginalBox, transform);
            _HiddenBox.Add(Prefab);
            Hidden hidden = Prefab.GetComponentInChildren<Hidden>();
            if (hidden != null)
            {
                _HiddenNoSet.Add(hidden);
            }
        }
    }
    public void Choice (Hidden box)
    {
        if (Turn >= 2)
        {
            return;
        }
        Turn++;
        box.SetFlip(false);
    }
    private void Update()
    {
        if (Turn >= 2)
        {
            if (TimeDelay <= 0f)
            {
                AutoFlip();
            }
            TimeDelay -= Time.deltaTime;
        }
        else return;
    }

    public void AutoFlip()
    {
        foreach (var box in _HiddenNoSet)
        {
            box.SetFlip(false);
        }
        TimeDelay = time;
    }
}

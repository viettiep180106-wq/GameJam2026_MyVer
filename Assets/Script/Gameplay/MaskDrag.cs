// using UnityEngine;
// using DG.Tweening;

// public class MaskDrag : MonoBehaviour
// {
//     private Vector3 _startPos;
//     private Transform _startParent;

//     [Header("Status")]
//     public BoxMaskSlot currentBox;

//     private void Start()
//     {
//         SaveStartState();
//     }

//     public void SaveStartState()
//     {
//         _startPos = transform.position;
//         _startParent = transform.parent;
//     }

//     public void ReturnToStart()
//     {
//         transform.DOMove(_startPos, 0.4f).SetEase(Ease.OutBack);
//         transform.SetParent(_startParent);
//     }
// }
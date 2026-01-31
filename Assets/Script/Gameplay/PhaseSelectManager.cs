using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PhaseSelectManager : Singleton<PhaseSelectManager>
{
    [Header("UI")]
    public Button startButton;
    public Image startImage;
    public Sprite disableSprite;
    public Sprite enableSprite;

    [Header("Box")]
    // [SerializeField] private BoxMaskSlot boxMaskSlot1;
    // [SerializeField] private BoxMaskSlot boxMaskSlot2;
    [SerializeField] private Transform nextPhaseBoxMaskSlot;

    [SerializeField] private Transform maskParent;

    //[SerializeField] private List<MaskDrag> _spawnedMasks = new();

    private int _totalMarkNeeded;
    private int _currentMarkInBox = 0;

    private bool _isSelectionStart;
    public bool IsSelectionStart => _isSelectionStart;

    [Header("Card Settings")]
    [SerializeField] private Transform cardParent;
    [SerializeField] private List<Card> cards;
    [SerializeField] List<GameObject> allCardPrefabs;
    public List<Card> Cards => cards;
    [SerializeField] private int cardsToSpawn = 3;

    // public void Awake()
    // {
    //     startButton.onClick.AddListener(OnStartButton);
    // }

    public void Init()
    {
        // spawn dung 4 card theo CurrentLevelData.Instance.cardsData
        // init card(card data)

        if (GamePlayManager.Instance.CurrentPhase != GamePhase.Select) return;

        _isSelectionStart = false;
        var data = GamePlayManager.Instance.CurrentLevelData;

        if (data == null) return;

        // 1. Khởi tạo Phase (Masks, Boxes)
        InitPhase(data);

        // 2. Khởi tạo Cards
        // Dọn dẹp Card cũ nếu có
        foreach (Transform child in cardParent) Destroy(child.gameObject);
        cards.Clear();

        SpawnRandomCards();

        _isSelectionStart = true;

        DOVirtual.DelayedCall(1f, () =>
        {
            _isSelectionStart = false;
            TransferMasksToPlayPhase();
            GamePlayManager.Instance.GoToPlay();
        });
        // new

    }

    private void SpawnRandomCards()
    {
        // 1. Dọn dẹp
        foreach (Transform child in cardParent) Destroy(child.gameObject);
        cards.Clear();

        if (allCardPrefabs == null || allCardPrefabs.Count == 0) return;

        // 2. Tạo bản sao danh sách để xáo trộn (không làm hỏng library gốc)
        List<GameObject> pool = new List<GameObject>(allCardPrefabs);

        // 3. Xáo trộn danh sách (Fisher-Yates Shuffle)
        for (int i = 0; i < pool.Count; i++)
        {
            int rnd = Random.Range(i, pool.Count);
            GameObject temp = pool[rnd];
            pool[rnd] = pool[i];
            pool[i] = temp;
        }

        // 4. Lấy 3 card đầu tiên sau khi xáo trộn
        int count = Mathf.Min(cardsToSpawn, pool.Count);
        for (int i = 0; i < count; i++)
        {
            GameObject cardGo = Instantiate(pool[i], cardParent);
            Card cardComp = cardGo.GetComponent<Card>();
            if (cardComp != null)
            {
                cards.Add(cardComp);
            }
        }
    }

    // void OnStartButton()
    // {
    //     _isSelectionStart = false;
    //     TransferMasksToPlayPhase();
    //     GamePlayManager.Instance.GoToPlay();
    // }

    public void TransferMasksToPlayPhase()
    {
        // // Lấy danh sách các Mask đang là con của BoxMaskSlot1
        // // Giả định boxSlot1 là RectTransform/Transform đã serialize
        // // MaskDrag[] masksInBox = boxMaskSlot1.GetComponentsInChildren<MaskDrag>();

        // foreach (MaskDrag mask in masksInBox)
        // {
        //     // Lưu lại localPosition hiện tại trước khi đổi cha
        //     Vector3 savedLocalPos = mask.transform.localPosition;

        //     // Đổi sang Parent mới (Masks container của PlayPhase)
        //     mask.transform.SetParent(nextPhaseBoxMaskSlot);

        //     // Gán lại localPosition để giữ đúng vị trí tương đối trong hệ tọa độ mới
        //     mask.transform.localPosition = savedLocalPos;

        //     // Vô hiệu hóa script Drag của Selection Phase nếu cần
        //     // mask.enabled = false;
        // }
    }

    // void OnEnable()
    // {
    //     if (GamePlayManager.Instance.CurrentPhase != GamePhase.Select) return;

    //     _isSelectionStart = false;
    //     // UpdateStartButton(false);
    //     var data = GamePlayManager.Instance.CurrentLevelData;
    //     Debug.Log("0");
    //     if (data != null) InitPhase(data);
    //     _isSelectionStart = true;
    // }

    private void InitPhase(LevelData data)
    {
        // _currentMarkInBox = 0;

        // 1. Setup Boxes
        // boxMaskSlot1.Init(data.maxMaskBox1);
        // boxMaskSlot2.Init(data.maxMaskBox2);
        // _totalMarkNeeded = data.maxMaskBox1 + data.maxMaskBox2;

        // 2. Spawn Masks
        // ClearExistingMasks();

        if (data.maskEntries != null)
        {
            foreach (var entry in data.maskEntries)
            {
                if (entry.maskPrefab == null) continue;
                ObjectMask newMask = Instantiate(entry.maskPrefab);
                newMask.transform.SetParent(maskParent);
                newMask.transform.localPosition = entry.spawnPosition;

                // MaskDrag dragComp = newMask.GetComponent<MaskDrag>();
                // if (dragComp == null) dragComp = newMask.gameObject.AddComponent<MaskDrag>();

                // dragComp.SaveStartState();
                // _spawnedMasks.Add(dragComp);
            }
        }

        // UpdateUI();
    }

    // private void ClearExistingMasks()
    // {
    //     foreach (var m in _spawnedMasks)
    //     {
    //         if (m != null) Destroy(m.gameObject);
    //     }
    //     _spawnedMasks.Clear();
    // }

    // public void OnMarkAdded()
    // {
    //     _currentMarkInBox++;
    //     UpdateUI();
    // }

    // public void OnMarkRemoved()
    // {
    //     _currentMarkInBox--;
    //     UpdateUI();
    // }

    // private void UpdateUI()
    // {
    //     bool canStart = _currentMarkInBox >= _totalMarkNeeded;
    //     startButton.interactable = canStart;
    //     startImage.sprite = canStart ? enableSprite : disableSprite;
    // }

    // Helper để MarkDrag kiểm tra va chạm
    // public List<BoxMaskSlot> GetActiveBoxes()
    // {
    //     return new List<BoxMaskSlot> { boxMaskSlot1, boxMaskSlot2 };
    // }

    // public void OnClickStart()
    // {
    //     GamePlayManager.Instance.ChangePhase(GamePhase.Play);
    // }

    // void UpdateStartButton(bool enable)
    // {
    //     startButton.interactable = enable;
    //     startImage.sprite = enable ? enableSprite : disableSprite;
    // }
}

// using UnityEngine;
// using UnityEngine.UI;

// public class PhaseSelectManager : Singleton<PhaseSelectManager>
// {
//     public Button startButton;
//     public Image startImage;

//     public Sprite disableSprite;
//     public Sprite enableSprite;

//     public int totalMarkNeeded; // tổng số Mark phải đặt

//     int currentMarkInBox = 0;

//     [Header("Load Level")]
//     //[SerializeField] private Transform objectContainer;
//     [SerializeField] private LevelData _currentLevelData;

//     void OnEnable()
//     {
//         UpdateStartButton(false);

//         _currentLevelData = GamePlayManager.Instance.CurrentLevelData;
//         LoadLevel(_currentLevelData); // lay du lieu list mask (kich co) + so slot cua box 1 + so slot cua box 2
//     }

//     public void OnMarkAdded()
//     {
//         currentMarkInBox++;
//         Check();
//     }

//     public void OnMarkRemoved()
//     {
//         currentMarkInBox--;
//         Check();
//     }

//     void Check()
//     {
//         bool canStart = currentMarkInBox >= totalMarkNeeded;
//         UpdateStartButton(canStart);
//     }

//     void UpdateStartButton(bool enable)
//     {
//         startButton.interactable = enable;
//         startImage.sprite = enable ? enableSprite : disableSprite;
//     }
// }

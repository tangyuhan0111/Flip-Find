using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("UI")]
    public Button button;
    public Image frontImage;
    public Image backImage;

    [HideInInspector] public int cardId; // ”√”⁄≈–∂œ∆•≈‰
    public bool IsFlipped { get; private set; }
    public bool IsMatched { get; private set; }

    private MemoryGameManager manager;

    public void Init(int id, Sprite face, MemoryGameManager gm)
    {
        cardId = id;
        frontImage.sprite = face;
        manager = gm;

        IsFlipped = false;
        IsMatched = false;
        ShowBack();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (IsMatched || IsFlipped) return;
        manager.TryFlip(this);
        Debug.Log("CLICK " + name);
    }

    public void ShowFront()
    {
        IsFlipped = true;
        frontImage.gameObject.SetActive(true);
        backImage.gameObject.SetActive(false);
    }

    public void ShowBack()
    {
        IsFlipped = false;
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
    }

    public void SetMatched()
    {
        IsMatched = true;
        button.interactable = false;
    }
}

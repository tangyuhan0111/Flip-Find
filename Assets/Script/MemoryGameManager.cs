using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MemoryGameManager : MonoBehaviour
{
    [Header("Board")]
    public Transform gridParent;
    public CardView cardPrefab;
    public List<Sprite> faceSprites;

    [Header("Grid Settings")]
    [Min(2)] public int gridSize = 4;          // 4 -> 4x4, 6 -> 6x6
    public bool autoApplyGridLayout = true;    // 自动设置列数=gridSize
    public bool autoCellSize = true;           // 自动计算 cell size 让网格铺满
    public bool keepSquareCells = true;        // 让卡牌保持正方形

    [Header("Tuning")]
    public float previewSeconds = 3f;
    public float mismatchHideDelay = 0.6f;

    [Header("Audio (optional)")]
    public AudioSource sfxSource;
    public AudioClip matchClip;
    public AudioClip mismatchClip;

    private CardView first;
    private CardView second;
    private bool inputLocked;

    [Header("Win UI")]
    public GameObject winPanel;
    private int matchedPairs = 0;
    private int totalPairs = 0;

    void Start()
    {
        StartLevel();
    }

    // 如果你是“点 Start 按钮才开始”，就把 Start() 里的 StartLevel() 删掉
    // 然后在按钮 OnClick() 里调用 StartLevel()
    public void StartLevel()
    {
        int totalCards = gridSize * gridSize;

        if (totalCards % 2 != 0)
        {
            Debug.LogError($"gridSize {gridSize} -> {totalCards} cards (odd). Must be even for pairs.");
            return;
        }

        int pairCount = totalCards / 2;

        if (faceSprites == null || faceSprites.Count < pairCount)
        {
            Debug.LogError($"Not enough faceSprites. Need at least {pairCount}, but got {faceSprites?.Count ?? 0}.");
            return;
        }

        if (autoApplyGridLayout || autoCellSize)
            ApplyGridLayoutAndSizing(gridSize);

        matchedPairs = 0;
        totalPairs = pairCount;

        if (winPanel != null) winPanel.SetActive(false);

        ClearBoard();
        BuildBoard(pairCount);
        StartCoroutine(PreviewThenHide());
    }

    void ApplyGridLayoutAndSizing(int n)
    {
        var grid = gridParent.GetComponent<GridLayoutGroup>();
        var rect = gridParent.GetComponent<RectTransform>();

        if (grid == null || rect == null) return;

        if (autoApplyGridLayout)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = n;
        }

        if (!autoCellSize) return;

        // 可用区域 = RectTransform 的宽高 - padding - spacing
        float width = rect.rect.width;
        float height = rect.rect.height;

        float padW = grid.padding.left + grid.padding.right;
        float padH = grid.padding.top + grid.padding.bottom;

        float spaceW = grid.spacing.x * (n - 1);
        float spaceH = grid.spacing.y * (n - 1);

        float availW = width - padW - spaceW;
        float availH = height - padH - spaceH;

        if (availW <= 0 || availH <= 0) return;

        float cellW = availW / n;
        float cellH = availH / n;

        if (keepSquareCells)
        {
            float s = Mathf.Min(cellW, cellH);
            grid.cellSize = new Vector2(s, s);
        }
        else
        {
            grid.cellSize = new Vector2(cellW, cellH);
        }
    }

    void ClearBoard()
    {
        StopAllCoroutines();
        first = null;
        second = null;
        inputLocked = false;

        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);
    }

    void BuildBoard(int pairCount)
    {
        var ids = new List<int>(pairCount * 2);
        for (int i = 0; i < pairCount; i++)
        {
            ids.Add(i);
            ids.Add(i);
        }

        Shuffle(ids);

        for (int i = 0; i < ids.Count; i++)
        {
            int id = ids[i];
            var cv = Instantiate(cardPrefab, gridParent);
            cv.Init(id, faceSprites[id], this);
        }
    }

    IEnumerator PreviewThenHide()
    {
        inputLocked = true;

        foreach (Transform t in gridParent)
            t.GetComponent<CardView>().ShowFront();

        yield return new WaitForSeconds(previewSeconds);

        foreach (Transform t in gridParent)
            t.GetComponent<CardView>().ShowBack();

        inputLocked = false;
    }

    public void TryFlip(CardView card)
    {
        if (inputLocked) return;

        card.ShowFront();

        if (first == null)
        {
            first = card;
            return;
        }

        second = card;
        inputLocked = true;
        StartCoroutine(CheckMatch());
    }

    IEnumerator CheckMatch()
    {
        if (first.cardId == second.cardId)
        {
            first.SetMatched();
            second.SetMatched();
            PlaySfx(matchClip);

            matchedPairs++;
            if (matchedPairs >= totalPairs)
            {
                ShowWin();
            }
            ResetTurn();
            inputLocked = false;
        }
        else
        {
            PlaySfx(mismatchClip);
            yield return new WaitForSeconds(mismatchHideDelay);

            first.ShowBack();
            second.ShowBack();

            ResetTurn();
            inputLocked = false;
        }
    }

    void ResetTurn()
    {
        first = null;
        second = null;
    }

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void ShowWin()
    {
        inputLocked = true; // 禁止继续翻牌
        if (winPanel != null) winPanel.SetActive(true);
    }

    public void GoToNextLevel()
    {
        // SceneManager.LoadScene("Level2Scene");
        StartLevel();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

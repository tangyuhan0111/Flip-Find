using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MemoryGameManager : MonoBehaviour
{
    [Header("Board")]
    public Transform gridParent;
    public CardView cardPrefab;
    public List<Sprite> faceSprites;

    [Header("Grid Settings")]
    [Min(2)] public int gridSize = 4; 
    public bool autoApplyGridLayout = true;    
    public bool autoCellSize = true;          
    public bool keepSquareCells = true;  

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

    [Header("Timer UI")]
    public TMP_Text timerText;        // HUD
    public TMP_Text winTimeText;      // WinPanel 上显示最终时间

    private float elapsedTime;
    private bool timerRunning;

    [Header("Stats UI (TMP)")]
    public TMP_Text attemptsText;     // HUD 可选（不想显示就留空）
    public TMP_Text matchesText;      // HUD 可选
    public TMP_Text accuracyText;     // HUD 可选

    public TMP_Text winAttemptsText;  // WinPanel
    public TMP_Text winMatchesText;
    public TMP_Text winAccuracyText;

    private int attempts;   // 翻开两张算一次
    private int matches;    // 配对成功次数

    void Start()
    {
        StartLevel();
    }

    void Update()
    {
        if (!timerRunning) return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartLevel()
    {
        if (winTimeText != null)
            winTimeText.text = "";

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

        attempts = 0;
        matches = 0;
        UpdateStatsUI();

        if (winAttemptsText != null) winAttemptsText.text = "";
        if (winMatchesText != null) winMatchesText.text = "";
        if (winAccuracyText != null) winAccuracyText.text = "";

        elapsedTime = 0f;
        timerRunning = true;
        UpdateTimerUI(); // 立刻刷新一次显示

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
        if (card == first) return;
        if (inputLocked) return;

        card.ShowFront();

        if (first == null)
        {
            first = card;
            return;
        }

        second = card;
        attempts++;          // ✅ 翻开两张算一次尝试
        UpdateStatsUI();     // 可选：立刻刷新 HUD
        inputLocked = true;
        StartCoroutine(CheckMatch());
    }

    IEnumerator CheckMatch()
    {
        if (first.cardId == second.cardId)
        {
            first.SetMatched();
            second.SetMatched();

            matches++;
            UpdateStatsUI();

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
        inputLocked = true;
        timerRunning = false;
        if (winPanel != null) winPanel.SetActive(true);
        if (winTimeText != null)
        {
            int totalSeconds = Mathf.FloorToInt(elapsedTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            winTimeText.text = $"Time: {minutes:00}:{seconds:00}";
        }
        float acc = (attempts > 0) ? (matches / (float)attempts) : 0f;
        int accPercent = Mathf.RoundToInt(acc * 100f);

        if (winAttemptsText != null) winAttemptsText.text = $"Attempts: {attempts}";
        if (winMatchesText != null) winMatchesText.text = $"Matches: {matches}";
        if (winAccuracyText != null) winAccuracyText.text = $"Accuracy: {accPercent}%";
    }

    public void GoToNextLevel()
    {
        SceneManager.LoadScene("Level2Scene");
        // StartLevel();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void UpdateStatsUI()
    {
        float acc = (attempts > 0) ? (matches / (float)attempts) : 0f;
        int accPercent = Mathf.RoundToInt(acc * 100f);

        if (attemptsText != null) attemptsText.text = $"Attempts: {attempts}";
        if (matchesText != null) matchesText.text = $"Matches: {matches}";
        if (accuracyText != null) accuracyText.text = $"Accuracy: {accPercent}%";
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level1CardPreview : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 4;
    public int columns = 4;

    [Header("Card Prefab")]
    public GameObject cardPrefab;      // 拖 CardPrefab 进来

    [Header("Card Sprites (至少 8 张不重复的正面图)")]
    public Sprite[] cardSprites;       // 在 Inspector 里拖 8 个不同鸭子图

    private void Start()
    {
        int totalCards = rows * columns;
        int pairCount = totalCards / 2;

        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned!");
            return;
        }

        if (cardSprites == null || cardSprites.Length < pairCount)
        {
            Debug.LogError("Not enough card sprites! Need at least " + pairCount);
            return;
        }

        // 1. 生成一个 sprite 列表：每个图案两张（A, A, B, B, ...）
        List<Sprite> spritesToUse = new List<Sprite>();
        for (int i = 0; i < pairCount; i++)
        {
            spritesToUse.Add(cardSprites[i]);
            spritesToUse.Add(cardSprites[i]);
        }

        // 2. 打乱顺序（洗牌）
        for (int i = 0; i < spritesToUse.Count; i++)
        {
            int randomIndex = Random.Range(i, spritesToUse.Count);
            (spritesToUse[i], spritesToUse[randomIndex]) = (spritesToUse[randomIndex], spritesToUse[i]);
        }

        // 3. 实例化卡片并设置图片
        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, transform); // 以 CardGrid 为父对象
            Image img = cardObj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = spritesToUse[i]; // 正面图
            }
        }
    }
}

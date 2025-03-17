using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI coinText; // UI text to show coins

    void Start()
    {
        // 🔥 Get saved coins from PlayerPrefs
        int collectedCoins = PlayerPrefs.GetInt("SavedCoins", 0);
        coinText.text = "Coins Collected: " + collectedCoins;
        Debug.Log($"🎉 Coins from previous scene: {collectedCoins}");
    }

}


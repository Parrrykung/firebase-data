using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;

public class CoinCollection : MonoBehaviour
{
    // Firebase components
    public FirebaseAuth auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;
    
    // Coin management
    public static int Coin { get; set; } // Allows both read & write access
    public TextMeshProUGUI coinText;
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        User = auth.CurrentUser; // ✅ Set the user when the script starts

        if (User == null)
        {
            Debug.LogWarning("⚠️ No user detected. Make sure you're logged in before saving scores.");
        }
        else
        {
            Debug.Log($"✅ User logged in: {User.DisplayName} (UID: {User.UserId})");
        }

        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("COIN"))
        {
            Coin++;
            UpdateUI();
            Destroy(collision.gameObject);
            Debug.Log($"Coin collected! Total: {Coin}");
            
            // Auto-save when reaching milestones
            if (Coin % 5 == 0)  // Save every 5 coins
            {
                SaveScoreToFirebase(Coin);
            }
        }
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = $"{Coin}/10";
        }
    }

    public void SaveScoreToFirebase(int newScore)
    {
        if (User == null)
        {
            Debug.LogWarning("❌ User not logged in! Cannot save score.");
            return;
        }

        string username = string.IsNullOrEmpty(User.DisplayName) ? "Guest" : User.DisplayName;

        var userEntry = new Dictionary<string, object>
        {
            { "username", username },
            { "scores", newScore }
        };

        DBreference.Child("users").Child(User.UserId).SetValueAsync(userEntry)
            .ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"✅ Score updated in Firebase! Username: {username}, Score: {newScore}");
                }
                else
                {
                    Debug.LogWarning($"❌ Failed to update score: {task.Exception}");
                }
            });
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;

public class Exit3 : MonoBehaviour
{
    private DatabaseReference dbReference;
    private bool isExiting = false; // Prevent multiple exits

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference; // Initialize Firebase Database
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isExiting)
        {
            isExiting = true;
            StartCoroutine(SaveDataAndExit());
        }
    }

    IEnumerator SaveDataAndExit()
    {
        int finalCoins = CoinCollection.Coin; // ✅ Get coins before resetting

        // 🔥 Save to PlayerPrefs (to send to next scene)
        PlayerPrefs.SetInt("SavedCoins", finalCoins);
        PlayerPrefs.Save(); // 🔹 Make sure it is stored

        // 🔥 Get Firebase User
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("❌ No user is logged in. Cannot save score.");
            SceneManager.LoadScene("Clear 1"); // Load scene even if Firebase fails
            yield break;
        }

        string userId = user.UserId;
        string username = string.IsNullOrEmpty(user.DisplayName) ? "Guest" : user.DisplayName;

        // 🔥 Prepare data to save
        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "username", username },
            { "score", finalCoins },
            { "coins", finalCoins },
            { "timestamp", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") } // 🔹 Standard format
        };

        // ✅ Save to Firebase
        var saveTask = dbReference.Child("users").Child(userId).SetValueAsync(playerData);
        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.Exception != null)
        {
            Debug.LogError($"❌ Failed to save data: {saveTask.Exception}");
        }
        else
        {
            Debug.Log($"✅ Data saved: Username: {username}, Score: {finalCoins}");
        }

        // ✅ Reset coin count after saving
        CoinCollection.Coin = 0;

        // ✅ Load next scene
        SceneManager.LoadScene("Clear 3");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
using System.Linq;

public class Scoreboard : MonoBehaviour
{
    public Transform scoreboardContent;
    public GameObject scoreElementPrefab;
    
    private DatabaseReference DBreference;
    
    void Start()
    {
        StartCoroutine(InitializeFirebase());
    }

    private IEnumerator InitializeFirebase()
    {
        Debug.Log("⏳ Initializing Firebase...");

        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.Exception != null)
        {
            Debug.LogError($"❌ Firebase dependency check failed: {dependencyTask.Exception}");
            yield break;
        }

        FirebaseApp app = FirebaseApp.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;

        Debug.Log("✅ Firebase Initialized Successfully!");

        // Now that Firebase is ready, load scoreboard data
        StartCoroutine(LoadScoreboardData());
    }

    private IEnumerator LoadScoreboardData()
    {
        Debug.Log("📡 Fetching scoreboard data...");

        if (DBreference == null)
        {
            Debug.LogError("❌ Database reference is null! Ensure Firebase is initialized.");
            yield break;
        }

        Task<DataSnapshot> DBTask = DBreference.Child("users").OrderByChild("score").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"❌ Failed to load scoreboard: {DBTask.Exception}");
            yield break;
        }

        DataSnapshot snapshot = DBTask.Result;

        if (!snapshot.Exists)
        {
            Debug.LogWarning("⚠️ No users found in Firebase.");
            yield break;
        }

        Debug.Log($"✅ Data received: {snapshot.ChildrenCount} users found.");

        foreach (Transform child in scoreboardContent)
        {
            Destroy(child.gameObject);
        }
        yield return null;

        List<DataSnapshot> sortedUsers = snapshot.Children.ToList();

        sortedUsers.Sort((a, b) =>
        {
            int scoreA = 0, scoreB = 0;
            int.TryParse(a.Child("score").Value?.ToString(), out scoreA);
            int.TryParse(b.Child("score").Value?.ToString(), out scoreB);
            return scoreB.CompareTo(scoreA);
        });

        int rank = 1;

        foreach (DataSnapshot userSnapshot in sortedUsers)
        {
            if (!userSnapshot.HasChild("username") || !userSnapshot.HasChild("score"))
            {
                Debug.LogWarning("⚠️ Skipping entry: Missing username or score.");
                continue;
            }

            string username = userSnapshot.Child("username").Value.ToString();
            int score;
            if (!int.TryParse(userSnapshot.Child("score").Value.ToString(), out score))
            {
                score = 0;
            }

            GameObject newElement = Instantiate(scoreElementPrefab, scoreboardContent);
            ScoreElement scoreElement = newElement.GetComponent<ScoreElement>();
            scoreElement.NewScoreElement(username, score, rank);

            Debug.Log($"🏆 Rank {rank}: {username} - {score}");

            rank++;
        }
    }
}

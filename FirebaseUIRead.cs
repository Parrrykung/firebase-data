using UnityEngine;
using Firebase;
using Firebase.Database;
using UnityEngine.UI;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;
using TMPro; 

public class FirebaseUIRead : MonoBehaviour
{
    private DatabaseReference databaseRef;
   // public Text leaderboardText; // Assign in Inspector
   public TextMeshProUGUI leaderboardText; 
    private SynchronizationContext mainThreadContext;

    void Start()
    {

        leaderboardText.text = "Test";
        // Capture the main thread's context
        mainThreadContext = SynchronizationContext.Current;

        // Initialize Firebase and check for dependencies
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                
                Debug.Log("✅ Firebase Dependencies Available");
                //leaderboardText.text = "Firebase Dependencies Available";
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("❌ Firebase dependencies are not resolved: " + task.Result);
            }
        });
    }

    void InitializeFirebase()
    {
        //leaderboardText.text = "Firebase Initialized Start";

        // Firebase Initialization
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference.Child("leaderboard");
        Debug.Log("✅ Firebase Initialized Successfully");

        Debug.Log("✅ Before  Updating UI: Firebase Initialized Successfully" );
        //leaderboardText.text = "Before  Updating UI: Firebase Initialized Successfully";

        // บังคับให้การอัปเดตข้อความรันบน Main Thread
        mainThreadContext.Post(_ => {
            leaderboardText.text = "After Updating UI: Firebase Initialized Successfully";
            Debug.Log("✅ After Updating UI: " + leaderboardText.text);
        }, null);


        ReadAndDisplayData();
    }
    
    public void ReadAndDisplayData()
    {
        databaseRef.GetValueAsync().ContinueWith(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    Debug.LogError("❌ No data found in Firebase!");
                    return;
                }

                Debug.Log("✅ Firebase Data Retrieved Successfully");
                ///leaderboardText.text = "Firebase Data Retrieved Successfully";

                StringBuilder leaderboardString = new StringBuilder("Leaderboard:\n");

                foreach (var child in snapshot.Children)
                {
                    if (child.Child("score").Exists)
                    {
                        string playerName = child.Key;
                        int playerScore = int.Parse(child.Child("score").Value.ToString());
                        leaderboardString.AppendLine($"{playerName}: {playerScore}");
                    }
                }

                Debug.Log("📝 Updating UI with Data: " + leaderboardString.ToString());
               
                // บังคับให้อัปเดต UI บน Main Thread
                // mainThreadContext.Post(_ => {
                //     leaderboardText.text = leaderboardString.ToString();
                //     Debug.Log("✅ Updating UI with Data: " + leaderboardString.ToString());
                // }, null);

                // บังคับให้รันบน Main Thread
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    leaderboardText.text = leaderboardString.ToString();
                    Debug.Log("✅ UI Updated Successfully");
                });
            }
            else
            {
                Debug.LogError("❌ Firebase Read Failed: " + task.Exception);
            }
        });
    }
}

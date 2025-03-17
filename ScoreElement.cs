using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ScoreElement : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text usernameText;
    public TMP_Text scoreText;

    public void NewScoreElement(string username, int score, int rank)
    {
        rankText.text = rank + ".";   // Show Rank
        usernameText.text = username; // Show Username
        scoreText.text = score.ToString(); // Show Score
    }
}
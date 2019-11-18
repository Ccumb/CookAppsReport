using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ScoreManager : MonoBehaviour
{
    public Text scoreText;
    public Text obstacleCountText;

    public int score;
    public int obstacleCount;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score.ToString();
        obstacleCountText.text = obstacleCount.ToString();
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
    }

    public void UpdateObstacleCount(int amount)
    {
        obstacleCount = amount;
    }
}

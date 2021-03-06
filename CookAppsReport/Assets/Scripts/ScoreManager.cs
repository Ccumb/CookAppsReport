﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
// 점수. 화면에 보여주는 것 담당 UI
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

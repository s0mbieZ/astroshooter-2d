﻿using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject spaceshipPrefab;
    public GameObject startingRockPrefab;
    public GameObject saucerPrefab;
    public GameObject gameUI;
    public GameObject mainUI;
    public GameObject gameOverUI;
    public GameObject finalScoreText;
    public GameObject scoreText;
    public GameObject livesText;

    public enum gameState { main, game, gameOver };
    public gameState state;

    public int playerLives = 3;
    public int score = 0;
    public int numStartingRocks = 4;

    public float saucerSpawnRate = 10f;

    private GameObject player;

    private Vector3 screenSW;
    private Vector3 screenNE;
    private Vector3 screenSE;
    private Vector3 screenNW;

    private Spaceship spaceship;

    private int rockSpawnRadius = 4;
    private int startingScore;
    private int startingLives;

    // Use a script object for easier calling?

	// Use this for initialization
	void Start () {

        mainUI.SetActive(false);
        gameUI.SetActive(false);
        gameOverUI.SetActive(false);

        switch(state)
        {
            case gameState.main:
                mainUI.SetActive(true);
                break;

            case gameState.game:
                gameUI.SetActive(true);
                break;

            case gameState.gameOver:
                gameOverUI.SetActive(true);
                break;
        }

        startingScore = score;
        startingLives = playerLives;
        UpdateScore(0);
        UpdateLives(0);
	}
	
	// Update is called once per frame
	void Update () {

        switch(state)
        {
            case gameState.main:

                if(Input.GetKeyDown(KeyCode.Return))
                {
                    StartCoroutine(GameStart());
                }

                break;

            case gameState.gameOver:

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    GameObject[] rocksToDestroy = GameObject.FindGameObjectsWithTag("Rock");
                    for(int i = 0; i < rocksToDestroy.Length; i++)
                    {
                        Destroy(rocksToDestroy[i]);
                    }

                    StartCoroutine(GameStart());
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Application.Quit();
                }

                break;

            case gameState.game:
                {
                    // Checking for player input on both axes. Value of 1 or -1 for each.
                    float translation = Input.GetAxis("Vertical");
                    float rotation = Input.GetAxis("Horizontal");

                    // Switch to 0.1?
                    if (rotation > 0.1) // When pushing the right arrow.
                    {
                        spaceship.TurnRight(rotation);
                    }

                    if (rotation < 0.1) // When pushing the left arrow.
                    {
                        spaceship.TurnLeft(rotation);
                    }

                    if (translation >= 0.5f) // When up arrow is pushed.
                    {
                        // This will actually just set the acceleration rate based on the input (1 or 0). Also sets the State in Mechanim.
                        spaceship.Move(translation);
                    }
                    else
                    {
                        // No movement keys being pressed calls the Idle() function.
                        spaceship.Idle();
                    }

                    // If the player hits the spacebar shoot some lasers.
                    if (Input.GetButton("Jump"))
                        spaceship.ShootBullet();

                    if (Input.GetButton("Fire1"))
                    {
                        spaceship.Warp();
                    }

                    GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");
                    if (rocks.Length <= 0)
                    {
                        for (int i = 0; i < numStartingRocks; i++)
                        {
                            float rockPosX = rockSpawnRadius * Mathf.Cos(Random.Range(0, 360));
                            float rockPosy = rockSpawnRadius * Mathf.Sin(Random.Range(0, 360));

                            GameObject rockClone = Instantiate(startingRockPrefab, new Vector3(rockPosX, rockPosy, 0), Quaternion.identity) as GameObject;

                            rockClone.GetComponent<Rock>().SetGameManager(this.gameObject);
                        }
                    }

                    break;
                }
        }
	}

    public void ResetShip()
    {
        player.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.GetComponent<GUIText>().text = "Score: " + score;
    }

    public void UpdateLives(int livesLost)
    {
        playerLives -= livesLost;
        livesText.GetComponent<GUIText>().text = "Lives: " + playerLives;

        if(playerLives < 1)
        {
            StartCoroutine(GameEnd());
        }
    }

    IEnumerator SaucerSpawn()
    {
        for(float timer = saucerSpawnRate; timer >= 0; timer -= Time.deltaTime)
        {
            yield return null;
        }

        int cornerSelection = Random.Range(0, 4);

        Vector3 saucerSpawnPos = new Vector3(0,0,0);

        if(cornerSelection == 0)
        {
            saucerSpawnPos = new Vector3(screenSW.x, screenSW.y, 0);
        }
        else if(cornerSelection == 1)
        {
            saucerSpawnPos = new Vector3(screenSE.x, screenSE.y, 0);
        }
        else if(cornerSelection == 2)
        {
            saucerSpawnPos = new Vector3(screenNE.x, screenNE.y, 0);
        }
        else if(cornerSelection == 3)
        {
            saucerSpawnPos = new Vector3(screenNW.x, screenNW.y, 0);
        }

        GameObject saucerClone = Instantiate(saucerPrefab, saucerSpawnPos, Quaternion.identity) as GameObject;
        saucerClone.GetComponent<Saucer>().SetGameManager(this.gameObject);

        if (cornerSelection == 0)
        {
            saucerClone.transform.Rotate(Vector3.back * Random.Range(0, 90));
        }
        else if (cornerSelection == 1)
        {
            saucerClone.transform.Rotate(Vector3.back * Random.Range(90, 180));
        }
        else if (cornerSelection == 2)
        {
            saucerClone.transform.Rotate(Vector3.back * Random.Range(180, 270));
        }
        else if (cornerSelection == 3)
        {
            saucerClone.transform.Rotate(Vector3.back * Random.Range(270, 360));
        }

        StartCoroutine(SaucerSpawn());
    }
    
    IEnumerator GameStart()
    {
        mainUI.SetActive(false);
        gameUI.SetActive(true);
        gameOverUI.SetActive(false);
        state = gameState.game;
        player = Instantiate(spaceshipPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        spaceship = player.GetComponent<Spaceship>();
        spaceship.SetGameManager(this.gameObject);

        for (int i = 0; i < numStartingRocks; i++)
        {
            float rockPosX = rockSpawnRadius * Mathf.Cos(Random.Range(0, 360));
            float rockPosy = rockSpawnRadius * Mathf.Sin(Random.Range(0, 360));

            GameObject rockClone = Instantiate(startingRockPrefab, new Vector3(rockPosX, rockPosy, 0), Quaternion.identity) as GameObject;

            rockClone.GetComponent<Rock>().SetGameManager(this.gameObject);
        }

        // ScreenSE and ScreenNW are incorrectly named. Change in the future.
        screenSW = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.transform.localPosition.z));
        screenNE = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.localPosition.z));
        screenSE = new Vector3(screenSW.x, screenNE.y, 0);
        screenNW = new Vector3(screenNE.x, screenSW.y, 0);

        StartCoroutine(SaucerSpawn());

        yield return null;
    }

    IEnumerator GameEnd()
    {
        mainUI.SetActive(false);
        gameUI.SetActive(false);
        gameOverUI.SetActive(true);
        state = gameState.gameOver;

        finalScoreText.GetComponent<GUIText>().text = "Final Score: " + score;

        Destroy(player);
        StopAllCoroutines();

        score = startingScore;
        playerLives = startingLives;

        yield return null;
    }
}
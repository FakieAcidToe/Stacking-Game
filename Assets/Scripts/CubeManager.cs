using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CubeManager : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] CubeLayer cubePrefab; // for spawning new cubes
	[SerializeField] QuadGrowEffect quadPrefab; // for perfect stack effect
	[SerializeField] CubeLayer currentCube; // the top cube of the stack
	[SerializeField] Camera cam;
	[SerializeField] TextMeshProUGUI scoreUI;
	[SerializeField] TextMeshProUGUI highscoreUI;
	[SerializeField] TextMeshProUGUI maxComboUI;
	[SerializeField] Button playButton;
	[SerializeField] Button retryButton;
	[SerializeField] Button continueButton;

	[Header("Audio")]
	[SerializeField] AudioClip musicIntro;
	[SerializeField] AudioClip musicLoop;
	[SerializeField] AudioClip perfectSfx;
	[SerializeField] AudioClip placeSfx;
	[SerializeField] AudioClip startSfx;
	[SerializeField] AudioClip growSfx;

	[Header("Cube spawn variables")]
	[SerializeField] float spawnPos = -2; // for x and z coords of where the new cubes spawn
	[SerializeField] Color cubeColour; // spawns new cubes with the saturation and value of this colour
	[SerializeField] float perfectOffset = 0.025f; // if stacked cube close enough to perfect, make it perfect 0
	[SerializeField] int perfectConsecutive = 5; // grow cube if perfect in a row
	[SerializeField] float perfectGrow = 0.1f; // grow cube amount

	uint score;
	uint maxCombo;
	uint highscore;

	float cubeHue;
	float cubeSaturation;
	float cubeValue;

	CubeLayer movingCube; // the cube that's controlled
	CubeLayer firstCube;
	bool spawnX = false; // toggles betweel X or Z axis when spawning a new cube
	float cameraOffsetY; // controls the camera height relative to currentCube; used in MoveCameraToHeight coroutine
	float cameraDefaultSize; // cam.orthographicSize start size
	uint perfectCounter;

	enum GameState
	{
		START,
		GAMEPLAY,
		GAMEOVER,
	}

	GameState state = GameState.START; // handles gameplay state in update


	void Start()
	{
		SoundManager.Instance.PlayMusic(musicLoop, musicIntro);

		cameraOffsetY = cam.transform.position.y - currentCube.transform.position.y;
		cameraDefaultSize = cam.orthographicSize;

		float h;
		Color.RGBToHSV(cubeColour, out h, out cubeSaturation, out cubeValue);

		firstCube = currentCube;

		InitGame();
	}

	void InitGame()
	{
		// score
		score = 0;
		maxCombo = 0;
		scoreUI.text = score.ToString();
		perfectCounter = 0;

		// init colours
		cubeHue = Random.Range(0f, 100f);
		currentCube.SetColHSV(cubeHue / 100f, cubeSaturation, cubeValue);
	}

	void Update()
	{
		if (state == GameState.GAMEPLAY)
		{
			if (Input.GetButtonDown("Fire1"))
			{
				if (movingCube.FreezeSplit(currentCube, perfectOffset)) // split cube
				{
					if (Equals(movingCube.transform.localScale, currentCube.transform.localScale)) // perfect stack
					{
						if (++perfectCounter % perfectConsecutive == 0) // grow cube if perfect combo
						{
							SoundManager.Instance.Play(growSfx, movingCube.transform.position);
							movingCube.transform.localScale = new Vector3(
								movingCube.transform.localScale.x + perfectGrow,
								movingCube.transform.localScale.y,
								movingCube.transform.localScale.z + perfectGrow);
							//movingCube.transform.position = new Vector3(
							//	movingCube.transform.position.x - Mathf.Sign(movingCube.transform.position.x) * perfectGrow/2f,
							//	movingCube.transform.position.y,
							//	movingCube.transform.position.z - Mathf.Sign(movingCube.transform.position.z) * perfectGrow/2f);
						}
						else
							SoundManager.Instance.Play(perfectSfx, movingCube.transform.position);

						// combo score
						if (perfectCounter > maxCombo) maxCombo = perfectCounter;

						// perfect stack quad effect
						QuadGrowEffect quad = Instantiate(quadPrefab, new Vector3(
							currentCube.transform.position.x,
							currentCube.transform.position.y + currentCube.transform.localScale.y / 2,
							currentCube.transform.position.z), Quaternion.AngleAxis(90, Vector3.right));
						quad.transform.localScale = new Vector3(
							movingCube.transform.localScale.x,
							movingCube.transform.localScale.z,
							1f);
					}
					else
						perfectCounter = 0;
					SoundManager.Instance.Play(placeSfx, movingCube.transform.position);

					currentCube.enabled = false;
					currentCube = movingCube;
					SpawnCube();

					// ui
					++score;
					if (score > highscore) highscore = score;
					scoreUI.text = score.ToString();

					StopAllCoroutines();
					StartCoroutine(MoveCameraToHeight(currentCube.transform.position.y + cameraOffsetY));
				}
				else // game over
				{
					state = GameState.GAMEOVER;

					StopAllCoroutines();
					StartCoroutine(MoveCameraToHeight(currentCube.transform.position.y / 2 + cameraOffsetY));
					StartCoroutine(MoveCameraToSize(cameraDefaultSize + score / 10f));

					retryButton.gameObject.SetActive(true);
					continueButton.gameObject.SetActive(true);
					highscoreUI.gameObject.SetActive(true);
					highscoreUI.text = "Highscore: " + highscore.ToString();
					maxComboUI.gameObject.SetActive(true);
					maxComboUI.text = "Max Combo: " + maxCombo.ToString();
				}
			}
		}
	}

	public void StartGame()
	{
		if (state == GameState.START)
		{
			SpawnCube();
			state = GameState.GAMEPLAY;
			SoundManager.Instance.Play(startSfx);

			playButton.gameObject.SetActive(false);
		}
	}

	public void ContinueGame()
	{
		if (state == GameState.GAMEOVER)
		{
			StopAllCoroutines();
			StartCoroutine(MoveCameraToHeight(currentCube.transform.position.y + cameraOffsetY));
			StartCoroutine(MoveCameraToSize(cameraDefaultSize));

			retryButton.gameObject.SetActive(false);
			continueButton.gameObject.SetActive(false);
			continueButton.interactable = false;
			highscoreUI.gameObject.SetActive(false);
			maxComboUI.gameObject.SetActive(false);

			SpawnCube();
			state = GameState.GAMEPLAY;
			SoundManager.Instance.Play(startSfx);
		}
	}

	public void RetryGame()
	{
		if (state == GameState.GAMEOVER)
		{
			// delete all layers (except the first layer)
			currentCube = firstCube;
			foreach (CubeLayer layer in FindObjectsOfType(typeof(CubeLayer))) if (layer != firstCube)
					Destroy(layer.gameObject);

			// set camera
			StopAllCoroutines();
			StartCoroutine(MoveCameraToHeight(currentCube.transform.position.y + cameraOffsetY));
			StartCoroutine(MoveCameraToSize(cameraDefaultSize));

			retryButton.gameObject.SetActive(false);
			continueButton.gameObject.SetActive(false);
			continueButton.interactable = true;
			highscoreUI.gameObject.SetActive(false);
			maxComboUI.gameObject.SetActive(false);

			// reset colours and score
			InitGame();

			SpawnCube();
			state = GameState.GAMEPLAY;
			SoundManager.Instance.Play(startSfx);
		}
	}

	void SpawnCube()
	{
		// position and scale
		movingCube = Instantiate(cubePrefab, new Vector3(
			spawnX ? spawnPos : currentCube.transform.position.x,
			currentCube.transform.position.y + currentCube.transform.localScale.y,
			spawnX ? currentCube.transform.position.z : spawnPos), Quaternion.identity);
		movingCube.transform.localScale = new Vector3(
			currentCube.transform.localScale.x,
			currentCube.transform.localScale.y,
			currentCube.transform.localScale.z);
		movingCube.isX = spawnX;
		movingCube.startPos = spawnPos;

		// colour
		cubeHue = (cubeHue + 3) % 100f;
		movingCube.SetColHSV(cubeHue / 100f, cubeSaturation, cubeValue);

		spawnX = !spawnX; // toggle direction
	}

	IEnumerator MoveCameraToHeight(float yPos)
	{
		Vector3 startPos = cam.transform.position;
		Vector3 endPos = cam.transform.position;
		endPos.y = yPos;

		float duration = 0.5f;
		float t = 0f;
		while (t <= 1f)
		{
			cam.transform.position = Vector3.Lerp(startPos, endPos, t);
			t += Time.deltaTime / duration;
			yield return null;
		}
	}

	IEnumerator MoveCameraToSize(float size)
	{
		float startPos = cam.orthographicSize;
		float endPos = size;

		float duration = 0.5f;
		float t = 0f;
		while (t <= 1f)
		{
			cam.orthographicSize = Mathf.Lerp(startPos, endPos, t);
			t += Time.deltaTime / duration;
			yield return null;
		}
	}
}

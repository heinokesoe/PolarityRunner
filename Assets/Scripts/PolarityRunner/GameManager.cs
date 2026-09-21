using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using BayatGames.SaveGameFree;
using BayatGames.SaveGameFree.Serializers;

using PolarityRunner.Characters;
using PolarityRunner.Collectables;
using PolarityRunner.TerrainGeneration;
using PolarityRunner.UI;
using PolarityRunner.Utilities;

namespace PolarityRunner
{
    public sealed class GameManager : MonoBehaviour
    {
        public delegate void AudioEnabledHandler(bool active);

        public delegate void ScoreHandler(float newScore, float highScore, float lastScore);

        public delegate void ResetHandler();

        public static event ResetHandler OnReset;
        public static event ScoreHandler OnScoreChanged;
        public static event AudioEnabledHandler OnAudioEnabled;

        public static event Action<int> OnStageChanged;
        public static event Action OnRunCompleted;

        private static GameManager m_Singleton;

        public static GameManager Singleton
        {
            get
            {
                return m_Singleton;
            }
        }

        [SerializeField]
        private Character m_MainCharacter;
        private float m_StartScoreX = 0f;
        private float m_HighScore = 0f;
        private float m_LastScore = 0f;
        private float m_Score = 0f;

        private bool m_GameStarted = false;
        private bool m_GameRunning = false;
        private bool m_AudioEnabled = true;

        [Header("Polarity Runner Stages")]
        [SerializeField]
        private float[] m_StageEndMeters = { 60f, 120f, 180f };
        private float[] m_StageEndWorldDistances;
        private int m_CurrentStage = 1;
        private bool m_RunCompleted = false;

        [Header("Revival")]
        [SerializeField]
        private int[] m_RevivalCosts = { 15, 30, 60 };
        [SerializeField]
        private float m_RevivalProtectionSeconds = 2f;
        private int m_RevivalsUsed;
        private bool m_AwaitingRevival;
        private Coroutine m_DeathCoroutine;
        private Vector3 m_InitialCharacterPosition;
        private Vector3 m_DeathPosition;

        /// <summary>
        /// This is my developed callbacks compoents, because callbacks are so dangerous to use we need something that automate the sub/unsub to functions
        /// with this in-house developed callbacks feature, we garantee that the callback will be removed when we don't need it.
        /// </summary>
        public Property<int> m_Coin = new Property<int>(0);


        #region Getters
        public bool gameStarted
        {
            get
            {
                return m_GameStarted;
            }
        }

        public bool gameRunning
        {
            get
            {
                return m_GameRunning;
            }
        }

        public bool audioEnabled
        {
            get
            {
                return m_AudioEnabled;
            }
        }

        public Character MainCharacter
        {
            get { return m_MainCharacter; }
        }

        public int CurrentStage
        {
            get { return m_CurrentStage; }
        }

        public bool runCompleted
        {
            get { return m_RunCompleted; }
        }

        public float CurrentStageProgress
        {
            get { return StageProgression.GetStageProgress(m_Score, m_CurrentStage, m_StageEndWorldDistances); }
        }

        public bool AwaitingRevival
        {
            get { return m_AwaitingRevival; }
        }

        public int RevivalCost
        {
            get
            {
                if (m_RevivalCosts == null || m_RevivalCosts.Length == 0)
                {
                    return 15;
                }
                return m_RevivalCosts[Mathf.Min(m_RevivalsUsed, m_RevivalCosts.Length - 1)];
            }
        }

        public bool CanRevive
        {
            get { return m_AwaitingRevival && m_Coin.Value >= RevivalCost; }
        }
        #endregion

        void Awake()
        {
            if (m_Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            SaveGame.Serializer = new SaveGameBinarySerializer();
            m_Singleton = this;
            m_Score = 0f;

            if (m_StageEndMeters == null || m_StageEndMeters.Length != StageProgression.StageCount)
            {
                m_StageEndMeters = new[] { 60f, 120f, 180f };
            }

            m_StageEndWorldDistances = new float[m_StageEndMeters.Length];
            for (int index = 0; index < m_StageEndMeters.Length; index++)
            {
                m_StageEndWorldDistances[index] = m_StageEndMeters[index].ToWorldUnits();
            }

            if (GetComponent<PolarityHUD>() == null)
            {
                gameObject.AddComponent<PolarityHUD>();
            }

            if (SaveGame.Exists("coin"))
            {
                m_Coin.Value = SaveGame.Load<int>("coin");
            }
            else
            {
                m_Coin.Value = 0;
            }
#if UNITY_EDITOR
            // Always begin editor play sessions with audible output. This prevents
            // an old saved mute preference from making the project appear broken.
            SetAudioEnabled(true);
#else
            if (SaveGame.Exists("audioEnabled"))
            {
                SetAudioEnabled(SaveGame.Load<bool>("audioEnabled"));
            }
            else
            {
                SetAudioEnabled(true);
            }
#endif
            if (SaveGame.Exists("lastScore"))
            {
                m_LastScore = SaveGame.Load<float>("lastScore");
            }
            else
            {
                m_LastScore = 0f;
            }
            if (SaveGame.Exists("highScore"))
            {
                m_HighScore = SaveGame.Load<float>("highScore");
            }
            else
            {
                m_HighScore = 0f;
            }

        }

        void UpdateDeathEvent(bool isDead)
        {
            if (isDead && !m_RunCompleted)
            {
                if (m_DeathCoroutine == null)
                {
                    m_DeathPosition = m_MainCharacter.transform.position;
                    m_DeathCoroutine = StartCoroutine(DeathCrt());
                }
            }
            else if (m_DeathCoroutine != null)
            {
                StopCoroutine(m_DeathCoroutine);
                m_DeathCoroutine = null;
            }
        }

        IEnumerator DeathCrt()
        {
            m_LastScore = m_Score;
            if (m_Score > m_HighScore)
            {
                m_HighScore = m_Score;
            }
            if (OnScoreChanged != null)
            {
                OnScoreChanged(m_Score, m_HighScore, m_LastScore);
            }

            yield return new WaitForSecondsRealtime(1.5f);

            EndGame();
            m_AwaitingRevival = true;
            m_DeathCoroutine = null;
        }

        private void Start()
        {
            m_MainCharacter.IsDead.AddEventAndFire(UpdateDeathEvent, this);
            m_StartScoreX = m_MainCharacter.transform.position.x;
            m_InitialCharacterPosition = m_MainCharacter.transform.position;
            if (TerrainGenerator.Singleton != null)
            {
                TerrainGenerator.Singleton.ConfigureLevelLength(m_StageEndWorldDistances[m_StageEndWorldDistances.Length - 1] + 20f);
            }
            Init();
        }

        public void Init()
        {
            EndGame();
            UIManager.Singleton.Init();
            StartCoroutine(Load());
        }

        void Update()
        {
            if (m_AwaitingRevival)
            {
                if (Input.GetKeyDown(KeyCode.C) && CanRevive)
                {
                    ReviveAtDeathPosition();
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartRunFromRevival();
                }
                return;
            }

            if (m_RunCompleted && Input.GetKeyDown(KeyCode.R))
            {
                RestartRun();
                return;
            }

            if (m_GameRunning)
            {
                float distance = Mathf.Max(0f, m_MainCharacter.transform.position.x - m_StartScoreX);
                if (distance > m_Score)
                {
                    m_Score = distance;
                    if (OnScoreChanged != null)
                    {
                        OnScoreChanged(m_Score, m_HighScore, m_LastScore);
                    }

                    UpdateStageProgress();
                }
            }
        }

        private void UpdateStageProgress()
        {
            float finalDistance = m_StageEndWorldDistances[m_StageEndWorldDistances.Length - 1];
            if (m_Score >= finalDistance)
            {
                CompleteRun();
                return;
            }

            int stage = StageProgression.GetStage(m_Score, m_StageEndWorldDistances);
            if (stage != m_CurrentStage)
            {
                m_CurrentStage = stage;
                if (OnStageChanged != null)
                {
                    OnStageChanged(m_CurrentStage);
                }
            }
        }

        private void CompleteRun()
        {
            if (m_RunCompleted)
            {
                return;
            }

            m_RunCompleted = true;
            m_CurrentStage = StageProgression.StageCount;
            m_LastScore = m_Score;
            m_HighScore = Mathf.Max(m_HighScore, m_Score);
            EndGame();

            if (OnRunCompleted != null)
            {
                OnRunCompleted();
            }
        }

        IEnumerator Load()
        {
            var startScreen = UIManager.Singleton.UISCREENS.Find(el => el.ScreenInfo == UIScreenInfo.START_SCREEN);
            yield return new WaitForSecondsRealtime(3f);
            UIManager.Singleton.OpenScreen(startScreen);
        }

        void OnApplicationQuit()
        {
            if (m_Score > m_HighScore)
            {
                m_HighScore = m_Score;
            }
            SaveGame.Save<int>("coin", m_Coin.Value);
            SaveGame.Save<bool>("audioEnabled", m_AudioEnabled);
            SaveGame.Save<float>("lastScore", m_Score);
            SaveGame.Save<float>("highScore", m_HighScore);
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void ToggleAudioEnabled()
        {
            SetAudioEnabled(!m_AudioEnabled);
        }

        public void SetAudioEnabled(bool active)
        {
            m_AudioEnabled = active;
            AudioListener.volume = active ? 1f : 0f;
            SaveGame.Save<bool>("audioEnabled", active);
            if (OnAudioEnabled != null)
            {
                OnAudioEnabled(active);
            }
        }

        public void StartGame()
        {
            m_GameStarted = true;
            ResumeGame();
        }

        public void StopGame()
        {
            m_GameRunning = false;
            Time.timeScale = 0f;
            if (AudioManager.Singleton != null)
            {
                AudioManager.Singleton.PauseMusic();
            }
        }

        public void ResumeGame()
        {
            m_GameRunning = true;
            Time.timeScale = 1f;
            if (AudioManager.Singleton != null)
            {
                AudioManager.Singleton.PlayMusic();
            }
        }

        public void EndGame()
        {
            m_GameStarted = false;
            StopGame();
            if (AudioManager.Singleton != null)
            {
                AudioManager.Singleton.StopMusic();
            }
        }

        public void RespawnMainCharacter()
        {
            RespawnCharacter(m_MainCharacter);
        }

        public void RespawnCharacter(Character character)
        {
            Block block = TerrainGenerator.Singleton.GetCharacterBlock();
            if (block != null)
            {
                Vector3 position = block.transform.position;
                position.y += 2.56f;
                position.x += 1.28f;
                character.transform.position = position;
                character.Reset();
            }
        }

        public void Reset()
        {
            m_Score = 0f;
            m_CurrentStage = 1;
            m_RunCompleted = false;
            m_AwaitingRevival = false;
            m_RevivalsUsed = 0;
            if (OnReset != null)
            {
                OnReset();
            }

            if (OnStageChanged != null)
            {
                OnStageChanged(m_CurrentStage);
            }
        }

        public void RestartRun()
        {
            Reset();
            var inGameScreen = UIManager.Singleton.GetUIScreen(UIScreenInfo.IN_GAME_SCREEN);
            if (inGameScreen != null)
            {
                UIManager.Singleton.OpenScreen(inGameScreen);
            }
            StartGame();
        }

        public void RestartRunFromRevival()
        {
            m_Coin.Value = 0;
            SaveGame.Save<int>("coin", m_Coin.Value);
            RestartRun();
        }

        public void ReviveAtDeathPosition()
        {
            if (!CanRevive)
            {
                return;
            }

            m_MainCharacter.Reset();
            Physics2D.SyncTransforms();
            Vector3 respawnPosition = FindSafeRevivalPosition(m_DeathPosition);

            int cost = RevivalCost;
            m_Coin.Value -= cost;
            SaveGame.Save<int>("coin", m_Coin.Value);
            m_RevivalsUsed++;
            m_AwaitingRevival = false;

            m_MainCharacter.Rigidbody2D.position = respawnPosition;
            m_MainCharacter.GrantInvulnerability(m_RevivalProtectionSeconds);
            Physics2D.SyncTransforms();
            if (CameraController.Singleton != null)
            {
                CameraController.Singleton.fastMove = true;
            }

            if (OnScoreChanged != null)
            {
                OnScoreChanged(m_Score, m_HighScore, m_LastScore);
            }

            var inGameScreen = UIManager.Singleton.GetUIScreen(UIScreenInfo.IN_GAME_SCREEN);
            if (inGameScreen != null)
            {
                UIManager.Singleton.OpenScreen(inGameScreen);
            }
            StartGame();
        }

        private Vector3 FindSafeRevivalPosition(Vector3 deathPosition)
        {
            float characterBottomOffset = m_MainCharacter.transform.position.y - m_MainCharacter.Collider2D.bounds.min.y;
            int groundMask = LayerMask.GetMask(GroundCheck.GROUND_LAYER_NAME);

            const float searchStep = 1.28f;
            const int maximumSearchSteps = 64;
            float rayStartY = Mathf.Max(100f, deathPosition.y + 50f);
            const float rayLength = 300f;
            for (int step = 0; step <= maximumSearchSteps; step++)
            {
                float offset = step * searchStep;
                float candidateX = deathPosition.x - offset;
                if (TryGetGroundedRevivalPosition(candidateX, rayStartY, rayLength, groundMask, characterBottomOffset, out Vector3 position))
                {
                    return position;
                }

                if (step > 0 && TryGetGroundedRevivalPosition(deathPosition.x + offset, rayStartY, rayLength, groundMask, characterBottomOffset, out position))
                {
                    return position;
                }
            }

            Debug.LogWarning("No safe ground was found near the death position. Using the beginning of the run instead.");
            return m_InitialCharacterPosition;
        }

        private bool TryGetGroundedRevivalPosition(float worldX, float rayStartY, float rayLength, int groundMask, float characterBottomOffset, out Vector3 position)
        {
            RaycastHit2D groundHit = Physics2D.Raycast(new Vector2(worldX, rayStartY), Vector2.down, rayLength, groundMask);
            if (groundHit.collider != null && groundHit.collider.CompareTag(GroundCheck.GROUND_TAG))
            {
                position = new Vector3(worldX, groundHit.point.y + characterBottomOffset + 0.08f, m_InitialCharacterPosition.z);
                return true;
            }

            position = Vector3.zero;
            return false;
        }

        [System.Serializable]
        public class LoadEvent : UnityEvent
        {

        }

    }

}

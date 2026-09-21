using PolarityRunner.Characters;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PolarityRunner.UI
{
    public sealed class PolarityHUD : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.025f, 0.035f, 0.07f, 0.86f);
        private static readonly Color MutedTextColor = new Color(0.72f, 0.76f, 0.84f, 1f);

        private Text m_StageText;
        private Text m_StageCountText;
        private Text m_PolarityText;
        private Text m_StageBannerText;
        private Image m_PolaritySwatch;
        private Image m_ProgressFill;
        private Image[] m_StageMarkers;
        private GameObject m_CompletionPanel;
        private GameObject m_RevivalPanel;
        private Text m_RevivalTitle;
        private Text m_RevivalCostText;
        private Button m_ContinueButton;
        private Button m_RestartButton;
        private int m_LastStage;
        private float m_BannerHideTime;

        private void Start()
        {
            BuildInterface();
            m_LastStage = GameManager.Singleton == null ? 1 : GameManager.Singleton.CurrentStage;
        }

        private void Update()
        {
            GameManager manager = GameManager.Singleton;
            if (manager == null || m_StageText == null)
            {
                return;
            }

            Character character = manager.MainCharacter;
            GameplayPolarity polarity = character == null ? GameplayPolarity.Red : character.Polarity;
            Color polarityColor = PolarityRules.GetColor(polarity);

            m_StageText.text = $"STAGE {manager.CurrentStage}";
            m_StageCountText.text = $"OF {StageProgression.StageCount}";
            m_PolarityText.text = polarity == GameplayPolarity.Red ? "RED" : "BLUE";
            m_PolarityText.color = polarityColor;
            m_PolaritySwatch.color = polarityColor;
            m_ProgressFill.color = polarityColor;
            m_ProgressFill.fillAmount = manager.CurrentStageProgress;

            for (int index = 0; index < m_StageMarkers.Length; index++)
            {
                if (index + 1 < manager.CurrentStage)
                {
                    m_StageMarkers[index].color = new Color(polarityColor.r, polarityColor.g, polarityColor.b, 0.55f);
                }
                else if (index + 1 == manager.CurrentStage)
                {
                    m_StageMarkers[index].color = polarityColor;
                }
                else
                {
                    m_StageMarkers[index].color = new Color(1f, 1f, 1f, 0.14f);
                }
            }

            if (manager.CurrentStage != m_LastStage)
            {
                m_LastStage = manager.CurrentStage;
                m_StageBannerText.text = $"STAGE {m_LastStage}";
                m_StageBannerText.gameObject.SetActive(true);
                m_BannerHideTime = Time.unscaledTime + 1.25f;
            }

            if (m_StageBannerText.gameObject.activeSelf && Time.unscaledTime >= m_BannerHideTime)
            {
                m_StageBannerText.gameObject.SetActive(false);
            }

            m_CompletionPanel.SetActive(manager.runCompleted);
            UpdateRevivalPanel(manager);
        }

        private void BuildInterface()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObject = new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
                eventSystemObject.transform.SetParent(transform, false);
            }

            GameObject canvasObject = CreateObject("Polarity Runner HUD", transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            BuildStageCard(canvasObject.transform, font);
            BuildPolarityCard(canvasObject.transform, font);
            BuildStageBanner(canvasObject.transform, font);
            BuildCompletionPanel(canvasObject.transform, font);
            BuildRevivalPanel(canvasObject.transform, font);
        }

        private void BuildStageCard(Transform parent, Font font)
        {
            GameObject panel = CreatePanel("Stage Card", parent, new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(310f, 96f));

            GameObject stageObject = CreateObject("Stage", panel.transform);
            SetRect(stageObject.GetComponent<RectTransform>(), new Vector2(18f, -12f), new Vector2(150f, 38f), new Vector2(0f, 1f));
            m_StageText = AddText(stageObject, font, 27, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);

            GameObject countObject = CreateObject("Stage Count", panel.transform);
            SetRect(countObject.GetComponent<RectTransform>(), new Vector2(168f, -17f), new Vector2(64f, 28f), new Vector2(0f, 1f));
            m_StageCountText = AddText(countObject, font, 16, TextAnchor.MiddleLeft, MutedTextColor, FontStyle.Bold);

            m_StageMarkers = new Image[StageProgression.StageCount];
            for (int index = 0; index < m_StageMarkers.Length; index++)
            {
                GameObject markerObject = CreateObject($"Stage {index + 1} Marker", panel.transform);
                SetRect(markerObject.GetComponent<RectTransform>(), new Vector2(240f + index * 18f, -24f), new Vector2(10f, 10f), new Vector2(0f, 1f));
                m_StageMarkers[index] = markerObject.AddComponent<Image>();
                m_StageMarkers[index].raycastTarget = false;
            }

            GameObject trackObject = CreateObject("Stage Progress Track", panel.transform);
            RectTransform trackRect = trackObject.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0f);
            trackRect.pivot = new Vector2(0.5f, 0f);
            trackRect.anchoredPosition = new Vector2(0f, 14f);
            trackRect.sizeDelta = new Vector2(-36f, 7f);
            Image track = trackObject.AddComponent<Image>();
            track.color = new Color(1f, 1f, 1f, 0.12f);
            track.raycastTarget = false;

            GameObject fillObject = CreateObject("Stage Progress", trackObject.transform);
            Stretch(fillObject.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
            m_ProgressFill = fillObject.AddComponent<Image>();
            m_ProgressFill.type = Image.Type.Filled;
            m_ProgressFill.fillMethod = Image.FillMethod.Horizontal;
            m_ProgressFill.raycastTarget = false;
        }

        private void BuildPolarityCard(Transform parent, Font font)
        {
            GameObject panel = CreatePanel("Polarity Card", parent, new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(300f, 96f));

            GameObject swatchObject = CreateObject("Polarity Swatch", panel.transform);
            SetRect(swatchObject.GetComponent<RectTransform>(), new Vector2(18f, -23f), new Vector2(48f, 48f), new Vector2(0f, 1f));
            m_PolaritySwatch = swatchObject.AddComponent<Image>();
            m_PolaritySwatch.raycastTarget = false;

            GameObject polarityObject = CreateObject("Polarity", panel.transform);
            SetRect(polarityObject.GetComponent<RectTransform>(), new Vector2(80f, -13f), new Vector2(108f, 34f), new Vector2(0f, 1f));
            m_PolarityText = AddText(polarityObject, font, 26, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);

            GameObject modeObject = CreateObject("Mode", panel.transform);
            SetRect(modeObject.GetComponent<RectTransform>(), new Vector2(80f, -47f), new Vector2(108f, 22f), new Vector2(0f, 1f));
            AddText(modeObject, font, 14, TextAnchor.MiddleLeft, MutedTextColor, FontStyle.Bold).text = "POLARITY";

            GameObject keyObject = CreateObject("Q Key", panel.transform);
            SetRect(keyObject.GetComponent<RectTransform>(), new Vector2(210f, -17f), new Vector2(50f, 38f), new Vector2(0f, 1f));
            Image keyBackground = keyObject.AddComponent<Image>();
            keyBackground.color = new Color(1f, 1f, 1f, 0.14f);
            keyBackground.raycastTarget = false;
            GameObject keyTextObject = CreateObject("Key Text", keyObject.transform);
            Stretch(keyTextObject.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
            AddText(keyTextObject, font, 20, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold).text = "Q";

            GameObject switchObject = CreateObject("Switch", panel.transform);
            SetRect(switchObject.GetComponent<RectTransform>(), new Vector2(198f, -59f), new Vector2(74f, 19f), new Vector2(0f, 1f));
            AddText(switchObject, font, 12, TextAnchor.MiddleCenter, MutedTextColor, FontStyle.Bold).text = "SWITCH";
        }

        private void BuildStageBanner(Transform parent, Font font)
        {
            GameObject bannerObject = CreateObject("Stage Banner", parent);
            RectTransform bannerRect = bannerObject.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 0.72f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.72f);
            bannerRect.sizeDelta = new Vector2(380f, 70f);
            m_StageBannerText = AddText(bannerObject, font, 40, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            bannerObject.SetActive(false);
        }

        private void BuildCompletionPanel(Transform parent, Font font)
        {
            m_CompletionPanel = CreatePanel("Run Complete", parent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 330f));
            Image completionBackground = m_CompletionPanel.GetComponent<Image>();
            completionBackground.color = new Color(0.015f, 0.02f, 0.04f, 0.96f);

            GameObject completionTextObject = CreateObject("Completion Text", m_CompletionPanel.transform);
            Stretch(completionTextObject.GetComponent<RectTransform>(), 35f, 35f, 35f, 35f);
            Text completionText = AddText(completionTextObject, font, 42, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            completionText.text = "POLARITY RUN COMPLETE!\n\nALL 3 STAGES CLEARED\n\nPRESS [R] TO RUN AGAIN";
            m_CompletionPanel.SetActive(false);
        }

        private void BuildRevivalPanel(Transform parent, Font font)
        {
            m_RevivalPanel = CreatePanel("Revival", parent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 360f));
            Image background = m_RevivalPanel.GetComponent<Image>();
            background.color = new Color(0.015f, 0.02f, 0.04f, 0.97f);
            background.raycastTarget = true;

            GameObject titleObject = CreateObject("Title", m_RevivalPanel.transform);
            SetRect(titleObject.GetComponent<RectTransform>(), new Vector2(0f, -42f), new Vector2(600f, 55f), new Vector2(0.5f, 1f));
            m_RevivalTitle = AddText(titleObject, font, 34, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);

            GameObject costObject = CreateObject("Cost", m_RevivalPanel.transform);
            SetRect(costObject.GetComponent<RectTransform>(), new Vector2(0f, -112f), new Vector2(600f, 50f), new Vector2(0.5f, 1f));
            m_RevivalCostText = AddText(costObject, font, 22, TextAnchor.MiddleCenter, MutedTextColor, FontStyle.Normal);

            m_ContinueButton = CreateButton("Continue", m_RevivalPanel.transform, font, new Vector2(-150f, 42f), "CONTINUE", new Color(0.12f, 0.55f, 0.95f, 1f));
            m_ContinueButton.onClick.AddListener(() => GameManager.Singleton.ReviveAtDeathPosition());

            m_RestartButton = CreateButton("Restart", m_RevivalPanel.transform, font, new Vector2(150f, 42f), "RESTART RUN", new Color(0.25f, 0.27f, 0.34f, 1f));
            m_RestartButton.onClick.AddListener(() => GameManager.Singleton.RestartRunFromRevival());
            m_RevivalPanel.SetActive(false);
        }

        private void UpdateRevivalPanel(GameManager manager)
        {
            bool visible = manager.AwaitingRevival;
            m_RevivalPanel.SetActive(visible);
            if (!visible)
            {
                return;
            }

            m_RevivalTitle.text = "REVIVE WHERE YOU DIED?";
            m_RevivalCostText.text = manager.CanRevive
                ? $"Continue from your death position for {manager.RevivalCost} coins  •  [C]"
                : "You don't have enough coins to revive";
            bool canRevive = manager.CanRevive;
            m_RevivalTitle.gameObject.SetActive(canRevive);
            m_RevivalCostText.gameObject.SetActive(true);
            m_ContinueButton.gameObject.SetActive(canRevive);

            RectTransform restartRect = m_RestartButton.GetComponent<RectTransform>();
            restartRect.anchoredPosition = new Vector2(canRevive ? 150f : 0f, restartRect.anchoredPosition.y);
        }

        private static Button CreateButton(string name, Transform parent, Font font, Vector2 position, string label, Color color)
        {
            GameObject buttonObject = CreateObject(name, parent);
            SetRect(buttonObject.GetComponent<RectTransform>(), position, new Vector2(240f, 66f), new Vector2(0.5f, 0f));
            Image image = buttonObject.AddComponent<Image>();
            image.color = color;
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            GameObject textObject = CreateObject("Label", buttonObject.transform);
            Stretch(textObject.GetComponent<RectTransform>(), 8f, 4f, 8f, 4f);
            AddText(textObject, font, 20, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold).text = label;
            return button;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            GameObject panel = CreateObject(name, parent);
            SetRect(panel.GetComponent<RectTransform>(), position, size, anchor);
            Image background = panel.AddComponent<Image>();
            background.color = PanelColor;
            background.raycastTarget = false;
            return panel;
        }

        private static GameObject CreateObject(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static Text AddText(GameObject gameObject, Font font, int size, TextAnchor alignment, Color color, FontStyle style)
        {
            Text text = gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.fontStyle = style;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(anchor.x, anchor.y);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}

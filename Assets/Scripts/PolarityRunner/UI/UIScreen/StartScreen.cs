using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PolarityRunner.UI
{
    public class StartScreen : UIScreen
    {
        [SerializeField]
        protected Button PlayButton = null;
        [SerializeField]
        protected Button HelpButton = null;
        [SerializeField]
        protected Button InfoButton = null;
        [SerializeField]
        protected Button ExitButton = null;

        private void Start()
        {
            AddPolarityHelpEntry();
            AddDeveloperInfo();

            PlayButton.SetButtonAction(() =>
            {
                var uiManager = UIManager.Singleton;
                var InGameScreen = uiManager.UISCREENS.Find(el => el.ScreenInfo == UIScreenInfo.IN_GAME_SCREEN);
                if (InGameScreen != null)
                {
                    uiManager.OpenScreen(InGameScreen);
                    GameManager.Singleton.StartGame();
                }
            });

            ExitButton.SetButtonAction(() =>
            {
                GameManager.Singleton.ExitGame();
            });
        }

        private void AddPolarityHelpEntry()
        {
            GameObject helpWindow = GameObject.Find("Help Window");
            if (helpWindow == null || helpWindow.transform.childCount == 0)
            {
                return;
            }

            RectTransform helpPanel = helpWindow.transform.GetChild(0) as RectTransform;
            if (helpPanel == null || helpPanel.Find("Polarity Help Key") != null)
            {
                return;
            }

            RectTransform keyTemplate = null;
            Text labelTemplate = null;
            foreach (RectTransform child in helpPanel)
            {
                Text childText = child.GetComponent<Text>();
                if (childText != null && childText.text == "Roll")
                {
                    labelTemplate = childText;
                }

                Text nestedText = child.GetComponentInChildren<Text>();
                if (nestedText != null && nestedText.text == "Shift")
                {
                    keyTemplate = child;
                }
            }

            if (keyTemplate == null || labelTemplate == null)
            {
                return;
            }

            helpPanel.sizeDelta = new Vector2(helpPanel.sizeDelta.x, 460f);

            // Re-space the existing controls into three even rows so the new
            // polarity row has the same breathing room as the others.
            foreach (RectTransform child in helpPanel)
            {
                if (child.name == "Close Button")
                {
                    continue;
                }

                Vector2 position = child.anchoredPosition;
                position.y = position.y > 60f ? 150f : position.y > -50f ? 50f : -50f;
                child.anchoredPosition = position;
            }

            RectTransform polarityKey = Instantiate(keyTemplate, helpPanel);
            polarityKey.name = "Polarity Help Key";
            polarityKey.anchoredPosition = new Vector2(-100.8f, -150f);
            polarityKey.GetComponentInChildren<Text>().text = "Q";

            Text polarityLabel = Instantiate(labelTemplate, helpPanel);
            polarityLabel.name = "Polarity Help Label";
            RectTransform labelRect = polarityLabel.rectTransform;
            labelRect.anchoredPosition = new Vector2(125f, -150f);
            labelRect.sizeDelta = new Vector2(180f, labelRect.sizeDelta.y);
            polarityLabel.text = "Change Color";
        }

        private void AddDeveloperInfo()
        {
            GameObject infoWindow = GameObject.Find("Info Window");
            GameObject helpWindow = GameObject.Find("Help Window");
            if (infoWindow == null || helpWindow == null || infoWindow.transform.childCount == 0)
            {
                return;
            }

            RectTransform infoPanel = infoWindow.transform.GetChild(0) as RectTransform;
            Text textTemplate = helpWindow.GetComponentInChildren<Text>(true);
            if (infoPanel == null || textTemplate == null || infoPanel.Find("Developer Names") != null)
            {
                return;
            }

            foreach (RectTransform child in infoPanel)
            {
                if (child.name != "Close Button")
                {
                    child.gameObject.SetActive(false);
                }
            }

            Text developerNames = Instantiate(textTemplate, infoPanel);
            developerNames.name = "Developer Names";
            developerNames.text = "Developed by Hein Oke Soe & Khine Khant";
            developerNames.fontSize = 30;
            developerNames.fontStyle = FontStyle.Bold;
            developerNames.alignment = TextAnchor.MiddleCenter;
            developerNames.color = Color.white;

            RectTransform namesRect = developerNames.rectTransform;
            namesRect.anchorMin = new Vector2(0.5f, 0.5f);
            namesRect.anchorMax = new Vector2(0.5f, 0.5f);
            namesRect.pivot = new Vector2(0.5f, 0.5f);
            namesRect.anchoredPosition = Vector2.zero;
            namesRect.sizeDelta = new Vector2(600f, 60f);
        }

        public override void UpdateScreenStatus(bool open)
        {
            base.UpdateScreenStatus(open);
        }
    }
}

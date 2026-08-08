using Assets.Scripts.System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Util;

namespace System
{
    /// <summary>
    /// <para>リザルトのフェーズを定義する</para>
    /// </summary>
    public class ResultPhase : GamePhase
    {
        private GameObject canvasObject;

        private readonly List<Action<GameObject>> itemBarDefinitions = new();

        private TextMeshProUGUI messageTexts;
        
        private GameObject resultContents;

        private GameObject controlGrid;
        private GameObject nextButton;
        private GameObject finishButton;
        private GameObject exitButton;

        private List<GameObject> itemBars;

        public ResultPhase(GameManager gameManager, GameManagerBehaviour gameManagerBehaviour) : base(gameManager, gameManagerBehaviour)
        {
            // 難易度を追加
            this.itemBarDefinitions.Add(gameObject =>
            {
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Name Display/Name"), textMeshProUGUI => textMeshProUGUI.text = "Difficulty");
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Value Display/Value"), textMeshProUGUI =>
                {
                    float difficulty = this.gameManager.Difficulty;

                    if (difficulty >= 0.5F)
                    {
                        textMeshProUGUI.text = "Hard";
                    }
                    else if (difficulty >= 0.1F)
                    {
                        textMeshProUGUI.text = "Normal";
                    }
                    else
                    {
                        textMeshProUGUI.text = "Easy";
                    }
                });
            });

            // ノルマを追加
            this.itemBarDefinitions.Add(gameObject =>
            {
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Name Display/Name"), textMeshProUGUI => textMeshProUGUI.text = "Quota");
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Value Display/Value"), textMeshProUGUI => textMeshProUGUI.text = $"{this.gameManager.Quata} $");
            });

            // ベットを追加
            this.itemBarDefinitions.Add(gameObject =>
            {
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Name Display/Name"), textMeshProUGUI => textMeshProUGUI.text = "Bet");
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Value Display/Value"), textMeshProUGUI => textMeshProUGUI.text = $"{this.gameManager.playerData.GetBet()} $");
            });

            // 所持金を追加
            this.itemBarDefinitions.Add(gameObject =>
            {
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Name Display/Name"), textMeshProUGUI => textMeshProUGUI.text = "Money");
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Value Display/Value"), textMeshProUGUI => textMeshProUGUI.text = $"{this.gameManager.playerData.GetValues()} $");
            });

            // スコアを追加
            this.itemBarDefinitions.Add(gameObject =>
            {
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Name Display/Name"), textMeshProUGUI => textMeshProUGUI.text = "Score");
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, "Value Display/Value"), textMeshProUGUI => textMeshProUGUI.text = $"{this.gameManager.playerData.GetScore()}");
            });
        }

        protected override void Init()
        {
            if (this.gameManagerBehaviour.ResultCanvas == null)
                return;

            this.canvasObject = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.ResultCanvas);

            if (this.canvasObject != null)
            {
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.canvasObject, "Message Display/Message"), textMeshProUGUI => this.messageTexts = textMeshProUGUI);

                this.resultContents = UIUtil.GetChild(this.canvasObject, "Result Display/Result Mask/Result Contents");
                this.controlGrid = UIUtil.GetChild(this.canvasObject, "Control Display/Control Grid");

                // コントロールボタンを定義する
                if (this.gameManagerBehaviour.ResultControlButton != null && this.controlGrid != null)
                {
                    this.nextButton = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.ResultControlButton);
                    this.finishButton = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.ResultControlButton);
                    this.exitButton = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.ResultControlButton);

                    if (this.nextButton != null)
                    {
                        this.nextButton.name = "Next";
                        this.nextButton.transform.SetParent(this.controlGrid.transform);
                        this.nextButton.transform.localScale = Vector3.one;

                        UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.nextButton, "Title"), textMeshProUGUI => textMeshProUGUI.text = "Next");

                        this.nextButton.SetActive(false);
                    }

                    if (this.finishButton != null)
                    {
                        this.finishButton.name = "Finish";
                        this.finishButton.transform.SetParent(this.controlGrid.transform);
                        this.finishButton.transform.localScale = Vector3.one;

                        UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.finishButton, "Title"), textMeshProUGUI => textMeshProUGUI.text = "Finish");

                        this.finishButton.SetActive(false);
                    }

                    if (this.exitButton != null)
                    {
                        this.exitButton.name = "Exit";
                        this.exitButton.transform.SetParent(this.controlGrid.transform);
                        this.exitButton.transform.localScale = Vector3.one;

                        UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.exitButton, "Title"), textMeshProUGUI => textMeshProUGUI.text = "Exit");

                        this.exitButton.SetActive(false);
                    }
                }
            }

            this.canvasObject.SetActive(false);
        }

        protected override void Start()
        {
            if (this.canvasObject == null)
                return;

            // 項目バーを生成する
            this.itemBars = new();

            if (this.resultContents != null)
            {
                if (this.itemBarDefinitions != null)
                {
                    GameObject itemBar;

                    foreach (Action<GameObject> action in this.itemBarDefinitions)
                    {
                        if (action == null)
                            continue;

                        // リザルト項目を生成する
                        itemBar = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.ResultItemBar);

                        if (itemBar != null)
                        {
                            action.Invoke(itemBar);

                            itemBar.transform.SetParent(this.resultContents.transform);
                            itemBar.transform.localScale = Vector3.one;

                            this.itemBars.Add(itemBar);
                        }
                    }
                }
            }

            // リザルト画面を生成する
            bool hasNext = false;
            bool hasFinish = false;
            bool hasExit = false;

            switch (this.gameManager.GameResult)
            {
                case Result.None:
                    if (this.messageTexts != null)
                        this.messageTexts.text = "No results...";

                    hasFinish = true;
                    break;
                case Result.Win:
                    if (this.messageTexts != null)
                        this.messageTexts.text = "You win!";

                    hasNext = hasFinish = true;
                    break;
                case Result.Draw:
                    if (this.messageTexts != null)
                        this.messageTexts.text = "It's a draw.";

                    hasNext = hasFinish = true;
                    break;
                case Result.Lose:
                    if (this.messageTexts != null)
                        this.messageTexts.text = "You lose...";

                    hasFinish = true;
                    break;
            }

            if (hasNext && this.nextButton != null)
                this.nextButton.SetActive(true);

            if (hasFinish && this.finishButton != null)
                this.finishButton.SetActive(true);

            if (hasExit && this.exitButton != null)
                this.exitButton.SetActive(true);

            this.canvasObject.SetActive(true);
        }

        protected override void Update()
        {
        }

        protected override void Finish()
        {
            if (this.canvasObject == null)
                return;

            UIUtil.DestoryAll(this.itemBars);

            if (this.nextButton != null)
                this.nextButton.SetActive(false);

            if (this.finishButton != null)
                this.finishButton.SetActive(false);

            if (this.exitButton != null)
                this.exitButton.SetActive(false);

            this.canvasObject.SetActive(false);
        }

        protected override void Destroy()
        {
            if (this.canvasObject == null)
                return;

            UnityEngine.Object.Destroy(this.canvasObject);
        }

        public override void Invoke(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            switch (gameObject.name)
            {
                case "Next":
                    this.gameManager.Call("bet");
                    this.gameManager.Play("Select");

                    break;
                case "Finish":
                    if (this.gameManager.Call("start"))
                    {
                        // プレイヤーの所持金をリセットする
                        this.gameManager.playerData.SetValues(50000);
                    }

                    this.gameManager.Play("Select");

                    break;
                case "Exit":
                    this.gameManager.Exit();
                    this.gameManager.Play("Invalid");

                    break;
            }
        }

        public enum Result
        {
            None,
            Win,
            Draw,
            Lose
        }
    }
}

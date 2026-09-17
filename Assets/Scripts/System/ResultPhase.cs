using Assets.Scripts.System;
using Player;
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
        public static readonly Color MAGENTA_COLOR = new(1.0F, 0.0F, 0.75F);
        public static readonly Color YELLOW_COLOR = new(1.0F, 0.75F, 0.0F);
        public static readonly Color CYAN_COLOR = new(0.0F, 0.75F, 1.0F);
        public static readonly Color WHITE_COLOR = new(1.0F, 1.0F, 1.0F);

        private GameObject canvasObject;

        private readonly List<Action<GameObject>> itemBarDefinitions = new();

        private TextMeshProUGUI messageTexts;
        
        private GameObject resultContents;

        private GameObject controlGrid;
        private GameObject nextButton;
        private GameObject finishButton;
        private GameObject exitButton;

        private List<GameObject> itemBars;

        private bool isPerfect = false;
        private bool perfectWin = false;

        public ResultPhase(GameManager gameManager, GameManagerBehaviour gameManagerBehaviour) : base(gameManager, gameManagerBehaviour)
        {
            // 難易度を追加
            this.AddItemBar(resultPhase => GameTexts.Get("result.difficulty"), resultPhase =>
            {
                float difficulty = resultPhase.gameManager.Difficulty;

                if (difficulty >= 0.5F)
                {
                    return GameTexts.Get("difficulty.hard");
                }
                else if (difficulty >= 0.1F)
                {
                    return GameTexts.Get("difficulty.normal");
                }
                else
                {
                    return GameTexts.Get("difficulty.easy");
                }
            },
                resultPhase => Color.white,
                resultPhase =>
                {
                    float difficulty = resultPhase.gameManager.Difficulty;

                    if (difficulty >= 0.5F)
                    {
                        return MAGENTA_COLOR;
                    }
                    else if (difficulty >= 0.1F)
                    {
                        return YELLOW_COLOR;
                    }
                    else
                    {
                        return CYAN_COLOR;
                    }
                }
            );

            // スコアを追加
            this.AddItemBar(resultPhase => GameTexts.Get("result.score"), resultPhase => $"{resultPhase.gameManager.playerData.GetScore()}",
                resultPhase => Color.white,
                resultPhase => resultPhase.gameManager.playerData.GetScore() == 21 ? MAGENTA_COLOR : WHITE_COLOR
            );

            // ノルマを追加
            this.AddItemBar(resultPhase => GameTexts.Get("result.quota"), resultPhase => $"{resultPhase.gameManager.Quata} $",
                resultPhase => Color.white,
                resultPhase => Color.white
            );

            // ベットを追加
            this.AddItemBar(resultPhase => GameTexts.Get("result.bet"), resultPhase => $"{resultPhase.gameManager.playerData.GetBet()}",
                resultPhase => Color.white,
                resultPhase => Color.white
            );

            // 倍率を追加
            this.AddItemBar(resultPhase => GameTexts.Get("result.ratio"), resultPhase => $"{resultPhase.gameManager.playerData.PayoutMultiplier.Calculate():0.00} $",
                resultPhase => Color.white,
                resultPhase => Color.white
            );

            // 所持金を追加
            this.AddItemBar(resultPhase => GameTexts.Get("result.money"), resultPhase =>
            {
                int oldVal = resultPhase.gameManager.playerData.GetOldValues();
                int newVal = resultPhase.gameManager.playerData.GetValues();
                int betVal = resultPhase.gameManager.playerData.GetBet();

                int dltVal = newVal - (oldVal + betVal);

                string sign = dltVal >= 0 ? "+" : "-";

                // return dltVal == 0 ? dltVal.ToString() : $"{newVal} $ ({sign} {dltVal} $)";
                return $"{newVal} $ ({sign} {Mathf.Abs(dltVal)} $)";
            },
                resultPhase => Color.white,
                resultPhase =>
                {
                    int oldVal = resultPhase.gameManager.playerData.GetOldValues();
                    int newVal = resultPhase.gameManager.playerData.GetValues();
                    int betVal = resultPhase.gameManager.playerData.GetBet();

                    int dltVal = newVal - (oldVal + betVal);

                    if (dltVal == 0)
                    {
                        return YELLOW_COLOR;
                    }
                    else if (dltVal > 0)
                    {
                        return MAGENTA_COLOR;
                    }
                    else
                    {
                        return CYAN_COLOR;
                    }
                }
            );
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

                        UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.nextButton, "Title"), textMeshProUGUI => textMeshProUGUI.text = GameTexts.Get("result.next"));

                        this.nextButton.SetActive(false);
                    }

                    if (this.finishButton != null)
                    {
                        this.finishButton.name = "Finish";
                        this.finishButton.transform.SetParent(this.controlGrid.transform);
                        this.finishButton.transform.localScale = Vector3.one;

                        UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.finishButton, "Title"), textMeshProUGUI => textMeshProUGUI.text = GameTexts.Get("result.finish"));

                        this.finishButton.SetActive(false);
                    }

                    if (this.exitButton != null)
                    {
                        this.exitButton.name = "Exit";
                        this.exitButton.transform.SetParent(this.controlGrid.transform);
                        this.exitButton.transform.localScale = Vector3.one;

                        UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.exitButton, "Title"), textMeshProUGUI => textMeshProUGUI.text = GameTexts.Get("result.next"));

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

            // ゲーム回数が5回より大きい、かつ所持金額がノルマ以上であると、完全勝利する
            //      0オリジンなため、4回以上かを判定
            if (this.gameManager.playerData.GetValues() >= this.gameManager.Quata && this.gameManager.GameCount >= 4)
            {
                isPerfect = true;
                perfectWin = true;
            }
            

            // ノルマ金額より所持金額が下回ったら完全敗北
            if(this.gameManager.playerData.GetValues() < this.gameManager.Quata)
            {
                isPerfect = true;
                perfectWin = false;
            }

            // リザルトに応じた画面の切り替え
            switch (this.gameManager.GameResult)
            {
                case Result.None:
                    if (this.messageTexts != null)
                        this.messageTexts.text = GameTexts.Get("result.none");
                    hasFinish = true;
                    this.gameManager.GameCount = 0;
                    break;
                case Result.Win:
                    if (this.messageTexts != null)
                        this.messageTexts.text = GameTexts.Get("result.win");

                    if (!isPerfect)
                    {
                        hasNext = hasFinish = true;
                        this.gameManager.GameCount += 1;
                    }
                    else
                    {
                        hasExit = true;
                        this.gameManager.GameCount = 0;
                        if (perfectWin)
                            this.gameManager.GameResult = Result.PerfectWin;
                    }
                    break;
                case Result.Draw:
                    if (this.messageTexts != null)
                        this.messageTexts.text = GameTexts.Get("result.draw");

                    if (!isPerfect)
                    {
                        hasNext = hasFinish = true;
                    }
                    else
                    {
                        hasExit = true;
                        this.gameManager.GameCount = 0;
                        if (perfectWin)
                            this.gameManager.GameResult = Result.PerfectWin;
                    }
                    break;
                case Result.Lose:
                    if (this.messageTexts != null)
                        this.messageTexts.text = GameTexts.Get("result.lose");

                    if (!isPerfect)
                    {
                        hasNext = hasFinish = true;
                        this.gameManager.GameCount += 1;
                        if (perfectWin)
                            this.gameManager.GameResult = Result.PerfectWin;
                    }
                    else
                    {
                        hasExit = true;
                        this.gameManager.GameCount = 0;
                        if (perfectWin)
                            this.gameManager.GameResult = Result.PerfectWin;
                    }
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

            isPerfect = false;
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

                        // プレイヤーのアイテムをクリアする
                        this.gameManager.ClearPlayerItemData();

                        this.gameManager.GameCount = 0;
                    }

                    this.gameManager.Play("Select");

                    break;
                case "Exit":
                    this.gameManager.Call("perfect");
                    this.gameManager.Play("Invalid");

                    break;

            }
        }

        public void AddItemBar(
            Func<ResultPhase, string> itemNameIdProvider,
            Func<ResultPhase, string> displayValueProvider,
            string nameDisplayPath = "Name Display/Name",
            string valueDisplayPath = "Value Display/Value"
        )
        {
            this.AddItemBar(
                itemNameIdProvider,
                displayValueProvider,
                resultPhase => Color.white,
                resultPhase => Color.white,
                nameDisplayPath,
                valueDisplayPath
            );
        }

        private void AddItemBar(
            Func<ResultPhase, string> itemNameIdProvider,
            Func<ResultPhase, string> displayValueProvider,
            Func<ResultPhase, Color> itemNameColorProvider,
            Func<ResultPhase, Color> displayValueColorProvider,
            string nameDisplayPath = "Name Display/Name",
            string valueDisplayPath = "Value Display/Value"
        )
        {
            this.itemBarDefinitions.Add(gameObject =>
            {
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, nameDisplayPath), textMeshProUGUI =>
                {
                    textMeshProUGUI.text = itemNameIdProvider.Invoke(this);
                    textMeshProUGUI.color = itemNameColorProvider.Invoke(this);
                });
                UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(gameObject, valueDisplayPath), textMeshProUGUI =>
                {
                    textMeshProUGUI.text = displayValueProvider.Invoke(this);
                    textMeshProUGUI.color = displayValueColorProvider.Invoke(this);
                });
            });
        }

        public enum Result
        {
            None,
            Win,
            Draw,
            Lose,
            PerfectWin
        }
    }
}

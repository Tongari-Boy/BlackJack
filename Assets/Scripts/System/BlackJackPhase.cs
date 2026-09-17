using Assets.Scripts.System;
using Cards;
using Item;
using Player;
using UnityEngine;
using Util;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

namespace System
{
    /// <summary>
    /// <para>ブラックジャックのフェーズを定義する</para>
    /// </summary>
    public class BlackjackPhase : GamePhase
    {
        /// <summary>
        /// ブラックジャック内での細かいフェーズ分け
        /// 
        /// プレイヤの入力無視などのために用いる
        /// </summary>
        private enum SubPhase
        {
            Dealing,    // 最初の2枚配り
            PlyaerTurn, // プレイヤターン
            StandConfirm,   // Standの確認待ち
            DealerTurn, // ディーラーターン
            Judge,      // バーストしていないか、互いにスライドしたかなどの判定
            Adaptation, // 払い戻しの適応(勝ったときのみ移動する)
            Result,     // 結果の話
        }

        private SubPhase currentSubPhase;

        /// <summary>
        /// アニメーション中などに入力を一時的に停止させるフラグ
        /// </summary>
        private bool isInputLocked = true;

        private Deck deck;
        private PlayerCards playerCards;
        private DealerCards dealerCards;
        private PlayerScoreView playerScoreView;
        private DealerScoreView dealerScoreView;
        private PlayerValueView playerValueView;

        private PlayerData playerData;
        private DealerData dealerData;

        private GameObject blackJackOnlyUIs;

        private GameObject resultOnlyUI;
        private GameObject winUI;
        private GameObject loseUI;
        private bool isWin = false;
        private bool isPlayerBurstWaiting = false;
        private float mutiplier = 1.0f; // 倍率
        private int payout = 0;         // 払い戻し

        /// <summary>
        /// ディーラーのカードめくり処理が進行中か
        /// 
        /// Update内でコルーチンを重複しての呼び出しを防ぐために用いる
        /// </summary>
        private bool isDealerCardsOpening = false;

        /// <summary>
        /// 確認ボタンのインスタンス
        /// </summary>
        private GameObject confirmPopupObject;

        private Action pendingComfirmAction;

        public BlackjackPhase(GameManager gameManager, GameManagerBehaviour gameManagerBehaviour) : base(gameManager, gameManagerBehaviour) { }

        /// <summary>
        /// 参照の取得/初期セットアップ
        /// </summary>
        protected override void Init()
        {
            deck = gameManagerBehaviour.Deck;
            gameManager.ResisterDeck(deck);

            playerCards = gameManagerBehaviour.PlayerCards;
            dealerCards = gameManagerBehaviour.DealerCards;
            playerScoreView = gameManagerBehaviour.PlayerScoreView;
            dealerScoreView = gameManagerBehaviour.DealerScoreView;
            playerValueView = gameManagerBehaviour.PlayerValueView;
            blackJackOnlyUIs = gameManagerBehaviour.BlackJackOnlyUIs;

            resultOnlyUI = gameManagerBehaviour.ResultOnlyUI;
            winUI = gameManagerBehaviour.WinUI;
            loseUI = gameManagerBehaviour.LoseUI;

            playerData = gameManager.playerData;
            dealerData = gameManager.dealerData;
            playerCards.Setup(playerData, deck);
            dealerCards.Setup(dealerData, deck);

            playerScoreView.Setup(playerData);
            dealerScoreView.Setup(dealerData);
            playerValueView.Setup(playerData);

            SetupComfirmPopup();
        }

        /// <summary>
        /// スタート
        /// </summary>
        protected override void Start()
        {
            playerValueView.SetQuota(this.gameManager.Quata);
            playerValueView.SetBet(playerData.GetBet());

            blackJackOnlyUIs.SetActive(false);
            resultOnlyUI.SetActive(false);
            winUI.SetActive(false);
            loseUI.SetActive(false);

            isWin = false;
            isPlayerBurstWaiting = false;

            playerData.SetScore(0);
            dealerData.SetScore(0);

            currentSubPhase = SubPhase.Dealing;

            blackJackOnlyUIs.SetActive(true);

            playerData.SetCard(new System.Collections.Generic.List<CardsManager.Card>());
            dealerData.SetCard(new System.Collections.Generic.List<CardsManager.Card>());

            playerData.SetIsPlaying(true);
            dealerData.SetIsPlaying(true);

            /* ItemSlotSetup(); */

            deck.InitializeDeck();
            deck.Shuffle();

            playerCards.DrawCard(2);
            dealerCards.DrawInitialCards();

            currentSubPhase = SubPhase.PlyaerTurn;
            isInputLocked = false;
        }

        /// <summary>
        /// 更新
        /// </summary>
        protected override void Update()
        {
            switch(currentSubPhase)
            {
                case SubPhase.PlyaerTurn:
                    if(!playerData.GetIsPlaying())
                    {
                        isInputLocked = true;
                        if(playerData.GetScore() > 21)
                        {
                            if(!isPlayerBurstWaiting)
                            {
                                isPlayerBurstWaiting = true;
                                gameManagerBehaviour.StartCoroutine(PlayerBurstRoutine());
                            }
                            break;
                        }
                        currentSubPhase = SubPhase.DealerTurn;
                    }

                    break;

                case SubPhase.DealerTurn:
                    //ディーラーの処理が未開始ならコルーチンをスタートさせる
                    if(!isDealerCardsOpening)
                    {
                        isDealerCardsOpening = true;
                        gameManagerBehaviour.StartCoroutine(DealerTurnRoutine());
                    }
                     break;

                case SubPhase.Judge:
                    JudgeResult();

                    if (this.gameManager.GameResult == ResultPhase.Result.Draw)
                    {
                        blackJackOnlyUIs.SetActive(false);
                        playerCards.ClearCards();
                        dealerCards.ClearCards();

                        playerData.SetOldValues();
                        playerData.AddValues(playerData.GetBet());

                        GameManager.INSTANCE.Call("result");

                        break;
                    }


                    if (isWin)
                    {
                        currentSubPhase = SubPhase.Adaptation;
                    }
                    else
                    {
                        blackJackOnlyUIs.SetActive(false);
                        playerCards.ClearCards();
                        dealerCards.ClearCards();
                        // SetResult(isWin);
                        playerData.SetOldValues();
                        GameManager.INSTANCE.Call("result");
                        // currentSubPhase = SubPhase.Result;
                    }
                    
                    break;

                    // 勝った場合、ここに飛んで計算処理を行う
                case SubPhase.Adaptation:
                    int bet = playerData.GetBet();

                    mutiplier = CalcultePayoutMultiplier();
                    Debug.Log(mutiplier);
                    payout = bet + Mathf.RoundToInt(bet * mutiplier);
                    Debug.Log(payout);

                    SetPayout();
                    Debug.Log(playerData.GetValues());


                    blackJackOnlyUIs.SetActive(false);
                    playerCards.ClearCards();
                    dealerCards.ClearCards();

                    // SetResult(isWin);
                    GameManager.INSTANCE.Call("result");
                    // currentSubPhase = SubPhase.Result;

                    break;

                case SubPhase.Result:
                    //結果表示処理

                    blackJackOnlyUIs.SetActive(false);
                    playerCards.ClearCards();
                    dealerCards.ClearCards();

                    ShowResultUI();
                    /*
                    ShowConfirmPopup("Result", () =>
                    {

                        GameManager.INSTANCE.Call("result");
                    });*/
                    break;
            }
        }

        public IEnumerator PlayerBurstRoutine()
        {
            yield return new WaitForSeconds(2.0f);

            currentSubPhase = SubPhase.Judge;
        }


        //============================
        // Dealing
        //============================

        private void ItemSlotSetup()
        {
            // アイテムボタンA
            GameObject slotA = UIUtil.GetChild(this.blackJackOnlyUIs, "ItemBtns/A");

            Image imgA = slotA != null ? slotA.GetComponent<Image>() : null;

            if (imgA != null)
            {
                ItemData itemA = this.gameManager.GetPlayerItemData(0);

                ItemImageHolder holderA = this.gameManager.GetItemImageHolder(itemA.Name, itemA.Rarity);

                imgA.sprite = holderA.ItemImage?.sprite;
            }

            // アイテムボタンB
            GameObject slotB = UIUtil.GetChild(this.blackJackOnlyUIs, "ItemBtns/B");

            Image imgB = slotB != null ? slotB.GetComponent<Image>() : null;

            if (imgB != null)
            {
                ItemData itemB = this.gameManager.GetPlayerItemData(1);

                ItemImageHolder holderB = this.gameManager.GetItemImageHolder(itemB.Name, itemB.Rarity);

                imgB.sprite = holderB.ItemImage?.sprite;
            }

            // アイテムボタンC
            GameObject slotC = UIUtil.GetChild(this.blackJackOnlyUIs, "ItemBtns/C");

            Image imgC = slotC != null ? slotC.GetComponent<Image>() : null;

            if (imgC != null)
            {
                ItemData itemC = this.gameManager.GetPlayerItemData(2);

                ItemImageHolder holderC = this.gameManager.GetItemImageHolder(itemC.Name, itemC.Rarity);

                imgC.sprite = holderC.ItemImage?.sprite;
            }

            // アイテムボタンD
            GameObject slotD = UIUtil.GetChild(this.blackJackOnlyUIs, "ItemBtns/D");

            Image imgD = slotD != null ? slotD.GetComponent<Image>() : null;

            if (imgD != null)
            {
                ItemData itemD = this.gameManager.GetPlayerItemData(3);

                ItemImageHolder holderD = this.gameManager.GetItemImageHolder(itemD.Name, itemD.Rarity);

                imgD.sprite = holderD.ItemImage?.sprite;
            }

            // アイテムボタンE
            GameObject slotE = UIUtil.GetChild(this.blackJackOnlyUIs, "ItemBtns/E");

            Image imgE = slotE != null ? slotE.GetComponent<Image>() : null;

            if (imgE != null)
            {
                ItemData itemE = this.gameManager.GetPlayerItemData(4);

                ItemImageHolder holderE = this.gameManager.GetItemImageHolder(itemE.Name, itemE.Rarity);

                imgE.sprite = holderE.ItemImage?.sprite;
            }

            // アイテムボタンF
            GameObject slotF = UIUtil.GetChild(this.blackJackOnlyUIs, "ItemBtns/F");

            Image imgF = slotF != null ? slotF.GetComponent<Image>() : null;

            if (imgF != null)
            {
                ItemData itemF = this.gameManager.GetPlayerItemData(5);

                ItemImageHolder holderF = this.gameManager.GetItemImageHolder(itemF.Name, itemF.Rarity);

                imgF.sprite = holderF.ItemImage?.sprite;
            }
        }


        //============================
        // PlayerTurn
        //============================

        /// <summary>
        /// ヒットできるか
        /// </summary>
        public void TryHit()
        {
            if (!CanPlayerAct())
            {
                return;
            }

            isInputLocked = true;
            playerCards.Hit();
            isInputLocked = false;

            this.gameManager.Play("Select");
        }

        /// <summary>
        /// スタンドできるか
        /// </summary>
        public void TryStand()
        {
            if (!CanPlayerAct())
            {
                return;
            }

            isInputLocked = true;

            ShowConfirmPopup(
                "ターンを終わりますか？",
                () =>
                {
                    playerCards.Stand();
                },
                () =>
                {
                    HideConfirmPopup();
                    isInputLocked = false;
                }
            );
        }

        /// <summary>
        /// アイテムボタン押せるか
        /// </summary>
        public void TryItemButtom(int index)
        {
            if (!CanPlayerAct() || GameManager.INSTANCE.GetPlayerItemData(index).Equals(ItemData.EMPTY))
            {
                return;
            }
            isInputLocked = true;

            GameManager.INSTANCE.GetPlayerItemData(index);
            GameManager.INSTANCE.UsePlayerItemData(index);

            isInputLocked = false;

            this.gameManager.Play("Select");
        }

        /// <summary>
        /// プレイヤの操作の判定
        /// </summary>
        /// <returns></returns>
        private bool CanPlayerAct()
        {
            return !isInputLocked
                && currentSubPhase == SubPhase.PlyaerTurn
                && playerData.GetIsPlaying();
        }

        /// <summary>
        /// アイテムボタンが押された時の処理
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="contexts"></param>
        public override void Invoke(GameObject gameObject, params object[] contexts)
        {
            if (gameObject == null)
                return;

            if (contexts != null && contexts.Length >= 1 && contexts[0] is int index)
            {
                this.TryItemButtom(index);
                ItemSlotSetup();
            }
        }


        //============================
        // DealerTurn
        //============================

        /// <summary>
        /// ディーラーの思考/カードめくりを行うコルーチン
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator DealerTurnRoutine()
        {
            dealerCards.CalcScore();

            while (dealerData.GetScore() < 17)
            {
                dealerCards.Hit();
            }

            dealerData.SetIsPlaying(false);

            // ディーラーのカードを一定時間ごとにめくる
            yield return gameManagerBehaviour.StartCoroutine(dealerCards.CardsOpen(0.7f));

            yield return new WaitForSeconds(0.5f);

            dealerScoreView.SetActiveText(true);

            yield return new WaitForSeconds(2f);

            currentSubPhase = SubPhase.Judge;
            isDealerCardsOpening = false;
        }


        //============================
        // Judge
        //============================
        private void JudgeResult()
        {
            bool playerBurst = ScoreCalclator.IsBurst(playerData.GetCard());
            bool dealerBurst = ScoreCalclator.IsBurst(dealerData.GetCard());
            int playerScore = playerData.GetScore();
            int dealerScore = dealerData.GetScore();

            if (playerBurst)
            {
                // プレイヤ負け処理
                Debug.Log("プレイヤの負け");
                isWin = false;
                this.gameManager.GameResult = ResultPhase.Result.Lose;
            }
            else if (dealerBurst || playerScore > dealerScore)
            {
                // プレイヤ勝ち
                Debug.Log("プレイヤの勝ち");
                isWin = true;
                this.gameManager.GameResult = ResultPhase.Result.Win;
            }
            else if (playerScore < dealerScore)
            {
                // プレイヤ負け
                Debug.Log("プレイヤの負け");
                isWin = false;
                this.gameManager.GameResult = ResultPhase.Result.Lose;
            }
            else
            {
                // 引き分け
                Debug.Log("ひきわけ");
                this.gameManager.GameResult = ResultPhase.Result.Draw;
            }
        }

        /// <summary>
        /// ブラックジャックかどうか判定
        /// </summary>
        private bool IsBlackjack()
        {
            return playerData.GetCard().Count == 2 && playerData.GetScore() == 21;
        }


        //============================
        // Result
        //============================

        /// <summary>
        /// 結果UIの表示
        /// </summary>
        public void ShowResultUI()
        {
            if (isWin)
            {
                winUI.SetActive(true);
                SetResult(isWin);
            }
            else
            {
                loseUI.SetActive(true);
                SetResult(isWin);
            }

            resultOnlyUI.SetActive(true);
        }

        private void SetResult(bool isWin)
        {
            if(isWin)
            {
                this.gameManager.GameResult = ResultPhase.Result.Win;
            }
            else
            {
                this.gameManager.GameResult = ResultPhase.Result.Lose;
            }
        }

        /// <summary>
        /// 倍率計算
        /// </summary>
        /// <returns></returns>
        private float CalcultePayoutMultiplier()
        {
            const string BlackjackBonus = "blackjack";

            if(IsBlackjack())
            {
                playerData.PayoutMultiplier.SetBonus(BlackjackBonus, 0.5f);
            }
            else
            {
                playerData.PayoutMultiplier.RemoveBonus(BlackjackBonus);
            }

            return playerData.PayoutMultiplier.Calculate();
        }

        
        /// <summary>
        /// プレイヤデータに払い戻し額を適応
        /// </summary>
        private void SetPayout()
        {
            playerData.SetOldValues();
            playerData.AddValues(payout);
        }

        //====================
        // その他汎用処理
        //====================
        private void SetupComfirmPopup()
        {
            if (gameManagerBehaviour.ConfirmPopupPrefab == null)
                return;

            confirmPopupObject = UnityEngine.Object.Instantiate(gameManagerBehaviour.ConfirmPopupPrefab);

            /*
            UIUtil.InvokeIfPresent<Button>(UIUtil.GetChild(confirmPopupObject, "ConfirmButton"), button =>
            {
                button.onClick.AddListener(OnConfirmButtonClicked);
            });
            */

            confirmPopupObject.SetActive(false);
        }
        

        /// <summary>
        /// 確認ポップアップを表示する
        /// </summary>
        /// <param name="message">表示メッセージ</param>
        /// <param name="onConfirmed">確認ボタンが押されたときに実行する処理</param>
        private void ShowConfirmPopup(string message,Action onYesConfirmed,Action onNoConfirmed)
        {
            if (confirmPopupObject == null)
                return;
           
            Button yesButton = confirmPopupObject.transform.Find("Yes").GetComponent<Button>();
            Button noButton = confirmPopupObject.transform.Find("No").GetComponent<Button>();

            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() =>
            {
                confirmPopupObject.SetActive(false);
                onYesConfirmed?.Invoke();
            });

            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() =>
            {
                confirmPopupObject.SetActive(false);
                onNoConfirmed?.Invoke();
            });

            confirmPopupObject.SetActive(true);
        }

        private void HideConfirmPopup()
        {
            confirmPopupObject.SetActive(false);
        }

        //====================
        // 終了処理
        //====================

        protected override void Finish()
        {
            // playerData.ResetBet();

            resultOnlyUI.SetActive(false);
            winUI.SetActive(false);
            loseUI.SetActive(false);
            isWin = false;
            currentSubPhase = SubPhase.Dealing;

            Debug.Log("リザルトフェーズへ移行");
        }

        protected override void Destroy()
        {
            deck = null;
            playerCards = null;
            dealerCards = null;
            playerScoreView = null;
            playerData = null;
            dealerData = null;

            if (confirmPopupObject != null)
            {
                UnityEngine.Object.Destroy(confirmPopupObject);
            }
        }
    }
}
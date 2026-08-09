using Assets.Scripts.System;
using Bet;
using Cards;
using Item;
using Player;
using UnityEngine;
using Util;
using UnityEngine.UI;

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
            DealerTurn, // ディーラーターン
            Judge,      // バーストしていないか、互いにスライドしたかなどの判定
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
        // private PayoutMultiplierView payoutMultiplierView;
        private PlayerValueView playerValueView;

        private PlayerData playerData;
        private DealerData dealerData;

        private GameObject blackJackOnlyUIs;

        //private GameObject betOnlyUIs;
        //private BetButtoms betButtoms;

        private GameObject resultOnlyUI;
        private GameObject winUI;
        private GameObject loseUI;
        private bool isWin = false;

        /// <summary>
        /// ディーラーのカードめくり処理が進行中か
        /// 
        /// Update内でコルーチンを重複しての呼び出しを防ぐために用いる
        /// </summary>
        private bool isDealerCardsOpening = false;


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
            //payoutMultiplierView = gameManagerBehaviour.PayoutMultiplierView;
            //if (payoutMultiplierView != null)
            //{
            //    payoutMultiplierView.Setup(playerData);
            //}
            blackJackOnlyUIs = gameManagerBehaviour.BlackJackOnlyUIs;

            //betOnlyUIs = gameManagerBehaviour.BetOnlyUIs;
            //betButtoms = gameManagerBehaviour.BetButtoms;

            resultOnlyUI = gameManagerBehaviour.ResultOnlyUI;
            winUI = gameManagerBehaviour.WinUI;
            loseUI = gameManagerBehaviour.LoseUI;

            playerData = gameManager.playerData;
            dealerData = gameManager.dealerData;
            dealerData = gameManager.dealerData;
            playerCards.Setup(playerData, deck);
            dealerCards.Setup(dealerData, deck);
            playerScoreView.Setup(playerData);
            dealerScoreView.Setup(dealerData);
            playerValueView.Setup(playerData);
            dealerScoreView.SetActiveText(false);
            blackJackOnlyUIs.SetActive(false);
            //betOnlyUIs.SetActive(false);

            //betButtoms.OnBetConfirmed += OnBetConfirmed;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Start()
        {
            blackJackOnlyUIs.SetActive(false);
            resultOnlyUI.SetActive(false);
            winUI.SetActive(false);
            loseUI.SetActive(false);
            isWin = false;

            playerData.SetScore(0);
            dealerData.SetScore(0);

            //betOnlyUIs.SetActive(true);
            //betButtoms.ResetInput();

            currentSubPhase = SubPhase.Dealing;

            blackJackOnlyUIs.SetActive(true);

            playerData.SetCard(new System.Collections.Generic.List<CardsManager.Card>());
            dealerData.SetCard(new System.Collections.Generic.List<CardsManager.Card>());

            playerData.SetIsPlaying(true);
            dealerData.SetIsPlaying(true);

            ItemSlotSetup();

            playerCards.DrawCard(2);
            dealerCards.DrawInitialCards();

            currentSubPhase = SubPhase.PlyaerTurn;
            isInputLocked = false;
        }

        //private void OnBetConfirmed(int betAmount)
        //{
        //    if(currentSubPhase != SubPhase.Dealing)
        //    {
        //        return;
        //    }

        //    betOnlyUIs.SetActive(false);
        //    blackJackOnlyUIs.SetActive(true);

        //    dealerScoreView.SetActiveText(false);
        //    playerData.SetScore(0);
        //    dealerData.SetScore(0);

        //    playerCards.ClearCards();
        //    dealerCards.ClearCards();

        //    currentSubPhase = SubPhase.Dealing;

        //    deck.InitializeDeck();
        //    deck.Shuffle();

        //    playerData.SetCard(new System.Collections.Generic.List<CardsManager.Card>());
        //    dealerData.SetCard(new System.Collections.Generic.List<CardsManager.Card>());

        //    playerCards.DrawCard(2);
        //    dealerCards.DrawInitialCards();

        //    currentSubPhase = SubPhase.PlyaerTurn;
        //    isInputLocked = false;
        //}

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
                            currentSubPhase = SubPhase.Judge;
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

                    currentSubPhase = SubPhase.Result;
                    break;

                case SubPhase.Result:
                    //結果表示処理

                    blackJackOnlyUIs.SetActive(false);
                    playerCards.ClearCards();

                    dealerCards.ClearCards();
                    if(isWin)
                    {
                        winUI.SetActive(true);

                        this.gameManager.GameResult = ResultPhase.Result.Win;
                    }
                    else
                    {
                        loseUI.SetActive(true);

                        this.gameManager.GameResult = ResultPhase.Result.Lose;
                    }

                    resultOnlyUI.SetActive(true);

                    GameManager.INSTANCE.Call("result");
                    break;
            }
        }

        /// <summary>
        /// ディーラーの思考/カードめくりを行うコルーチン
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator DealerTurnRoutine()
        {
            while(dealerData.GetScore() < 17)
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
            // gameManager.Play();
            isInputLocked = false;
            Debug.Log("ヒット終了");

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
            playerCards.Stand();
            isInputLocked = false;

            this.gameManager.Play("Select");
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

        private void JudgeResult()
        {
            bool playerBurst = ScoreCalclator.IsBurst(playerData.GetCard());
            bool dealerBurst = ScoreCalclator.IsBurst(dealerData.GetCard());
            int playerScore = playerData.GetScore();
            int dealerScore = dealerData.GetScore();

            int bet = playerData.GetBet();

            if(playerBurst)
            {
                // プレイヤ負け処理
                Debug.Log("プレイヤの負け");
                isWin = false;
            }
            else if(dealerBurst || playerScore > dealerScore)
            {
                float mutiplier = CalcultePayoutMultiplier();
                int payout = bet + Mathf.RoundToInt(bet * mutiplier);

                // プレイヤ勝ち
                Debug.Log("プレイヤの勝ち");
                isWin = true;
            }
            else if(playerScore < dealerScore)
            {
                // プレイヤ負け
                Debug.Log("プレイヤの負け");
                isWin = false;
            }
            else
            {
                playerData.AddValues(bet);

                // 引き分け
                Debug.Log("ひきわけ");
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
        /// ブラックジャックかどうか判定
        /// </summary>
        private bool IsBlackjack()
        {
            return playerData.GetCard().Count == 2 && playerData.GetScore() == 21;
        }

        protected override void Finish()
        {
            playerData.ResetBet();

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

            // デバッグ
            //this.gameManager.AddPlayerItemData(new ItemData(new DiceDefinition(), 1.0F, 1));

            if (contexts != null && contexts.Length >= 1 && contexts[0] is int index)
            {
                this.TryItemButtom(index);
                ItemSlotSetup();
            }
        }

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
    }
}
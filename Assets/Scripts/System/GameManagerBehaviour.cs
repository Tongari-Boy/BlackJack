using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.System
{
    public class GameManagerBehaviour : MonoBehaviour
    {
        [Header("スタート画面")]
        [SerializeField] private GameObject startCanvas;

        [Header("難易度セレクト画面")]
        [SerializeField] private GameObject selectCanvas;

        [Header("ベット画面")]
        [SerializeField] private GameObject betCanvas;

        [Header("アイテム購入画面")]
        [SerializeField] private GameObject shopCanvas;

        [Header("アイテム購入画面のアイテムスロット")]
        [SerializeField] private GameObject itemDisplaySlot;

        [Header("アイテム購入画面のカートスロット")]
        [SerializeField] private GameObject itemCartSlot;

        [Header("ブラックジャック画面")]
        [SerializeField] private GameObject blackjackCanvas;

        [Header("リザルト画面")]
        [SerializeField] private GameObject resultCanvas;

        [Header("リザルト画面の項目バー")]
        [SerializeField] private GameObject resultItemBar;

        [Header("リザルト画面のコントロールボタン")]
        [SerializeField] private GameObject resultControlButton;

        [Header("ブラックジャック関連")]
        [SerializeField] private Cards.Deck deck;
        [SerializeField] private Cards.PlayerCards playerCards;
        [SerializeField] private Cards.DealerCards dealerCards;
        [SerializeField] private Player.PlayerScoreView playerScoreView;
        [SerializeField] private Player.DealerScoreView dealerScoreView;
        [SerializeField] private Player.PlayerValueView playerValueView;
        [SerializeField] private GameObject blackJackOnlyUIs;

        [Header("ブラックジャックの結果UI")]
        [SerializeField] private GameObject resultOnlyUI;
        [SerializeField] private GameObject winUI;
        [SerializeField] private GameObject loseUI;

        [Header("確認ボタン")]
        [SerializeField] private GameObject confirmPopupPrefab;

        [Header("ベット関連")]
        [SerializeField] private GameObject betOnlyUIs;
        [SerializeField] private Bet.BetButtoms betButtoms;


        public GameObject StartCanvas
        {
            get { return this.startCanvas; }
        }

        public GameObject SelectCanvas
        {
            get { return this.selectCanvas; }
        }

        public GameObject BetCanvas
        {
            get { return this.betCanvas; }
        }

        public GameObject ShopCanvas
        {
            get { return this.shopCanvas; }
        }

        public GameObject ItemDisplaySlot
        {
            get { return this.itemDisplaySlot; }
        }

        public GameObject ItemCartSlot
        {
            get { return this.itemCartSlot; }
        }

        public GameObject BlackjackCanvas
        {
            get { return this.blackjackCanvas; }
        }

        public GameObject ResultCanvas
        {
            get { return this.resultCanvas; }
        }

        public GameObject ResultItemBar
        {
            get { return this.resultItemBar; }
        }

        public GameObject ResultControlButton
        {
            get { return this.resultControlButton; }
        }

        public Cards.Deck Deck => deck;
        public Cards.PlayerCards PlayerCards => playerCards;
        public Cards.DealerCards DealerCards => dealerCards;
        public Player.PlayerScoreView PlayerScoreView => playerScoreView;
        public Player.DealerScoreView DealerScoreView => dealerScoreView;
        public Player.PlayerValueView PlayerValueView => playerValueView;
        public GameObject BlackJackOnlyUIs => blackJackOnlyUIs;

        public GameObject ResultOnlyUI => resultOnlyUI;
        public GameObject WinUI => winUI;
        public GameObject LoseUI => loseUI;

        public GameObject ConfirmPopupPrefab => confirmPopupPrefab;

        public GameObject BetOnlyUIs => betOnlyUIs;

        public Bet.BetButtoms BetButtoms => betButtoms;

        void Awake()
        {
            GameManager.INSTANCE.Init(this);
        }

        void Update()
        {
            GameManager.INSTANCE.Update(this);
        }
    }
}
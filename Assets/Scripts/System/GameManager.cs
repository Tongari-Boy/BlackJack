using Assets.Scripts.System;
using Audio;
using Item;
using Cards;
using Player;
using System.Collections.Generic;
using UnityEngine;

namespace System
{
    /// <summary>
    /// <para>ゲームの進行を管理する</para>
    /// </summary>
    public class GameManager
    {
        public static readonly GameManager INSTANCE = new();

        private readonly Dictionary<string, GamePhase> gamePhases = new();

        public readonly PlayerData playerData;
        public readonly DealerData dealerData;

        private string bindingGamePhaseId;
        private GamePhase bindingGamePhase;

        private readonly Dictionary<string, ItemImageHolder> itemImageHolders = new();
        private readonly Dictionary<string, AudioSourceHolder> audioSourceHolders = new();

        private readonly List<ItemData> playerItemData = new();
        private readonly int playerItemCount = 6;

        private ResultPhase.Result gameResult = ResultPhase.Result.None;
        private float difficulty = 1.0F;
        private bool infiniteMoneyMode = false;

        private Deck deck;

        /// <summary>
        /// ゲームのリザルト
        /// </summary>
        public ResultPhase.Result GameResult
        {
            get { return this.gameResult; }
            set { this.gameResult = value; }
        }

        /// <summary>
        /// ゲームの難易度
        /// </summary>
        public float Difficulty
        {
            get { return this.difficulty; }
            set
            {
                float old = this.Quata;

                this.difficulty = Mathf.Max(0.0F, value);

                // デバッグ
                if (old != this.Quata)
                {
                    UnityEngine.Debug.Log($"難易度が変更されました！（ノルマ額：{old:N0} $ → {this.Quata:N0} $）");
                }
            }
        }

        /// <summary>
        /// ゲームのノルマ（難易度依存）
        /// </summary>
        public int Quata
        {
            get { return this.CalculateQuota(this.difficulty); }
        }

        /// <summary>
        /// ゲームのノルマを計算する
        /// </summary>
        public int CalculateQuota(float difficulty)
        {
            return (int)(600000.0D * difficulty);
        }

        /// <summary>
        /// 無限の所持金モード（デバッグ）
        /// </summary>
        public bool InfiniteMoneyMode
        {
            get { return this.infiniteMoneyMode; }
            set { this.infiniteMoneyMode = value; }
        }

        private GameManager()
        {
            // プレイヤー、ディーラーの初期化
            this.playerData = new(this);
            this.dealerData = new(this);
        }

        /// <summary>
        /// <para>GameManagerの初期化</para>
        /// </summary>
        public void Init(GameManagerBehaviour gameManagerBehaviour)
        {
            // フェーズの登録
            this.RegisterGamePhase("start", new StartPhase(this, gameManagerBehaviour));
            this.RegisterGamePhase("bet", new BetPhase(this, gameManagerBehaviour));
            this.RegisterGamePhase("select", new SelectPhase(this, gameManagerBehaviour));
            this.RegisterGamePhase("shop", new ShopPhase(this, gameManagerBehaviour));
            this.RegisterGamePhase("blackjack", new BlackjackPhase(this, gameManagerBehaviour));
            this.RegisterGamePhase("result", new ResultPhase(this, gameManagerBehaviour));

            this.Call("start");
        }

        /// <summary>
        /// <para>GameManagerの更新</para>
        /// </summary>
        public void Update(GameManagerBehaviour gameManagerBehaviour)
        {

            if (this.bindingGamePhase != null)
            {
                this.bindingGamePhase.DoUpdate();
            }
        }

        /// <summary>
        /// <para>GamePhaseを登録する</para>
        /// </summary>
        public bool RegisterGamePhase(string id, GamePhase gamePhase)
        {
            if (id == null || gamePhase == null)
                return false;

            // IDが存在する場合はそのGamePhaseは破棄する
            if (this.gamePhases.ContainsKey(id))
            {
                this.gamePhases[id]?.DoDestroy();
            }

            // GamePhaseの登録
            this.gamePhases[id] = gamePhase;

            // GamePhaseの初期化
            try
            {
                gamePhase.DoInit();
            }
            catch(System.Exception e)
            {
                UnityEngine.Debug.LogError($"GamePhase（ID: {id}）の初期化に失敗しました…\n{e}");

                gamePhase.DoDiscard();
            }

            // デバッグ
            UnityEngine.Debug.Log($"GamePhase（ID: {id}）が登録されました！");

            return true;
        }

        /// <summary>
        /// <para>GamePhaseを削除する</para>
        /// </summary>
        public bool DeleteGamePhase(string id)
        {
            if (id == null || !this.gamePhases.ContainsKey(id))
                return false;

            // GamePhaseの破棄
            this.gamePhases[id]?.DoDestroy();

            // デバッグ
            UnityEngine.Debug.Log($"GamePhase（ID: {id}）が削除されました！");

            return this.gamePhases.Remove(id);
        }

        /// <summary>
        /// <para>登録されたGamePhaseを返す</para>
        /// </summary>
        public T GetPhase<T>(string id) where T : GamePhase
        {
            if (id == null || !this.gamePhases.ContainsKey(id))
                return null;

            return this.gamePhases[id] as T;
        }

        /// <summary>
        /// <para>登録されたGamePhaseを呼び出す</para>
        /// </summary>
        public bool Call(string id)
        {
            if (id == null || !this.gamePhases.ContainsKey(id) || this.gamePhases[id] == null)
                return false;

            // フェーズを終了する
            if (this.bindingGamePhase != null)
            {
                this.bindingGamePhase.DoFinish();
            }

            this.bindingGamePhaseId = id;
            this.bindingGamePhase = this.gamePhases[id];

            // フェーズを開始する
            if (this.bindingGamePhase.DoStart())
            {
                UnityEngine.Debug.Log($"GamePhase（ID: {id}）が呼び出されました！");
            }
            else
            {
                UnityEngine.Debug.LogError($"GamePhase（ID: {id}）の呼び出しに失敗しました…");
            }

            return true;
        }

        /// <summary>
        /// <para>ゲームを終了する</para>
        /// </summary>
        public void Exit()
        {
            UnityEngine.Debug.Log("ゲームを終了しました！");
        }

        /// <summary>
        /// <para>ItemImageHolderを登録する</para>
        /// </summary>
        public void RegisterItemImageHolders(ItemImageHolder[] itemImageHolders)
        {
            foreach (ItemImageHolder itemImageHolder in itemImageHolders)
            {
                if (itemImageHolder != null && itemImageHolder.Name != null)
                {
                    this.itemImageHolders[ItemImageHolder.GetID(itemImageHolder)] = itemImageHolder;

                    UnityEngine.Debug.Log($"新しいItemImageHolder（Name: {itemImageHolder.Name}, Rarity: {itemImageHolder.Rarity}）が登録されました！");
                }
            }
        }

        /// <summary>
        /// <para>登録されたItemImageHolder</para>
        /// </summary>
        public ItemImageHolder GetItemImageHolder(string name, float rarity)
        {
            if (this.itemImageHolders.ContainsKey(ItemImageHolder.GetID(name, rarity)))
            {
                return this.itemImageHolders[ItemImageHolder.GetID(name, rarity)] ?? ItemImageHolder.EMPTY;
            }

            return ItemImageHolder.EMPTY;
        }

        /// <summary>
        /// <para>AudioSourceHolderを登録する</para>
        /// </summary>
        public void RegisterAudioSourceHolders(AudioSourceHolder[] audioSourceHolders)
        {
            foreach (AudioSourceHolder audioSourceHolder in audioSourceHolders)
            {
                if (audioSourceHolder != null && audioSourceHolder.Name != null && audioSourceHolder.AudioSource != null)
                {
                    this.audioSourceHolders[audioSourceHolder.Name] = audioSourceHolder;

                    UnityEngine.Debug.Log($"新しいAudioSourceHolder（Name: {audioSourceHolder.Name}）が登録されました！");
                }
            }
        }

        /// <summary>
        /// <para>登録されたAudioSourceHolderを再生する</para>
        /// </summary>
        public bool Play(string name)
        {
            if (this.audioSourceHolders.ContainsKey(name))
            {
                this.audioSourceHolders[name].Play();

                return true;
            }

            return false;
        }

        /// <summary>
        /// GamePhaseにイベントを発生させる
        /// </summary>
        public void Invoke(GameObject gameObject)
        {
            if (this.bindingGamePhase == null)
                return;

            this.bindingGamePhase.Invoke(gameObject);
        }

        /// <summary>
        /// GamePhaseにイベントを発生させる
        /// </summary>
        public void Invoke(GameObject gameObject, params object[] contexts)
        {
            if (this.bindingGamePhase == null)
                return;

            this.bindingGamePhase.Invoke(gameObject, contexts);
        }

        /// <summary>
        /// <para>プレイヤーのItemDataを増やす</para>
        /// </summary>
        public void AddPlayerItemData(params ItemData[] itemData)
        {
            if (this.playerItemData == null || itemData == null)
                return;

            int itemDataSize = Mathf.Clamp(this.playerItemData.Count, 0, this.playerItemCount);

            int totalCount;
            int itemDataIndex;
            bool addFlag = false;

            ItemData havingItemData;

            foreach (ItemData itemDataToAdd in itemData)
            {
                if (itemDataToAdd == null || itemDataToAdd.Equals(ItemData.EMPTY))
                    continue;

                totalCount = 0;
                itemDataIndex = -1;
                addFlag = false;

                for (int i = 0; i < itemDataSize; ++i)
                {
                    havingItemData = this.playerItemData[i];

                    if (havingItemData == null || havingItemData.Equals(ItemData.EMPTY))
                    {
                        // 空のアイテムスロットを記憶する
                        itemDataIndex = i;
                    }
                    else if (itemDataToAdd.Equals(havingItemData))
                    {
                        // ItemData.Countを加算する
                        havingItemData.Count += itemDataToAdd.Count;
                        totalCount = havingItemData.Count;
                        addFlag = true;

                        break;
                    }
                }

                // ItemDataのリストに追加する
                if (!addFlag && this.playerItemData.Count < this.playerItemCount)
                {
                    this.playerItemData.Add(itemDataToAdd);

                    totalCount = itemDataToAdd.Count;
                    addFlag = true;
                }

                // デバッグ
                if (addFlag && itemDataToAdd.Count > 0)
                {
                    UnityEngine.Debug.Log($"プレイヤーにアイテム（Name: {itemDataToAdd.Information}）を{itemDataToAdd.Count}コ追加しました！（合計：{totalCount}コ）");
                }
            }
        }

        /// <summary>
        /// <para>プレイヤーのItemDataを使用する</para>
        /// </summary>
        public void UsePlayerItemData(ItemData itemData, bool isForce = false)
        {
            if (this.playerItemData == null || itemData == null || itemData.Equals(ItemData.EMPTY))
                return;

            // ItemDataを強制的に使用する
            if (isForce)
            {
                itemData.DoUse(this.playerData, this.dealerData,this.deck);

                return;
            }

            // プレイヤーが所持するItemDataを使用する
            foreach (ItemData havingItemData in this.playerItemData)
            {
                if (havingItemData != null && !havingItemData.Equals(ItemData.EMPTY) && havingItemData.Equals(itemData) && havingItemData.CanUse(this.playerData, this.dealerData,this.deck))
                {
                    havingItemData.DoUse(this.playerData, this.dealerData,this.deck);
                }
            }
        }

        /// <summary>
        /// <para>プレイヤーのItemDataを使用する</para>
        /// </summary>
        public void UsePlayerItemData(int index)
        {
            ItemData itemData = this.playerItemData[index];

            if (itemData != null && !itemData.Equals(ItemData.EMPTY) && itemData.CanUse(this.playerData, this.dealerData,this.deck))
            {
                itemData.DoUse(this.playerData, this.dealerData,this.deck);
            }
        }

        /// <summary>
        /// <para>プレイヤーからItemDataを取得する</para>
        /// <para>存在しない場合はItemData.EMPTYを返す</para>
        /// </summary>
        public ItemData GetPlayerItemData(int index)
        {
            if (index < 0 || this.playerItemData == null || index >= this.playerItemData.Count)
                return ItemData.EMPTY;

            return this.playerItemData[index] ?? ItemData.EMPTY;
        }

        /// <summary>
        /// <para>プレイヤーからItemDataのリストを取得する</para>
        /// </summary>
        public List<ItemData> GetAllPlayerItemData()
        {
            return new(this.playerItemData);
        }


        /// <summary>
        /// 使用中の山札
        /// </summary>
        public Deck Deck
        {
            get { return this.deck; }
        }

        /// <summary>
        /// Deckを登録する
        /// </summary>
        /// <param name="deck"></param>
        public void ResisterDeck(Deck deck)
        {
            this.deck = deck;
        }
    }
}

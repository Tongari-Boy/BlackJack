using Assets.Scripts.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace System
{
    /// <summary>
    /// <para>難易度の選択のフェーズを定義する</para>
    /// </summary>
    public class SelectPhase : GamePhase
    {
        private GameObject canvasObject;

        public SelectPhase(GameManager gameManager, GameManagerBehaviour gameManagerBehaviour) : base(gameManager, gameManagerBehaviour) { }

        protected override void Init()
        {
            if (this.gameManagerBehaviour.SelectCanvas == null)
                return;

            this.canvasObject = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.SelectCanvas);
            this.canvasObject.SetActive(false);
        }

        protected override void Start()
        {
            if (this.canvasObject == null)
                return;

            UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.canvasObject, "Easy/Texts"), textMeshProUGUI => textMeshProUGUI.text = $"EASY [Quota: {this.gameManager.CalculateQuota(0.05F):N0} $]");
            UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.canvasObject, "Normal/Texts"), textMeshProUGUI => textMeshProUGUI.text = $"NORMAL [Quota: {this.gameManager.CalculateQuota(0.1F):N0} $]");
            UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.canvasObject, "Hard/Texts"), textMeshProUGUI => textMeshProUGUI.text = $"HARD [Quota: {this.gameManager.CalculateQuota(0.5F):N0} $]");
            UIUtil.InvokeIfPresent<TextMeshProUGUI>(UIUtil.GetChild(this.canvasObject, "Message/Money"), textMeshProUGUI => textMeshProUGUI.text = $"You currently have {this.gameManager.playerData.GetValues():N0} $");

            this.canvasObject.SetActive(true);
        }

        protected override void Update()
        {
        }

        protected override void Finish()
        {
            if (this.canvasObject == null)
                return;

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
                case "Easy":
                    this.gameManager.Difficulty = 0.05F;
                    this.gameManager.Play("Select");

                    break;
                case "Normal":
                    this.gameManager.Difficulty = 0.1F;
                    this.gameManager.Play("Select");

                    break;
                case "Hard":
                    this.gameManager.Difficulty = 0.5F;
                    this.gameManager.Play("Select");

                    break;
                case "OK":
                    this.gameManager.Call("bet");
                    this.gameManager.Play("Select");

                    break;
            }
        }
    }
}

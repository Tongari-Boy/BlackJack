using Assets.Scripts.System;
using System;
using UnityEngine;

public class PerfectPhase:GamePhase
{

    private GameObject perfectCanvas;

    public PerfectPhase(GameManager gameManager, GameManagerBehaviour gameManagerBehaviour) : base(gameManager, gameManagerBehaviour) { }

    protected override void Init()
    {
        if (this.gameManagerBehaviour.PerfectCanvas == null)
            return;

        this.perfectCanvas = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.PerfectCanvas);

        perfectCanvas.SetActive(false);
    }

    protected override void Start()
    {
        perfectCanvas.SetActive(true);

        if(this.gameManager.GameResult == ResultPhase.Result.PerfectWin)
        {
            Debug.Log("かち");
        }
        else
        {
            Debug.Log("まけ");
        }
    }

    protected override void Update()
    {
    }
    
    protected override void Finish()
    {
        perfectCanvas.SetActive(false);
        this.gameManager.playerData.SetValues(50000);
    }
    
    protected override void Destroy()
    {
        perfectCanvas = null;
    }

    public override void Invoke(GameObject gameObject)
    {
        this.gameManager.Call("start");
    }
}
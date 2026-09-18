using Assets.Scripts.System;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class PerfectPhase:GamePhase
{

    private GameObject perfectCanvas;
    private GameObject perfectWinObject;
    private GameObject perfectLoseObject;

    public PerfectPhase(GameManager gameManager, GameManagerBehaviour gameManagerBehaviour) : base(gameManager, gameManagerBehaviour) { }

    protected override void Init()
    {
        if (this.gameManagerBehaviour.PerfectCanvas == null)
            return;
        this.perfectCanvas = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.PerfectCanvas);
        perfectCanvas.SetActive(false);

        if (this.gameManagerBehaviour.PerfectWinObject == null)
            return;
        this.perfectWinObject = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.PerfectWinObject);
        perfectWinObject.SetActive(false);

        if (this.gameManagerBehaviour.PerfectLoseObject == null)
            return;
        this.perfectLoseObject = UnityEngine.Object.Instantiate(this.gameManagerBehaviour.PerfectLoseObject);
        perfectLoseObject.SetActive(false);
    }

    protected override void Start()
    {
        perfectCanvas.SetActive(true);

        if(this.gameManager.GameResult == ResultPhase.Result.PerfectWin)
        {
            Debug.Log("Perfect_かち");
            perfectWinObject.SetActive(true);
        }
        else
        {
            Debug.Log("Perfect_まけ");
            perfectLoseObject.SetActive(true);
        }
    }

    protected override void Update()
    {
    }
    
    protected override void Finish()
    {
        perfectCanvas.SetActive(false);
        perfectWinObject.SetActive(false);
        perfectLoseObject.SetActive(false);
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
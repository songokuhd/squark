﻿using UnityEngine;
using System.Collections;

public class RunnerGameOverController : GameOverController 
{
    public RunnerGameOverController()
        : base(Names.RunnerDemoScene)
    { }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnGUI()
    {
        base.OnGUI();
    }

    protected override void Retry()
    {
        base.Retry();

        Application.LoadLevel(Names.RunnerDemoScene);
    }
}

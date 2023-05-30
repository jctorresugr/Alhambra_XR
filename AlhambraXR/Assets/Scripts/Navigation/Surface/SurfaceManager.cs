using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static SurfaceAnalysisRANSAC;

public class SurfaceManager : MonoBehaviour
{
    // input data
    [Header("Input data")]
    public MeshFilter meshFilter;

    // algorithm objects
    [Header("Algorithms")]
    public SurfaceAnalysisRANSAC surfaceAnalysisRANSAC;
    public SurfaceAnalyzeSemantic surfaceAnalyzeSemantic;
    public DisplaySurface displaySurface;

    [Header("Load & Compute Setups")]
    public bool computeForAnalyze = true;
    public bool useCacheFirst = true;
    private bool isFinished = false;
    public bool showDebugSurface = false;
    public string filePath = "MiddleSave/surfaceInformation.json";

    public bool IsFinished
    {
        get
        {
            return isFinished;
        }
        set
        {
            isFinished = value;
            if(isFinished)
            {
                Debug.Log("Writing files...");
                Utils.SaveFile(filePath, JsonUtility.ToJson(surfaceInfos));
                DebugSurface();
            }
        }
    }


    // final data
    [Header("Final Surface Data")]
    [SerializeField]
    public SurfaceInfoBundle surfaceInfos;

    public void Start()
    {
        surfaceAnalysisRANSAC.finishEvent += ComputeStep2;
        LoadCache(filePath);
        if(!IsFinished)
        {
            Compute(meshFilter);
        }
    }

    public void LoadCache(string filePath)
    {
        this.filePath = filePath;
        if (useCacheFirst)
        {
            if(File.Exists(filePath))
            {
                try
                {
                    surfaceInfos = JsonUtility.FromJson<SurfaceInfoBundle>(Utils.ReadFile(filePath));
                    IsFinished = true;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Debug.Log("Failed to load cache from " + filePath);
                }
            }
        }
    }

    public void Compute(MeshFilter meshFilter)
    {
        surfaceAnalysisRANSAC.StartCompute(meshFilter);
    }

    protected void ComputeStep2(SurfaceInfoBundle step1Data)
    {
        surfaceInfos = surfaceAnalyzeSemantic.StartCompute(step1Data);
        IsFinished = true;
    }

    protected void DebugSurface()
    {
        if (showDebugSurface && displaySurface != null)
        {
            displaySurface.surfaceInfos = surfaceInfos;
            displaySurface.DebugSurface();
        }
    }


}

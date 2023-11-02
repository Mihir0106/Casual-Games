using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundle
{
    [MenuItem("Assets/Create Assets Bundles")]
    private static void BuildAllAssetBundles()
    {
        string assetBundleDirectoryPath = Application.dataPath + "/../AssetBundles";

        try
        {
            BuildPipeline.BuildAssetBundles(assetBundleDirectoryPath, BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
}

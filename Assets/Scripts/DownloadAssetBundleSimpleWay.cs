using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadAssetBundleSimpleWay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(DownLoadAssetBundleFromServer());
    }

    public void DownloadAssets()
    {
        StartCoroutine(DownLoadAssetBundleFromServer());
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator DownLoadAssetBundleFromServer()
    {
        //GameObject go = null;
        
        // Link Format :  "MyServer/MyAssetBundle/myGameObject"
        string url = "https://drive.google.com/uc?export=download&id=1YW3FKCrbnRPPpoRD0RNp1H7d9o-zeTEK";
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("Error on the get request at" + url + " " + www.error);
            }
            else
            {
                    
                //AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                //go = bundle.LoadAsset(bundle.GetAllAssetNames()[0]) as GameObject;
                //bundle.Unload(false);
                    
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                LoadAndInstantiateAllAssetsFromBundle(bundle);
                bundle.Unload(false);
                    
                    
                yield return new WaitForEndOfFrame();
            }
            www.Dispose();
            //InstantiateGameObjectFromAssetBundle(go);
        }
    }

    void LoadAndInstantiateAllAssetsFromBundle(AssetBundle bundle)
    {
        if (bundle != null)
        {
            string[] assetNames = bundle.GetAllAssetNames();
            foreach (string assetName in assetNames)
            {
                GameObject go = bundle.LoadAsset(assetName) as GameObject;
                if (go != null)
                {
                    InstantiateGameObjectFromAssetBundle(go);
                }
                else
                {
                    Debug.LogWarning("Failed to load asset: " + assetName);
                }
            }
        }
        else
        {
            Debug.LogWarning("Asset Bundle is null.");
        }
    }
        
    void InstantiateGameObjectFromAssetBundle(GameObject go)
    {
        if(go != null)
        {
            Instantiate(go);
            //GameObject instanceGo = Instantiate(go);
            //instanceGo.transform.position = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("your asset bundle gameObject go is null");
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Editor
{
    public class DownloadAssetBundleSimpleWay : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(DownLoadAssetBundleFromServer());
        }

        IEnumerator DownLoadAssetBundleFromServer()
        {
            GameObject go = null;
        
            // LinkFormate :  "MyServer/MyAssetBundle/myGameObject"
            string url = "https://drive.google.com/uc?export=download&id=1EBcnpnur5kNxjnPG9sDqr2vTya6de_3a";
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
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                    go = bundle.LoadAsset(bundle.GetAllAssetNames()[0]) as GameObject;
                    bundle.Unload(false);
                    yield return new WaitForEndOfFrame();
                }
                www.Dispose();
                InstantiateGameObjectFromAssetBundle(go);
            }
        }

        void InstantiateGameObjectFromAssetBundle(GameObject go)
        {
            if(go != null)
            {
                GameObject instanceGo = Instantiate(go);
                instanceGo.transform.position = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("your asset bundle gameObject go is null");
            }
        }
    }
}

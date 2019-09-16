using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ResourceTest : MonoBehaviour
{
    private void Start()
    {
        TestLoadAB();
    }

    private void TestLoadAB()
    {
       // AssetBundle configAB = AssetBundle.LoadFromFile(string.Format("{0}/RealFram/Data/ABData/AssetBundleConfig", Application.dataPath));
       // TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        TextAsset textAsset = Resources.Load<TextAsset>("AssetBundleConfig");
        AssetBundleConfig config = null;
        using (MemoryStream ms = new MemoryStream(textAsset.bytes))
        {
            BinaryFormatter bf = new BinaryFormatter();
            config = bf.Deserialize(ms) as AssetBundleConfig;
        }
        string path = "Assets/RealFram/ABTest/Cube.prefab";
        uint crc = Crc32.GetCrc32(path);
        ABBase abBase = null;
        for (int i = 0; i < config.ABList.Count; i++)
        {
            if (config.ABList[i].Crc == crc)
            {
                abBase = config.ABList[i];
            }
        }

        for (int i = 0; i < abBase.ABDependce.Count; i++)
        {
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABDependce[i]);

        }

      
        AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.dataPath+ "/"+"Resources/" + abBase.ABName);
        GameObject obj = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>(abBase.AssetName));

        GameObject obj2 = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>("cylinder"));
    }
}

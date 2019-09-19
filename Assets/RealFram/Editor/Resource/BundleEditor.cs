﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class BundleEditor
{
    private static string m_BunleTargetPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();
    private static string ABCONFIGPATH = "Assets/RealFram/Editor/Resource/ABConfig.asset";
    private static string ABBYTEPATH = RealConfig.GetRealFram().m_ABBytePath;
    //key是ab包名，value是路径，所有文件夹ab包dic
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    //过滤的list
    private static List<string> m_AllFileAB = new List<string>();
    //单个prefab的ab包
    private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
    //储存所有有效路径
    private static List<string> m_ConfigFil = new List<string>();
    [MenuItem("Tools/buildJson")]
    public static void BuildJson()
    {
        
        //AssetBundle configAB = AssetBundle.LoadFromFile(ABCONFIGPATH);
        TextAsset textAsset = Resources.Load("AssetbundleConfig1") as TextAsset;
        if (textAsset == null)
        {
            Debug.LogError("AssetBundleConfig is no exist!");

        }
        string json = textAsset.text;
        Debug.Log(json);
        var Assetssss = JsonUtility.FromJson<AssetBundleJson>(json);
        foreach(var o in Assetssss.ABbundles)
        {
            Debug.Log(o.ABName);
            Debug.Log(o.crc);
            Debug.Log(o.path);
        }
        //AssetBundleJson configJson = new AssetBundleJson();
        //configJson.ABbundles = new List<bundle>();
        //bundle abBase = new bundle();
        //abBase.ABName = "tsas";
        //abBase.crc = 1234234;
        //abBase.path = "dsfsd";

        //configJson.ABbundles.Add(abBase);
        //bundle abBase2 = new bundle();
        //abBase2.ABName = "tsas";
        //abBase2.crc = 1234234;
        //abBase2.path = "dsfsd";

        //configJson.ABbundles.Add(abBase2);

        //foreach (bundle o in configJson.ABbundles)
        //{
        //    Debug.Log(o.ABName);
        //    Debug.Log(o.crc);
        //    Debug.Log(o.path);
        //}
        //string jsonPath = Application.dataPath + "/JsonTest/AssetbundleConfig.json";
        //string json = EditorJsonUtility.ToJson(configJson);
        //Debug.Log("json:   final   " + json);
        //System.IO.File.WriteAllText(jsonPath, json);
        //Debug.Log(EditorUserBuildSettings.activeBuildTarget);



        //foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        //{
        //    var path = AssetDatabase.GetAssetPath(obj);
        //    Debug.Log(path);
        //}

    }
    [MenuItem("Tools/打包")]
    public static void Build()
    {
        DataEditor.AllXmlToBinary();
        m_ConfigFil.Clear();
        m_AllFileAB.Clear();
        m_AllFileDir.Clear();
        m_AllPrefabDir.Clear();
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        foreach (ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
        {
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.LogError("AB包配置名字重复，请检查！");
            }
            else
            {
                m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                m_AllFileAB.Add(fileDir.Path);
                m_ConfigFil.Add(fileDir.Path);
            }
        }

        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPath.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i * 1.0f / allStr.Length);
            m_ConfigFil.Add(path);
            if (!ContainAllFileAB(path))
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDepend = AssetDatabase.GetDependencies(path);
                List<string> allDependPath = new List<string>();
                m_AllFileAB.Add(path);
                allDependPath.Add(path);
                for (int j = 0; j < allDepend.Length; j++)
                {
                    if (!ContainAllFileAB(allDepend[j]) && !allDepend[j].EndsWith(".cs") && !allDepend[j].EndsWith(".prefab"))
                    {
                        m_AllFileAB.Add(allDepend[j]);
                        allDependPath.Add(allDepend[j]);
                    }
                }
                if (m_AllPrefabDir.ContainsKey(obj.name))
                {
                    Debug.LogError("存在相同名字的Prefab！名字：" + obj.name);
                }
                else
                {
                    m_AllPrefabDir.Add(obj.name, allDependPath);
                }
            }
        }

        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }

        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }

        BunildAssetBundle();

        string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            EditorUtility.DisplayProgressBar("清除AB包名", "名字：" + oldABNames[i], i * 1.0f / oldABNames.Length);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    static void SetABName(string name, string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.LogError("不存在此路径文件：" + path);
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }

    static void SetABName(string name, List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            SetABName(name, paths[i]);
        }
    }

    static void BunildAssetBundle()
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        //key为全路径，value为包名
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; i++)
        {
            string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            for (int j = 0; j < allBundlePath.Length; j++)
            {
                if (allBundlePath[j].EndsWith(".cs"))
                    continue;

                Debug.Log("此AB包：" + allBundles[i] + "下面包含的资源文件路径：" + allBundlePath[j]);
                resPathDic.Add(allBundlePath[j], allBundles[i]);
            }
        }

        if (!Directory.Exists(m_BunleTargetPath))
        {
            Directory.CreateDirectory(m_BunleTargetPath);
        }

        DeleteAB();
        //生成自己的配置表
        WriteData(resPathDic);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(m_BunleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        if (manifest == null)
        {
            Debug.LogError("AssetBundle 打包失败！");
        }
        else
        {
            Debug.Log("AssetBundle 打包完毕");
        }
    }

    static void WriteData(Dictionary<string ,string> resPathDic)
    {
        AssetBundleJson configJson = new AssetBundleJson();
        configJson.ABbundles = new List<bundle>();
        foreach(string path in resPathDic.Keys)
        {
            if (!ValidPath(path))
                continue;
            bundle abBase = new bundle();
            abBase.path = path;
            abBase.crc = Crc32.GetCrc32(path);
            abBase.ABName = resPathDic[path];
            abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
            abBase.ABDependce = new List<string>();
            string[] resDependce = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < resDependce.Length; i++)
            {
                string tempPath = resDependce[i];
                if (tempPath == path || path.EndsWith(".cs"))
                    continue;

                string abName = "";
                if (resPathDic.TryGetValue(tempPath, out abName))
                {
                    if (abName == resPathDic[path])
                        continue;

                    if (!abBase.ABDependce.Contains(abName))
                    {
                        abBase.ABDependce.Add(abName);
                    }
                }
            }
            configJson.ABbundles.Add(abBase);
        }





        //AssetBundleConfig config = new AssetBundleConfig();
        //config.ABList = new List<ABBase>();
        //foreach (string path in resPathDic.Keys)
        //{
        //    if (!ValidPath(path))
        //        continue;

        //    ABBase abBase = new ABBase();
        //    abBase.Path = path;
        //    abBase.Crc = Crc32.GetCrc32(path);
        //    abBase.ABName = resPathDic[path];
        //    abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);
        //    abBase.ABDependce = new List<string>();
        //    string[] resDependce = AssetDatabase.GetDependencies(path);
        //    for (int i = 0; i < resDependce.Length; i++)
        //    {
        //        string tempPath = resDependce[i];
        //        if (tempPath == path || path.EndsWith(".cs"))
        //            continue;

        //        string abName = "";
        //        if (resPathDic.TryGetValue(tempPath, out abName))
        //        {
        //            if (abName == resPathDic[path])
        //                continue;

        //            if (!abBase.ABDependce.Contains(abName))
        //            {
        //                abBase.ABDependce.Add(abName);
        //            }
        //        }
        //    }
        //    config.ABList.Add(abBase);
        //    Debug.Log("ABList" + abBase.ABName + abBase.Path);
        //}

        //写入xml
        //string xmlPath = Application.dataPath + "/AssetbundleConfig.xml";
        //if (File.Exists(xmlPath)) File.Delete(xmlPath);
        //FileStream fileStream = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        //StreamWriter sw = new StreamWriter(fileStream, System.Text.Encoding.UTF8);
        //XmlSerializer xs = new XmlSerializer(config.GetType());
        //xs.Serialize(sw, config);
        //sw.Close();
        //fileStream.Close();

        // write to json
        string jsonPath = Application.dataPath + "/JsonTest/AssetbundleConfig.json";
        string json = EditorJsonUtility.ToJson(configJson);
        Debug.Log("json:   final   "+json);
        System.IO.File.WriteAllText(jsonPath, json);

        ////写入二进制
        //foreach (ABBase abBase in config.ABList)
        //{
        //    abBase.Path = "";
        //}
        //FileStream fs = new FileStream(ABBYTEPATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        //fs.Seek(0, SeekOrigin.Begin);
        //fs.SetLength(0);
        //BinaryFormatter bf = new BinaryFormatter();
        //bf.Serialize(fs, config);
        //fs.Close();
        //AssetDatabase.Refresh();
        //SetABName("assetbundleconfig", ABBYTEPATH);
    }

    /// <summary>
    /// 删除无用AB包
    /// </summary>
    static void DeleteAB()
    {
        string[] allBundlesName = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo direction = new DirectoryInfo(m_BunleTargetPath);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (ConatinABName(files[i].Name, allBundlesName) || files[i].Name.EndsWith(".meta")|| files[i].Name.EndsWith(".manifest") || files[i].Name.EndsWith("assetbundleconfig"))
            {
                continue;
            }
            else
            {
                Debug.Log("此AB包已经被删或者改名了：" + files[i].Name);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                }
                if(File.Exists(files[i].FullName + ".manifest"))
                {
                    File.Delete(files[i].FullName + ".manifest");
                }
            }
        }
    }

    /// <summary>
    /// 遍历文件夹里的文件名与设置的所有AB包进行检查判断
    /// </summary>
    /// <param name="name"></param>
    /// <param name="strs"></param>
    /// <returns></returns>
    static bool ConatinABName(string name, string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            if (name == strs[i])
                return true;
        }
        return false;
    }

    /// <summary>
    /// 是否包含在已经有的AB包里，做来做AB包冗余剔除
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            if (path == m_AllFileAB[i] || (path.Contains(m_AllFileAB[i]) && (path.Replace(m_AllFileAB[i],"")[0] == '/')))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 是否有效路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static bool ValidPath(string path)
    {
        for (int i = 0; i < m_ConfigFil.Count; i++)
        {
            if (path.Contains(m_ConfigFil[i]))
            {
                return true;
            }
        }
        return false;
    }
}

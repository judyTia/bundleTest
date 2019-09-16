using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class xmlTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var monsterCollection = MonsterContainer.Load(Path.Combine(Application.dataPath, "RealFram/xmlTest/monsters.xml"));
        Monster[] m_monsters = monsterCollection.Monsters;
        Debug.Log(m_monsters[0].Name);
        Debug.Log(m_monsters[0].Health);
        Debug.Log(m_monsters[0].Description);
        Debug.Log(m_monsters[0].Funny);
        var monsterCollection2 = new MonsterContainer();
        monsterCollection2.Monsters = new Monster[1];
        Monster mMonter = new Monster();
        mMonter.Name = "test2";
        mMonter.Health = 10;
        mMonter.Funny = "heyhey";
        mMonter.Description = "running away";
        monsterCollection2.Monsters[0] = mMonter;
        monsterCollection2.Save(Path.Combine(Application.dataPath, "RealFram/xmlTest/monsters.xml"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

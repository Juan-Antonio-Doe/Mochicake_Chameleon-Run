using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsDatabase", menuName = "Scriptables/Levels/Data/Levels Database SO")]
public class LevelsDatabaseSO : ScriptableObject {

	[field: SerializeField] public LevelData[] levelDatas = new LevelData[0];

}
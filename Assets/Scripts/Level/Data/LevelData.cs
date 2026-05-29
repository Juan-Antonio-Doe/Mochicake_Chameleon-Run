using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData {

	[field: Header("Level data")]
	[field: SerializeField] public int level {  get; set; }
	[field: SerializeField] public string levelName { get; set; }
	[field: SerializeField] public string maxCollectibles { get; set; }

}
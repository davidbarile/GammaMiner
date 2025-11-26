using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DontDestroyOnLoad : MonoBehaviour 
{
	private void Awake() 
	{
		DontDestroyOnLoad( gameObject );
	}
}

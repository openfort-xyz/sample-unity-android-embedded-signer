using System;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using TMPro;

public class GameSharedUI : MonoBehaviour
{
	#region Singleton class: GameSharedUI

	public static GameSharedUI Instance;

	void Awake ()
	{
		if (Instance == null) {
			Instance = this;
		}
	}

	#endregion

	[SerializeField] TMP_Text[] coinsUIText;

	void OnEnable()
	{
		if (FirebaseManager.Instance.initialized)
		{
			UpdateCoinsUIText();
		}
		else
		{
			FirebaseManager.Instance.OnFirebaseInitialized += FirebaseManager_OnFirebaseInitialized_Handler;
		}
	}

	private void OnDisable()
	{
		FirebaseManager.Instance.OnFirebaseInitialized -= FirebaseManager_OnFirebaseInitialized_Handler;
	}
	
	private void FirebaseManager_OnFirebaseInitialized_Handler(FirebaseAuth arg1, FirebaseFirestore arg2)
	{
		UpdateCoinsUIText();
	}

	public void UpdateCoinsUIText ()
	{
		for (int i = 0; i < coinsUIText.Length; i++) {
			SetCoinsText (coinsUIText [i], GameDataManager.GetCoins());
		}
	}

	void SetCoinsText (TMP_Text textMesh, int value)
	{
		// if (value >= 1000000)...
		// .....

		if (value >= 1000)
			textMesh.text = string.Format ("{0}K.{1}", (value / 1000), GetFirstDigitFromNumber (value % 1000));
		else
			textMesh.text = value.ToString ();
	}

	int GetFirstDigitFromNumber (int num)
	{
		return int.Parse (num.ToString () [0].ToString ());
	}
}

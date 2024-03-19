using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

//Shop Data Holder
[System.Serializable]
public class CharactersShopData
{
	public List<int> purchasedCharactersIndexes = new List<int>();
}
//Player Data Holder
[System.Serializable]
public class PlayerData
{
	public int coins = 0;
	public int selectedCharacterIndex = 0;
}

public static class GameDataManager
{
	static PlayerData playerData = new PlayerData();
	static CharactersShopData charactersShopData = new CharactersShopData();
	private static FirebaseFirestore db;
	static Character selectedCharacter;

	static GameDataManager()
	{
		db = FirebaseFirestore.DefaultInstance;
		LoadPlayerData();
		LoadCharactersShopData();
	}

	//Player Data Methods -----------------------------------------------------------------------------
	public static Character GetSelectedCharacter()
	{
		return selectedCharacter;
	}

	public static void SetSelectedCharacter(Character character, int index)
	{
		selectedCharacter = character;
		playerData.selectedCharacterIndex = index;
		SavePlayerData();
	}

	public static int GetSelectedCharacterIndex()
	{
		return playerData.selectedCharacterIndex;
	}

	public static int GetCoins()
	{
		return playerData.coins;
	}

	public static void AddCoins(int amount)
	{
		playerData.coins += amount;
		SavePlayerData();
	}

	public static bool CanSpendCoins(int amount)
	{
		return (playerData.coins >= amount);
	}

	public static void SpendCoins(int amount)
	{
		playerData.coins -= amount;
		SavePlayerData();
	}

	static void LoadPlayerFromFirestore()
	{
		db.Collection("player").GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted && !task.IsFaulted)
			{
				UnityEngine.Debug.Log("<color=green>[CharactersShopData] Loaded Player from Firestore.</color>");
			}
			else
			{
				UnityEngine.Debug.LogError("Failed to load characters from Firestore.");
			}
		});
	}


	static void LoadPlayerData()
	{
		// LoadPlayerFromFirestore();
		playerData = BinarySerializer.Load<PlayerData>("player-data.txt");
		UnityEngine.Debug.Log("<color=green>[PlayerData] Loaded.</color>");
	}

	static void SavePlayerData()
	{
		BinarySerializer.Save(playerData, "player-data.txt");
		UnityEngine.Debug.Log("<color=magenta>[PlayerData] Saved.</color>");
	}

	//Characters Shop Data Methods -----------------------------------------------------------------------------
	public static void AddPurchasedCharacter(int characterIndex)
	{
		charactersShopData.purchasedCharactersIndexes.Add(characterIndex);
		SaveCharactersShopData();
	}

	public static List<int> GetAllPurchasedCharacter()
	{
		return charactersShopData.purchasedCharactersIndexes;
	}

	public static int GetPurchasedCharacter(int index)
	{
		return charactersShopData.purchasedCharactersIndexes[index];
	}

	static void LoadCharactersFromFirestore()
	{
		db.Collection("characters").GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			if (task.IsCompleted && !task.IsFaulted)
			{
				// UnityEngine.Debug.Log(task.Result.Documents);
				// foreach (DocumentSnapshot characterDoc in task.Result.Documents)
				// {
				// 	charactersShopData.purchasedCharactersIndexes.Add(int.Parse(characterDoc.Id
				// }
				// UnityEngine.Debug.Log("<color=green>[CharactersShopData] Loaded Characters from Firestore.</color>");
			}
			else
			{
				UnityEngine.Debug.LogError("Failed to load characters from Firestore.");
			}
		});
	}

	static void LoadCharactersShopData()
	{
		// LoadCharactersFromFirestore();
		charactersShopData = BinarySerializer.Load<CharactersShopData>("characters-shop-data.txt");
		UnityEngine.Debug.Log("<color=green>[CharactersShopData] Loaded.</color>");
	}

	static void SaveCharactersShopData()
	{
		BinarySerializer.Save(charactersShopData, "characters-shop-data.txt");
		UnityEngine.Debug.Log("<color=magenta>[CharactersShopData] Saved.</color>");
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Openfort;
using Openfort.Model;
using Openfort.Recovery;
using UnityEngine;
using UnityEngine.Networking;
using static Clients.Shield;
public class OpenfortController : MonoBehaviour
{
	public static OpenfortController Instance { get; private set; }

	private const string PublishableKey = "pk_test_505bc088-905e-5a43-b60b-4c37ed1f887a";
	private const string ShieldKey = "a4b75269-65e7-49c4-a600-6b5d9d6eec66";
	private const string ShieldEncKey = "/cC/ElEv1bCHxvbE/UUH+bLIf8nSLZOrxj8TkKChiY4=";

	[HideInInspector] public string accessToken;
	private OpenfortSDK Openfort;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private async Task SetAutomaticRecoveryMethod(string idToken)
	{
		int chainId = 80001;
		OpenfortAuthOptions shieldConfig = new OpenfortAuthOptions
		{ authProvider = ShieldAuthProvider.Openfort, openfortOAuthProvider = OpenfortOAuthProvider.Firebase, openfortOAuthToken = idToken, openfortOAuthTokenType = OpenfortOAuthTokenType.IdToken };

		await Openfort.ConfigureEmbeddedSigner(chainId, shieldConfig);
	}

	public async void AuthenticateWithOAuth(string idToken, string accessToken)
	{
		Debug.Log("Google Sign-In Success! Token: " + idToken);
		accessToken = accessToken;
		Debug.Log("Openfort Auth");
		Openfort = new OpenfortSDK(PublishableKey);
		await Openfort.AuthenticateWithThirdPartyProvider("firebase", idToken, TokenType.IdToken);
		await SetAutomaticRecoveryMethod(idToken);
	}

	public async UniTask<string> Mint()
	{
		if (string.IsNullOrEmpty(accessToken))
		{
			Debug.LogError($"mAccessToken is null or empty");
			return null;
		}

		var webRequest = UnityWebRequest.Post("http://localhost:3000/mint", "");
		webRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
		webRequest.SetRequestHeader("Content-Type", "application/json");
		webRequest.SetRequestHeader("Accept", "application/json");
		await SendWebRequestAsync(webRequest);

		Debug.Log("Mint request sent");
		if (webRequest.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("Mint Failed: " + webRequest.error);
			return null;
		}


		var responseText = webRequest.downloadHandler.text;
		Debug.Log("Mint Response: " + responseText);
		var responseJson = JsonConvert.DeserializeObject<RootObject>(responseText);
		var id = responseJson.Data.Id;
		if (responseJson.Data.NextAction == null)
		{
			Debug.Log("No Next Action");
			return null;
		}

		var nextAction = responseJson.Data.NextAction.Payload.UserOpHash;

		Debug.Log("Next Action: " + nextAction);
		var intentResponse = await Openfort.SendSignatureTransactionIntentRequest(id, nextAction);
		var transactionHash = intentResponse.Response.TransactionHash;

		return transactionHash;
	}

	private Task SendWebRequestAsync(UnityWebRequest webRequest)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		webRequest.SendWebRequest().completed += _ =>
		{
			switch (webRequest.result)
			{
				case UnityWebRequest.Result.Success:
					tcs.SetResult(true);
					break;
				default:
					tcs.SetException(new Exception(webRequest.error));
					break;
			}
		};
		return tcs.Task;
	}

	public class RootObject
	{
		public TransactionIntentResponse Data { get; set; }
	}
}

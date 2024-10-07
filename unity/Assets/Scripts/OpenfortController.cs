using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Openfort.OpenfortSDK;
using Openfort.OpenfortSDK.Model;
using UnityEngine;
using UnityEngine.Networking;
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
		int chainId = 80002;
		ShieldAuthentication shieldConfig = new ShieldAuthentication(ShieldAuthType.Openfort, idToken, "firebase", "idToken" );
        EmbeddedSignerRequest request = new EmbeddedSignerRequest(chainId, shieldConfig);
		await Openfort.ConfigureEmbeddedSigner(request);
	}

	public async void AuthenticateWithOAuth(string idToken, string accessToken)
	{
		Debug.Log("Google Sign-In Success! Token: " + idToken);
		accessToken = accessToken;
		Debug.Log("Openfort Auth");
        if (OpenfortSDK.Instance != null)
        {
            Openfort = OpenfortSDK.Instance;
        }
		Openfort = await OpenfortSDK.Init(PublishableKey, ShieldKey, ShieldEncKey);
		ThirdPartyOAuthRequest request = new ThirdPartyOAuthRequest(ThirdPartyOAuthProvider.Firebase, idToken, TokenType.IdToken);
		await Openfort.AuthenticateWithThirdPartyProvider(request);
		await SetAutomaticRecoveryMethod(idToken);
	}

	public async UniTask<string> Mint()
	{
		if (string.IsNullOrEmpty(accessToken))
		{
			Debug.LogError($"mAccessToken is null or empty");
			return null;
		}

		var webRequest = UnityWebRequest.Post("https://openfort-auth-non-custodial.vercel.app/api/protected-collect", "");
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

        SignatureTransactionIntentRequest request = new SignatureTransactionIntentRequest(responseJson.transactionIntentId, responseJson.userOperationHash);
		TransactionIntentResponse intentResponse = await Openfort.SendSignatureTransactionIntentRequest(request);
		return intentResponse.Response.TransactionHash;
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
        public string transactionIntentId { get; set; }
        public string userOperationHash { get; set; }
    }
}

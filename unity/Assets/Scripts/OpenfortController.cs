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

	public async Task SetAutomaticRecoveryMethod()
	{

		try
		{
			int chainId = 80002;
			string accessToken;

			// Check if Openfort is initialized
			if (Openfort == null)
			{
				Debug.LogError("Openfort SDK is null - not initialized");
				throw new Exception("Openfort SDK not initialized");
			}

			Debug.Log("Getting access token from Openfort SDK...");
			try
			{
				accessToken = await Openfort.GetAccessToken();
				Debug.Log("Access token obtained: " + (string.IsNullOrEmpty(accessToken) ? "EMPTY" : "SUCCESS"));

				if (string.IsNullOrEmpty(accessToken))
				{
					Debug.LogError("Access token is null or empty");
					throw new Exception("Access token is null or empty");
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to get access token: " + e.Message);
				throw;
			}

			// Get encryption session from API
			// substitute with your backend endpoint
			var webRequest = UnityWebRequest.PostWwwForm("https://firebase-auth-embedded-wallet.vercel.app/api/protected-create-encryption-session", "");
			webRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
			webRequest.SetRequestHeader("Content-Type", "application/json");
			webRequest.SetRequestHeader("Accept", "application/json");

			Debug.Log("Sending web request...");
			await SendWebRequestAsync(webRequest);

			Debug.Log($"Web request completed. Result: {webRequest.result}, Response Code: {webRequest.responseCode}");

			if (webRequest.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Web request failed - Result: {webRequest.result}, Error: {webRequest.error}, Response Code: {webRequest.responseCode}");
				throw new Exception($"Cannot resolve destination host: {webRequest.error}");
			}

			var responseText = webRequest.downloadHandler.text;
			Debug.Log("Response received: " + responseText);

			var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);
			var encryptionSession = responseJson["session"];
			Debug.Log("Encryption session: " + encryptionSession);

			var recoveryParams = new AutomaticRecoveryParams(encryptionSession);

			ConfigureEmbeddedWalletRequest request = new ConfigureEmbeddedWalletRequest(
				recoveryParams: recoveryParams,
				chainId: chainId
			);

			Debug.Log("Configuring embedded wallet...");
			await Openfort.ConfigureEmbeddedWallet(request);
			Debug.Log("=== SetAutomaticRecoveryMethod completed successfully ===");
		}
		catch (Exception ex)
		{
			Debug.LogError($"=== SetAutomaticRecoveryMethod FAILED: {ex.Message} ===");
			Debug.LogError($"Stack trace: {ex.StackTrace}");
			throw;
		}
	}

	public async Task Init(Func<string, Task<string>> getThirdPartyToken)
	{
		if (OpenfortSDK.Instance != null)
		{
			Openfort = OpenfortSDK.Instance;
		}
		Openfort = await OpenfortSDK.Init(
			PublishableKey,
			ShieldKey,
			thirdPartyProvider: "firebase",
			getThirdPartyToken: getThirdPartyToken,
			iframeUrl: "https://development-iframe.vercel.app"
		);
	}

	public async UniTask<string> Mint()
	{
		if (string.IsNullOrEmpty(accessToken))
		{
			Debug.LogError($"mAccessToken is null or empty");
			return null;
		}
		// substitute with your backend endpoint
		var webRequest = UnityWebRequest.PostWwwForm("https://firebase-auth-embedded-wallet.vercel.app/api/protected-collect", "");
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

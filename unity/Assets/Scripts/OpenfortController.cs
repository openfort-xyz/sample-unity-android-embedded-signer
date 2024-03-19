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

public class OpenfortController : MonoBehaviour
{
    public static OpenfortController Instance { get; private set; }
    
    private const string PublishableKey = "YourPublishableKey";
    private OpenfortSDK mOpenfort;

    [HideInInspector] public string oauthAccessToken;
    
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
    
    public async void AuthenticateWithOAuth(string idToken)
	{
		Debug.Log("Google Sign-In Success! Token: " + idToken);
        
		Debug.Log("Openfort Auth");
		mOpenfort = new OpenfortSDK(PublishableKey); 
		oauthAccessToken = await mOpenfort.AuthenticateWithOAuth(OAuthProvider.Firebase, idToken);
		Debug.Log("Access Token: " + oauthAccessToken);

		try
		{
			mOpenfort.ConfigureEmbeddedSigner(80001);
		}
		catch (MissingRecoveryMethod)
		{
			await mOpenfort.ConfigureEmbeddedRecovery(new PasswordRecovery("secret"));
		}
	}
	
	public async UniTask<string> Mint()
	{
		if (string.IsNullOrEmpty(oauthAccessToken))
		{
			Debug.LogError($"mAccessToken is null or empty");
			return null;
		}
		
		var webRequest = UnityWebRequest.Post("https://localhost/mint", "");
		webRequest.SetRequestHeader("Authorization", "Bearer " + oauthAccessToken);
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
		var intentResponse = await mOpenfort.SendSignatureTransactionIntentRequest(id, nextAction);
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

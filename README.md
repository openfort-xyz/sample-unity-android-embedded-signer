## Overview

[Firebase](https://firebase.google.com/) is Google's all-encompassing app development platform, providing game developers with an array of backend tools powered by Google Cloud.

Within Unity, we've integrated the Firebase SDK to facilitate backend functionalities. Coupled with the [Google Play Games plugin for Unity](https://github.com/playgameservices/play-games-plugin-for-unity), it ensures a secure and efficient login for Android users.

The integration is further enhanced with the [Openfort Embedded Smart Accounts](https://www.openfort.xyz/blog/embedded-smart-accounts), which are created for every new Google Play Games user logged into Firebase. This enables Unity clients to directly leverage Openfort's blockchain capabilities, allowing for sophisticated blockchain interactions within the gaming environment.

## Application Workflow

<div align="center">
    <img
      width="100%"
      height="100%"
      src="https://blog-cms.openfort.xyz/uploads/firebase_embedded_integration_workflow_00eaa7dae2.png?updated_at=2024-03-20T10:34:17.422Z"
      alt='Openfort Firebase Embedded workflow'
    />
</div>

## Prerequisites

- Sign in to [dashboard.openfort.xyz](http://dashboard.openfort.xyz) and create a new project.
- You need a [Google Play Developer account](https://support.google.com/googleplay/android-developer/answer/6112435?hl=en).
- You need a [Google Cloud project](https://developers.google.com/workspace/guides/create-project).
- Run the [Express backend](https://github.com/openfort-xyz/sample-unity-android-embedded-signer/tree/main/backend) locally.
- Clone or download the repository and open it with Unity [2021.3](https://unity.com/releases/editor/qa/lts-releases?version=2021.3).
  When opening the project, select ***Ignore*** on this popup:

  ![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_03a708cc91.png?updated_at=2023-11-07T18:41:24.181Z)

  Once opened, you will see some reference errors. We will solve this in the next step by importing the Firebase SDK.
- Follow the [Firebase-Unity setup guide](https://firebase.google.com/docs/unity/setup?hl=es-419).
  On [step 4](https://firebase.google.com/docs/unity/setup?hl=es-419#add-sdks), you just need to import ***FirebaseAuth*** and ***FirebaseFirestore*** packages:

  ![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_1_7d8a33fb8d.png?updated_at=2023-11-07T18:41:24.676Z)

  Do it one by one and disable ***ExternalDependencyManager*** folder before importing:

  ![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_2_352fc42325.png?updated_at=2023-11-07T18:41:37.181Z)

  Most reference errors should be solved by now. If `UnityEditor.iOS.Extensions.Xcode` error is still standing, select ***Firebase.Editor*** asset, disable ***Validate References*** and choose ***Apply***:

  ![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_3_9ea2133564.png?updated_at=2023-11-07T18:41:46.887Z)
- Create a keystore
  Follow this [guide](https://docs.unity3d.com/Manual/android-keystore-create.html) to create a new keystore for the Unity project.
- Find SHA1 certificate fingerprint
  You need to extract the certificate fingerprint from the created keystore. Follow this [video tutorial](https://www.youtube.com/watch?v=lDXE4lfM0aQ) on how to do it, it also covers the creation of the keystore.

  This is the command that you will need to run:
  ```shell
  keytool -list -v -keystore "path/to/your/keystore" -alias "your_key_alias"
  ```

## Set up Firebase

### Add Google sign-in provider

Go to the [Firebase console](https://console.firebase.google.com/?hl=es-419), select your project and select ***Authentication***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_4_e8b0d867f8.png?updated_at=2023-11-07T18:41:47.295Z)

Select ***Get started***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_5_140cfd6f28.png?updated_at=2023-11-07T18:41:35.185Z)

Select ***Google*** as a sign-in provider:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_6_eacbe66a92.png?updated_at=2023-11-07T18:41:38.487Z)

Activate ***Enable*** toggle, choose a public-facing name and select ***Save***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_7_4b75cbc33e.png?updated_at=2023-11-07T18:41:40.678Z)

A popup will appear. Copy the ***Web client ID*** and the ***Web client secret*** somewhere safe and choose ***Done***. You will see your Google provider enabled:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_8_7d2b07af1c.png?updated_at=2023-11-07T18:41:29.178Z)

Select the provider and choose ***Project Settings***. Under ***Your apps*** section select ***Add fingerprint*** and add your SHA1 certificate fingerprint. Then choose ***Save***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_26_0877bd3b91.png?updated_at=2023-11-07T18:41:44.183Z)

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_27_9505073e7c.png?updated_at=2023-11-07T18:41:29.584Z)

### Add Google Play sign-in provider

Select ***Add new provider*** and choose ***Google Play***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_9_7c803a1a83.png?updated_at=2023-11-07T18:41:36.279Z)

Activate ***Enable*** toggle, enter the credentials you just saved and choose ***Save***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_10_3cd041e888.png?updated_at=2023-11-07T18:41:33.579Z)

Both ***Google*** and ***Google Play*** sign-in providers are ready:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_11_0a946e50ff.png?updated_at=2023-11-07T18:41:31.578Z)

## Set up Openfort

### [Set up Firebase provider](https://dashboard.openfort.xyz/players/auth/providers)

Enable Firebase, add your project ID and choose ***Save***:

![Alt text](https://blog-cms.openfort.xyz/uploads/unity_firebase_android_embedded_4be4bc7920.png?updated_at=2024-03-19T11:22:43.865Z)

### [Add a Contract](https://dashboard.openfort.xyz/assets/new)

Choose ***Add contract***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_45_dff8dbd5d9.png?updated_at=2023-11-09T04:23:43.095Z)

This sample requires a contract to run. We use [0x38090d1636069c0ff1Af6bc1737Fb996B7f63AC0](https://mumbai.polygonscan.com/address/0x38090d1636069c0ff1Af6bc1737Fb996B7f63AC0) (NFT contract deployed in 80001 Mumbai). You can use the same for this tutorial:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_46_05a8645885.png?updated_at=2023-11-09T04:25:24.000Z) 

### [Add a Policy](https://dashboard.openfort.xyz/policies/new)

Choose ***Add policy***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_47_e043788f2d.png?updated_at=2023-11-09T04:27:17.395Z)

We aim to cover gas fees for users. Set a new gas policy:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_48_3cd0914ae9.png?updated_at=2023-11-09T04:31:49.793Z)

Now, add a rule so our contract uses this policy:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_49_74f55004ba.png?updated_at=2023-11-09T04:31:50.888Z)

## Set up Google Play

> **Reminder:** Use the same Google account you used for setting up your Firebase app.

### Create a new app
Go to [Play Console](https://play.google.com/console) and create a new app. Enter app details (it's important you select ***Game***), confirm policies and select ***Create app***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_12_0487318190.png?updated_at=2023-11-07T18:41:44.580Z)

Under ***Grow --> Play Games Services --> Setup and management --> Configuration***, select ***Create new Play Games Services project*** and choose your Firebase project as the cloud project. Then select ***Use***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_13_5ad11592ff.png?updated_at=2023-11-07T18:41:47.787Z)

### Add credentials

#### Add Android OAuth client credential

Under ***Credentials*** section choose ***Add credential***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_14_7e4b6cb5d1.png?updated_at=2023-11-07T18:41:46.982Z)

Select ***Android***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_15_a46a653f09.png?updated_at=2023-11-07T18:41:46.784Z)

Scroll down and select ***Create OAuth client***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_16_27ab5da789.png?updated_at=2023-11-07T18:41:33.186Z)

Choose ***Create OAuth Client ID***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_17_c6d544e6c4.png?updated_at=2023-11-07T18:41:31.682Z)

This will open the Google Cloud console. Now select ***Android*** as *Application type*, enter a *Name* and fill the *Package name* with the **Unity app package name** (found in the Android Platform Player Settings):

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_19_72b69ccb8c.png?updated_at=2023-11-07T18:41:46.678Z)

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_20_e3286e880c.png?updated_at=2023-11-07T18:41:47.076Z)

Enter your SHA1 certificate fingerprint and choose ***CREATE***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_21_b1c2c44246.png?updated_at=2023-11-07T18:41:34.781Z)

Now you can download the JSON and choose ***OK***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_22_ee531ceeba.png?updated_at=2023-11-07T18:41:36.781Z)

Go back to the Google Play console, select ***Done*** and choose your newly created Android OAuth client. Then select ***Save changes***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_23_5ff3dc73de.png?updated_at=2023-11-07T18:41:44.877Z)

#### Add Game server/Web OAuth client credential

Go back to ***Configuration*** and select ***Add credential***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_24_b44fc158b9.png?updated_at=2023-11-07T18:41:44.287Z)

Choose ***Game server***, refresh OAuth clients, select ***Web client (auto created by Google Service)*** (it was created automatically during [this process](https://github.com/openfort-xyz/sample-unity-android-embedded-signer/tree/main?tab=readme-ov-file#add-google-sign-in-provider)) and select ***Save changes***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_25_1f76c7f29a.png?updated_at=2023-11-07T18:41:45.679Z)

Finally copy the ***OAuth client ID***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_28_ecba674180.png?updated_at=2023-11-07T18:41:30.082Z)

## Set up Unity project

> **Reminder:** Make sure ***Android*** is selected as a platform in ***Build settings***. 

### Set up Google Play Games

Go to ***Window --> Google Play Games --> Setup --> Android setup***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_30_818b8cbf6d.png?updated_at=2023-11-07T18:41:35.884Z)

Paste the ***Game server OAuth client ID*** you just copied under ***Client ID***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_31_421e75f3b2.png?updated_at=2023-11-07T18:41:32.077Z)

Go to the [Google Play console](https://play.google.com/console) and on your app's configuration select ***Get resources***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_32_52815075fc.png?updated_at=2023-11-07T18:41:44.377Z)

Copy the Android (XML):

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_33_6690691229.png?updated_at=2023-11-07T18:41:38.876Z)

In Unity, paste it in ***Resources Definition*** and then select ***Setup***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_35_b08e422040.png?updated_at=2023-11-07T18:41:46.888Z)

### Add Firebase configuration ``.json``

Finally, go to the [Firebase console](https://console.firebase.google.com/?hl=es-419) and under your app configuration, download the ***google-services.json***:

![Alt text](https://blog-cms.openfort.xyz/uploads/firebase_extension_unity_img_36_5048e220db.png?updated_at=2023-11-07T18:41:46.980Z)

Import it in your Unity project ***Assets*** folder to make sure every credential is up to date.

### Add Openfort Publishable Key

Add your publishable key [here](https://github.com/openfort-xyz/sample-unity-android-embedded-signer/blob/f370c01fe90fafa79ead1fcdf01c93a5212f5fde/unity/Assets/Scripts/OpenfortController.cs#L17).

### Add Express backend URL

Add your Express backend minting URL [here](https://github.com/openfort-xyz/sample-unity-android-embedded-signer/blob/f370c01fe90fafa79ead1fcdf01c93a5212f5fde/unity/Assets/Scripts/OpenfortController.cs#L62).

## Test on Android

Upon building and running the game on an Android device, the registration/login process is automated via Google Play Games, resulting in a streamlined user experience.

## Conclusion

Upon completing the above steps, your Unity game will be fully integrated with Openfort Embedded Smart Accounts and Firebase and Google Play Games. Always remember to test every feature before deploying to guarantee a flawless player experience.

## Get support
If you found a bug or want to suggest a new [feature/use case/sample], please [file an issue](../../issues).

If you have questions, comments, or need help with code, we're here to help:
- on Twitter at https://twitter.com/openfortxyz
- on Discord: https://discord.com/invite/t7x7hwkJF4
- by email: support+youtube@openfort.xyz

# Build an AI Powered AR Character in Unity with AR Foundation

*This pattern was originally published using ARKit and only available on iOS. With Unity introducing AR Foundation, this pattern can now run on either ARKit or ARCore depending on what device you build for.*

In this Code Pattern we will use [Assistant](https://www.ibm.com/watson/developercloud/conversation.html), [Speech-to-Text](https://www.ibm.com/watson/developercloud/speech-to-text.html), and [Text-to-Speech](https://www.ibm.com/watson/developercloud/text-to-speech.html) deployed to an iPhone or an Android phone, using either ARKit or ARCore respectively, to have a voice-powered animated avatar in Unity.

Augmented reality allows a lower barrier to entry for both developers and end-users thanks to framework compatibility in phones and digital eyewear. Unity's AR Foundation continues to lower the barrier for developers, allowing a single source code for a Unity project to take advantage of ARKit and ARCore. 

For more information about AR Foundation, take a look at [Unity's blog](https://blogs.unity3d.com/2018/12/18/unitys-handheld-ar-ecosystem-ar-foundation-arcore-and-arkit/).

When the reader has completed this Code Pattern, they will understand how to:

* Add IBM Watson Speech-to-Text, Assistant, and Text-to-Speech to Unity with AR Foundation to create an augmented reality experience.

!["diagram"](doc/source/images/architecture.png)

## Flow

1. User interacts in augmented reality and gives voice commands such as "Walk Forward".
2. The phone microphone picks up the voice command and the running application sends it to Watson Speech-to-Text.
3. Watson Speech-to-Text converts the audio to text and returns it to the running application on the phone.
4. The application sends the text to Watson Assistant. Watson assistant returns the recognized intent "Forward". The intent triggers an animation state event change.
5. The application sends the response from Watson Assistant to Watson Text-to-Speech.
6. Watson Text-to-Speech converts the text to audio and returns it to the running application on the phone.
7. The application plays the audio response and waits for the next voice command.

<!--
# Watch the Video
TODO: MAKE VIDEO
-->

## Included components

* [IBM Watson Assistant](https://www.ibm.com/watson/developercloud/conversation.html): Create a chatbot with a program that conducts a conversation via auditory or textual methods.
* [IBM Watson Speech-to-Text](https://www.ibm.com/watson/developercloud/speech-to-text.html): Converts audio voice into written text.
* [IBM Watson Text-to-Speech](https://www.ibm.com/watson/developercloud/speech-to-text.html): Converts written text into audio.

## Featured technologies

* [Unity](https://unity3d.com/): A cross-platform game engine used to develop video games for PC, consoles, mobile devices and websites.
* [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.0/manual/index.html): A Unity package for AR functionality to create augmented reality experiences.

# Steps

1. [Before you begin](#1-before-you-begin)
2. [Create IBM Cloud services](#2-create-ibm-cloud-services)
3. [Building and Running](#3-building-and-running)

## 1. Before You Begin

* [IBM Cloud Account](http://ibm.biz/Bdimr6)
* [Unity](https://unity3d.com/get-unity/download)

## 2. Create IBM Cloud services

On your local machine:
1. `git clone https://github.com/IBM/Watson-Unity-ARKit.git`
2. `cd Watson-Unity-ARKit`

In [IBM Cloud](https://cloud.ibm.com/):

1. Create a [Speech-To-Text](https://cloud.ibm.com/catalog/speech-to-text/) service instance.
2. Create a [Text-to-Speech](https://cloud.ibm.com/catalog/text-to-speech/) service instance.
3. Create an [Assistant](https://cloud.ibm.com/catalog/services/conversation/) service instance.
4. Once you see the services in the Dashboard, select the Assistant service you created and click the `Launch Tool`. 
    !["Launch Tool Button"](doc/source/images/workspace_launcher2.png?raw=true)
5. After logging into the Assistant Tool, click `Create a Skill`.
    !["Create a Skill Button"](doc/source/images/create_a_skill.png?raw=true)
6. Click `Create skill` button.
    !["Create skill button](doc/source/images/create_skill.png?raw=true)
7. Click "Import skill".
8. Import the Assistant [`voiceActivatedMotionSimple.json`](data/voiceActivatedMotionSimple.json) file located in your clone of this repository.
9. Once the skill has been created, we'll need to add it to an Assistant. If you have opend your skill, back out of it. Click `Assistants`.
10. Click `Create Assistant`.
11. Name your assistant, click `Create assistant`.
12. Click `Add dialog skill` to add the skill you just imported to this Assistant.
13. Click the `...` menu in the top and click "Settings" to see the Assistant Settings.
    !["Assistant Skills menu"](doc/source/images/assistant_settings.png?raw=true)
14. Click `API Details` and find your Assistant Id. You will need this in the next section.

## 3. Building and Running

> Note: This has been compiled and tested using Unity 2018.3.0f2 and Watson SDK for Unity 3.1.0 (2019-04-09) & Unity Core SDK 0.2.0 (2019-04-09). 

> Note: If you are in *any* IBM Cloud region other than US-South/Dallas you *must* use Unity 2018.2 or higher. This is because Unity 2018.2 or higher is needed for TLS 1.2, which is the only TLS version available in all regions other than US-South.

The directories for unity-sdk and unity-sdk-core are blank within the Assets directory, placeholders for where the SDKs should be. Either delete these blank directories or move the contents of the SDKs into the directories after the following commands.

1. Download the [Watson SDK for Unity](https://github.com/watson-developer-cloud/unity-sdk) or perform the following:

`git clone https://github.com/watson-developer-cloud/unity-sdk.git`

Make sure you are on the 3.1.0 tagged branch.

2. Download the [Unity Core SDK](https://github.com/IBM/unity-sdk-core) or perform the following:

`git clone https://github.com/IBM/unity-sdk-core.git`

Make sure you are on the 0.2.0 tagged branch.

3. Open Unity and inside the project launcher select the ![Open](doc/source/images/unity_open.png?raw=true) button.
4. If prompted to upgrade the project to a newer Unity version, do so.
5. Follow [these instructions](https://github.com/watson-developer-cloud/unity-sdk#getting-the-watson-sdk-and-adding-it-to-unity) to add the Watson SDK for Unity downloaded in step 1 to the project.
6. Follow [these instructions](https://github.com/watson-developer-cloud/unity-sdk#configuring-your-service-credentials) to create your Speech To Text, Text to Speech, and Watson Assistant services and find your credentials using [IBM Cloud](https://cloud.ibm.com)

**Please note, the following instructions include scene changes and game objects have been added or replaced for AR Foundation.**

7. In the Unity Hierarchy view, click to expand under `AR Default Plane`, click `DefaultAvatar`. If you are not in the Main scene, click `Scenes` and `Main` in your Project window, then find the game objects listed above. 
8. In the Inspector you will see Variables for `Speech To Text`, `Text to Speech`, and `Assistant`. If you are using US-South or Dallas, you can leave the `Assistant URL`, `Speech to Text URL`, and `Text To Speech URL` blank, taking on the default value as shown in the WatsonLogic.cs file. If not, please provide the URL values listed on the Manage page for each service in IBM Cloud.
9. Fill out the `Assistant Id`, `Assistant IAM Apikey`, `Speech to Text Iam Apikey`, `Text to Speech Iam Apikey`. All Iam Apikey values are your API key or token, listed under the URL on the Manage page for each service.  

!["Unity Editor enter credentials"](doc/source/images/UnityEditorUpdated.png?raw=true)

### Building for iOS
Build steps for iOS have been tested with iOS 11+ and Xcode 10.2.1.

1. To Build for iOS and deploy to your phone, you can _File_ -> _Build_ Settings (Ctrl + Shift +B) and click Build.
2. When prompted you can name your build. 
3. When the build is completed, open the project in Xcode by clicking on `Unity-iPhone.xcodeproj`.
4. Follow [steps](https://help.apple.com/xcode/mac/current/#/dev60b6fbbc7) to sign your app. Note - you must have an Apple Developer Account.
5. Connect your phone via USB and select it from the target device list at the top of Xcode. Click the play button to run it.
6. Alternately, connect the phone via USB and _File_-> _Build and Run_ (or Ctrl+B).

### Building for Android
Build steps for Android have been tested with Pie on a Pixel 2 device with Android Studio 3.4.1.

1. To Build for Android and deploy to your phone, you can _File_ -> _Build_ Settings (Ctrl + Shift +B) and click Switch Platform.
2. The project will reload in Unity. When done, click Build.
3. When prompted you can name your build.
4. When the build is completed, install the APK on your emulator or device.
5. Open the app to run.

  
# Links

<!--* TODO ADD VIDEO LINK-->
* [Watson Unity SDK](https://github.com/IBM/unity-sdk)
* [Unity Core SDK](https://github.com/IBM/unity-sdk-core)

# Troubleshooting

AR features are only available on iOS 11+ and can not run on an emulator/simulator. Be sure to check your player settings to target minimum iOS device of 11, and your Xcode deployment target (under deployment info) to be 11 also.

In order to run the app you will need to sign it. Follow steps [here](https://help.apple.com/xcode/mac/current/#/dev60b6fbbc7).

Mojave updates may adjust security settings and block microphone access in Unity. If Watson Speech to Text appears to be in a ready and listening state but not hearing audio, make sure to check your security settings for microphone permissions. For more information: https://support.apple.com/en-us/HT209175.

You may need the [ARCore APK](https://github.com/google-ar/arcore-android-sdk/releases) for your Android emulator. This pattern has been tested with ARCore SDK v1.9.0 on a Pixel 2 device running Pie.

# Learn more

* **Artificial Intelligence Code Patterns**: Enjoyed this Code Pattern? Check out our other [AI Code Patterns](https://developer.ibm.com/code/technologies/artificial-intelligence/).
* **AI and Data Code Pattern Playlist**: Bookmark our [playlist](https://www.youtube.com/playlist?list=PLzUbsvIyrNfknNewObx5N7uGZ5FKH0Fde) with all of our Code Pattern videos
* **With Watson**: Want to take your Watson app to the next level? Looking to utilize Watson Brand assets? [Join the With Watson program](https://www.ibm.com/watson/with-watson/) to leverage exclusive brand, marketing, and tech resources to amplify and accelerate your Watson embedded commercial solution.

# License

This code pattern is licensed under the Apache Software License, Version 2.  Separate third party code objects invoked within this code pattern are licensed by their respective providers pursuant to their own separate licenses. Contributions are subject to the [Developer Certificate of Origin, Version 1.1 (DCO)](https://developercertificate.org/) and the [Apache Software License, Version 2](http://www.apache.org/licenses/LICENSE-2.0.txt).

[Apache Software License (ASL) FAQ](http://www.apache.org/foundation/license-faq.html#WhatDoesItMEAN)

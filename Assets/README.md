
# Yoooga lab plugin

This system is in alpha mode, pending changes per usage. Its main goal is to encapsulate ads / analytics and events in a self contained enviroment and linked to the game through the YoogoLabManager wrapper class. 

## Initial Setup

- Navigate to Project Settings-> Script execution order and set the PluginSDKManager as the highest loading priority above all else.

- Open BootStraper scene and expand YoogoLabManager prefab.

- Inside Ads/Adjust and Revenue, input the ios/android tokens per game.

- You can always directly edit it from the YoogalabPlugin folder.

- Goto Assets->Google mobile ads->Settngs. open it and on the inspector window add the app ids per platform. 

- Similarly goto Applovin->Integration Manager and open it. on the window scroll to find "Mediated Networks", in there also set admob app ids per platform into their spot on the Google Ads Manager network.

- Navigate to Project Settings -> Mobile Notifications and toggle the IOS tab, uncheck "Request Authorization on App Launch" and make sure Enable Push Notifications is on!

- Navigate to In-App Purchasing and link the project, in case the key is missing simply add it!

- For usage with Android open the Manifest file located at : Assets->Plugins->Android and for production only set the android:debuggale to false. as well as change the app ids to the current ones used by the game.

- Similarly in the same folder there is a GoogleMobileAdsPlugin.androidlib file, open it and inside find the manifest and also change the app ids. IF not there or empty look at the Potential errors section below and once resolved perform the changes again!

- Add google plist and json for firebase usage as well as the keystore for Android releases.

- if on Android use the Resolver inside Assets-> External Dependency Manager -> Android Resolver once all data have been updated , googleservices added etc etc.

- For build make sure min SDK is 23 as google mobile ads require it.

---------------
---------------
## Important Notes

- In the settings of google mobile ads make sure User Tracking Usage Description has values Otherwise ios ATT will not fire!!


        "This identifier will be used to deliver personalized ads to you."


- On ios on my end i ended up using 2022.3.10 as 0.8 threw issues on the gradle and android core, if the same needs to be done, you must also change the version inside the manifests.

- While testing we must make our device id into a test device in Applovin so as to get demo ads and not live ones and risk banning due to ad overuse!

- If you are adding products through code, you don't need to manually configure anything else for IAP.
  - However, if you are not adding products via code, you must manually add them through Services → In-App Purchasing → IAP Catalog in the Unity Editor.
---------------
---------------


## Potential Errors
- In case the post install class fails, duplicates will be created on YoogoLabManager wrapper class, simply delete the native per game leaving only the one placed inside Assets/YoogalabPlugin

- I have attached a packages json file for getting the packages we need but in case it fails manualy install :
    - In app Purchasing  
    - ios 14 Advertising Support 
    - Mobile Notifications

- After porting the plugin there is a high chance google mobile ads fails to include GoogleMobileAdsPlugin.androidlib and it will be empty, in such case (double click to open it and see), i have included a zip cotnaining it as it was on the plugin Project. simply delete the empty one, open the project in the windows manager/Finder and extract the zip into the same location, open it to verify manifests are generated!

- In case of colliding atributes with unity version validate unity version in the Manifests (Android) and min max sdk version. min expexted is 23 !


## Usage

- Drop the bootstrapper scene in the Hierarchy, after perfoming the setup depending on the games loading system find Bootstrapper go and toggle the "Can Change Scene" flag if the game does NOT load any data prior to the game scene as well as set the string to the game scene (e.g., "Game", "MainScene").

- If using custom loading screen simply copy and paste the yoogaLabPlugin go , Bootstraper and bootstrapper canvas into your own loadin scene!

- On the occasion that the gamme uses its own loader class prior to the game scene, the "Can Change Scene" should be toggled off. As per general instructions the custom loader should wait for the SDK to finish loading at least the Remoteconfig so it can pull values at once when the game starts through the use of "TryStartGame" action.

- The bootstraper has a timeout value in seconds for the max amount of time it should wait before loading the scene when being used as a standalone scene manager, as well as min splash duration for the splash screen as its loading the scene in an additive  mode thus blending in the game flow.

- There also is a plugin tester scene which can be used per discretion! its not updated based on the rest of the plugin but feel free to mess with it!

- The system also contains two flags on the SDKManager. one for sandbox mode which should be used while testing and building and one for logs. All classes have logs where needed, when the flag is off it wont log them in the console. So while building sandbox and log = true!


---
#### Any updates or suggestions are always welcomed! And in case of issues do ping me!
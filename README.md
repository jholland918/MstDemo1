# Master Server Toolkit Demo

## MST9 Authorization/Authentication
Docs: https://master-toolkit.com/en/demos/basic-authorization/
1. Open Client Scene
2. Under MasterCanvas, drag the following auth view prefabs into it:
   * These prefabs are under \Assets\MasterServerToolkit\Bridges\Shared\Prefabs\UI :
     * SignupView
     * SigninView
     * PasswordResetView
     * PasswordResetCodeView
     * EmailConfirmationView
3. Remove SimpleSignInView from MasterCanvas
4. Wire stuff up in the hierarchy in Client scene
	*  Drag `--CONNECTION_TO_MASTER` to `--CLIENT_MANAGER > Auth Behavior > Client To Master Connector`
	*  Drag `MenuView` to `--CLIENT_MANAGER > Auth Behavior > On Email Confirmed Event` and assign UIView.Show as the handler.
5. Setup email
	*  Open Master scene, go to `--MASTER_SERVER > Mailers > BaseSmtp` game object and configure Smtp Mailer component:
	    *  Sign up with an email service (brevo.com).
		*  Go to Brevo > Transactional > Email > Real time
		*  Use the Brevo SMTP server, port, login, and password in the Smtp Mailer component settings
6.  Handle successful EmailConfirmationView event
	*  When the email confirmation is successfull it'll simply hide the view. To make sure the user is not staring at a blank screen, add a handler to `Client Scene/MasterCanvas/EmailConfirmationView/Email Confirmation View component:On Hide Event`. Drag the MenuView from the hierarchy to the On Hide Event and select UIView.Show as the handler.
7.  Secure SMTP passwords
    *  In Master scene, select BaseSmtp in the hierarchy and reset the Smtp Mailer component
	*  Add back the Email Body Template of `EmailMessageForm` to the Smtp Mailer component and maybe the Sender Display Name as well.
	*  Update build scripts (`WinAppBuilder.cs` etc.) to create configs with the following settings:
```
// Email/Smtp Options
properties.Add(Mst.Args.Names.SmtpHost, Environment.GetEnvironmentVariable("MST_SMTP_HOST", EnvironmentVariableTarget.User));
properties.Add(Mst.Args.Names.SmtpUsername, Environment.GetEnvironmentVariable("MST_SMTP_USERNAME", EnvironmentVariableTarget.User));
properties.Add(Mst.Args.Names.SmtpPassword, Environment.GetEnvironmentVariable("MST_SMTP_PASSWORD", EnvironmentVariableTarget.User));
properties.Add(Mst.Args.Names.SmtpPort, Environment.GetEnvironmentVariable("MST_SMTP_PORT", EnvironmentVariableTarget.User));
properties.Add(Mst.Args.Names.SmtpMailFrom, Environment.GetEnvironmentVariable("MST_SMTP_MAIL_FROM", EnvironmentVariableTarget.User));
properties.Add(Mst.Args.Names.SmtpSenderDisplayName, Environment.GetEnvironmentVariable("MST_SMTP_SENDER_DISPLAY_NAME", EnvironmentVariableTarget.User));
```

## MST8 Room Scene Changes
### Persisted/Destroyed Objects when room loads

*  Persisted GameObjects
    *  --ROOM_CLIENT
    *  --CONNECTION_TO_MASTER
    *  --ROOM_SERVER
*  Persisted Objects
    *  RoomClientManager
    *  ClientToMasterConnector
    *  RoomNetworkManager
    *  RoomServerManager
*  Destroyed GameObjects:
    *  Main Camera
    *  Directional Light
    *  MasterCanvas
    *  --TERMINAL
    *  EventSystem
    *  --PLAYER_SPAWNER
    *  --GAME_MANAGER
    *  Arena
*  Destroyed Objects:
    *  Terminal
    *  PlayerSpawner
    *  GameManager

### Steps for implementing room scene changes
0.  Create a Room folder inside Assets/App/Prefabs
1.  Create several prefabs by dropping them inside the new folder: App/Prefabs/Room
	*  Main Camera
	*  Directional Light
	*  EventSystem
    *  MasterCanvas/RoomHudView
    *  MasterCanvas
	*  --PLAYER_SPAWNER
	*  --GAME_MANAGER
    *  --TERMINAL
2.  Duplicate Room scene and rename new scene as RoomFoo
3.  In Room scene, delete all objects from hierarchy except for
    *  --CONNECTION_TO_MASTER
    *  --ROOM_SERVER
    *  --ROOM_CLIENT
4.  In Room scene, remove old, invalid RoomServerManager event handlers that used to belong to the GameManager.
5.  In RoomFoo scene, delete the following objects from hierarchy:
    *  --CONNECTION_TO_MASTER
    *  --ROOM_SERVER
    *  --ROOM_CLIENT
6.  Edit GameManager to fix event listeners:
    *  In Awake(), add _roomServerManager = FindFirstObjectByType<RoomServerManager>();
	*  For handlers RoomServerManager_OnPlayerJoinedRoom & RoomServerManager_OnPlayerLeftRoom
	   *  Manually add listeners: 
	      *  _roomServerManager.OnPlayerJoinedRoomEvent.AddListener(RoomServerManager_OnPlayerJoinedRoom);
		  *  _roomServerManager.OnPlayerLeftRoomEvent.AddListener(RoomServerManager_OnPlayerLeftRoom);
    *  For handler RoomServerManager_OnBeforeRoomRegister
	   *  Delete hander and just assign _roomOptions = _roomServerManager.RoomOptions;
    *  For handler RoomServerManager_OnRoomRegistered
	   *  Assign _roomController = _roomServerManager.RoomController;
	   *  Rename handler to OnFoundRoomServerManager() and call it from Awake(), put all that extra wire up logic inside it.
    *  Update PlayerRegistrationCollection logic and affected game handlers...
7.  Add new RoomFoo scene to build in App/Editor/WinAppBuilder.cs
8.  Add call to _lobby.SetLobbyProperty(Mst.Args.Names.RoomOnlineScene, "RoomFoo", (isSuccessful, error) => ...) in LobbyView.cs - Then test it!
9.  Create a new RoomQux scene from scratch
    *  Add the same prefabs as RoomFoo
	*  Add the game manager reference to the RoomHudView object
	*  Add a floor and a couple of boxes or whatever for an arena
	*  Add RoomQux to the WinAppBuilder.cs and LobbyView.cs, then test it!
10.  Add room/level select dropdown in LobbyView.


## MST7 Game Types
1.  Add /Assets/App/Scripts/Character/PlayerCharacterShoot.cs
2.  Add /Assets/App/Scripts/Character/PredictedProjectile.cs
3.  Attach PlayerCharacterShoot.cs to FpsCharacter prefab
4.  Create Bullet prefab and attach PredictedProjectile.cs
5.  Add reference to Bullet prefab on FpsCharacter > PlayerCharacterShoot > Projectile ref
6.  Add Player Health
	*   For Room//MasterCanvas/RoomHudView, unpack RoomHudView prefab so MST upgrades will not affect it
	*   Add "HealthText" to RoomHudView: Right-click RoomHudView > UI > Text - TextMeshPro, named "HealthText"
	*   HealthText Setup
        *   Rect Transform > Anchor > Top Left
        *   Rect Transform > Pivot > x 0 y 1
        *   Rect Transform > Pos X 20, Pos Y -90
		https://youtu.be/hQyBnHofCKg?si=Mj7DfUMpfiFORJaZ
	*   Attach PlayerCharacterVitals.cs to FpsCharacter prefab
        *   Reference the Character Controller on the prefab to the property of the same name to the PlayerCharacterVitals.cs
        *   Ensure custom code is added to script...
	*   Update PredictedProjectile > OnCollisionEnter() to add damage to PlayerCharacterVitals...
	    *   Note: The `PlayerCharacterVitals playerVitals = collision.gameObject.GetComponent<PlayerCharacterVitals>();` needs to use GetComponentInParent<>() since the collision will be happening on a child gameObject of "Body"
	*   Add rigidbody to FpsCharacter prefab
	    *   Open FpsCharacter prefab
	    *   Add rigidbody to child gameobject "Body"
		    *   Freeze XYZ Position & Rotation
		    *   Uncheck "Use Gravity"
7.  Make RoomHudView.cs (in Room//MasterCanvas) our own script so we can customize it.
	*   Copy Assets/MasterServerToolkit/Demos/BasicRoomsAndLobbies/Scripts/RoomHudView.cs to Assets/App/Scripts/RoomHudView.cs
	*   Update guid refs in Room.unity scene
	    OLD: 1b0d71f0b05837841878ef1150897d70
		NEW: 3101137a69689e8448641bc5f343daee (changes each time it's copied btw)
	*   Change namespace to `Assets.App.Scripts`
	*   Add HealthText ref from hierarchy to script property in inspector
8.  Create Die Effect Prefab
	*   Open Sample Scene
	*   Create new game object, name it "DieEffect"
	*   Add new cylinder, Scale x 0.1, y 2.0, z 0.1
	*   Change cylinder Mesh Renderer > Material: Orange
	    *   Duplicate the cylinder three times so there is 4 of them
	    *   Rotate them around to make an asterisk looking shape
		*   Place the 4 cylinders into a "Group1" container and duplicate the container
		*   Rotate the duplicated container to make an "asterisk ball" look.
		*   Make the Transform Postion: x 0, y 1, z 0
	*   Drag the "DieEffect" into the /App/Prefabs directory, then delete it from the SampleScene
	*   Add DieEffect prefab to FpsCharacter prefab > Player Character Vitals > Die Effect Prefab
	*   Fix FpsCharacter refs inside App/Data/Demo SinglePrefabsObjects & Room/--PLAYER_SPAWNER...
8.  Create GameManager script inside App/Scripts/GameManagement
	*   Copy logic from MatchTimer into GameManager and delete MatchTimer
	*   Create new --GAME_MANAGER object in Hierarchy and attach GameManager.cs to it
	*   Copy remaining GameManagement scripts
	*   Add GameManager ref to RoomHudView
	*   Add GameResultsText to RoomHudView and populate OnGameResults...
	    *    Rect Transform > Width 600, Height 100
		*    TextMeshPro > Font Size 96
		*    Add reference to gameresultstext to parent roomhudview
9.  Support team games
    *   Add changeTeamButton to Client/MasterCanvas/LobbyView.
	*   Change Buttons > Layout Element > Preferred Width to 200 for all buttons in row
	*   Update Button Text to "Change Team"
	*   Wire up button to OnChangeTeamClick click handler
	*   Make dropdown options for gametype bigger: CreateNewLobbyView/panel/container/gameTypeInputDropdown
	    *    Change to 45 for the following:
			 Dropdown > Template > Viewport > Content : Height 45
			 Dropdown > Template > Viewport > Content > Item : Height 45
			 Dropdown > Template > Viewport > Content > Item > Item Label : Font size 48
	    *    See also https://forum.unity.com/threads/changing-size-of-dropdown-in-unity.412977/
10. Indicate team in-game...
    *   For the FpsCharacter prefab, follow https://www.youtube.com/watch?v=aPJVhLVEexY to 9:50, then...
	*   Attach LookAtCamera.cs to FpsPrefab > Canvas.
    *   Set the player name using `PlayerNameTracker.SetName(playerName);` in GameManager > PlayerCharacter . OnLocalCharacterSpawnedEvent :: PlayerNameTracker.SetName(Mst.Client.Auth.AccountInfo.Username);
    *   Attach the PlayerNameTracker.cs script to Room/--GAME_MANAGER
    *   ...
	

## Reference Cleanup (WIP)

1.  Room Scene References
	*   Room//Arena object is a prefab that's outside of the /Assets/App folder. 
        *   Might not need to copy this one over unless you want to keep it in your game and make edits to it. Just know its still in MST.
	*   Room//--ROOM_SERVER/RoomNetworkManager 
        *   This is referencing MST/Bridges/FishNet/BasicRoomsAndLobbies/Scripts/Room/RoomNetworkManager.cs
        *   This seems more like a framework-level script that users won't have to edit. So to make things less confusing, delete the copied RoomNetworkManager.cs inside /App/Scripts/Room... to remove confusion. 
	*   Room//--ROOM_CLIENT/RoomClientManager 
        *   This is referencing the script inside MST/Bridges/FishNet/BasicRoomsAndLobbies/Scripts/Room/. 
        *   This seems more like a framework class that wouldn't need any user edits. So I'm deleting the copy made inside the /Assets/App... directory
	*   Room//--ROOM_SERVER/NetworkManager/Spawnable Prefabs 
        *   This still references the MST object, change to /App/Data/Demo SinglePrefabsObjects
	*   Room//--ROOM_SERVER/NetworkManager/Logging 
        *   This still references the MST object, change to /App/Data/Demo LoggingConfiguration
	*   Room//--PLAYER_SPAWNER/PlayerSpawner/Player Prefab 
        *   This still references the prefab in MST, change it to the one under /App/Prefabs
	*   Assets/App/Data/Demo SinglePrefabsObjects
        *   Switch FpsCharacter to correct one located under Assets/App...
		*   Also fix the "TopdownCharacter" reference as well
	*   Room//--ROOM_SERVER/DefaultScene
        *   Switch Offline Scene to Client under /App instead of MST...
		*   Switch Online Scene to Room under /App instead of MST...
	*   Assets/App/Prefabs/FpsCharacter
		*   Fix references for attached scripts to be from /Assets/App instead of Assets/MST...
			PlayerCharacter
			  OLD: 0543a073d387b9b479f00b44ff7b949f
			  NEW: 85aa384a4284be0488ed946023e63858
			PlayerCharacterAvatar
			  OLD: 25cbee4aa79d5cc48ac6a2009de1ddba
			  NEW: 57c57943fea2d4b4eabacddd3910b205
			PlayerCharacterFpsLook
			  OLD: 90ac1276ec5a5e24bad6f9acde8e171d
			  NEW: ebff41c91a3155546bee1270943b4775
			PlayerCharacterInput
			  OLD: f5887ef270305f54fbdd6c4b9696540d
			  NEW: 77ebb3e1156c246478ab36e696c394b8
			PlayerCharacterMovement
			  OLD: f46a67a6e947bed4588feab023ff4320
			  NEW: a9a9bda166f3aa74ab551469db5c0139
			PlayerCharacterTopDownLook
			  OLD: bd8c72afd7f3cdc4083a95070d81bd35
			  NEW: 778f12db4da93fd4d8bac75555f1bd1a
			PlayerCharacterTopDownMovement
			  OLD: 2069cae679e418548ac870ccec6faf83
			  NEW: 49985a3c3c1dc2f40b6e5d2d17c89951
	*   Fix Assets/App/Prefabs/--ROOM_SERVER refs
		*   NetworkManager > Logging & Spawnable Prefabs
		*   DefaultScene > Offline Scene & Online Scene
		*   Delete --ROOM_SERVER from Client Scene and re-add it from App/Prefabs
		*   Delete --ROOM_SERVER from Room Scene and re-add it from App/Prefabs
		*   In Room Scene//--ROOM_SERVER game object (NOT prefab)
		    *   Add Match Timer script
			*   Set Room Server Manager > Terminate Room Delay = 0
	*   TODO: Look at Room//MasterCanvas refs and decide if they should be left alone or not.
2. Cleanup Namespaces
	*   In Assets/App/Scripts/Character, update all scripts to use a namespace representing the directory they're located in
	
## Game Type Implementation Steps (WIP)

1.  Create LobbiesListView based on GamesListView
    *   Follow [Unity Multiplayer: Competitive Shooting Tutorial 🎮 - FishNet](https://www.youtube.com/watch?v=5bLfaGsBXl8&list=PLF6lFlLzb6CSuf4g8ZR1VRq-TAgC6AQWD&index=25) for shooting logic
    *   Create `PerformantShoot` C# script inside /Assets/App/Scripts/Character
	*   Create bullet prefab in /Assets/App/Prefabs
        *   Open SampleScene
        *   Create new sphere named "Bullet" in scene
        *   Scale the sphere to 0.3, 0.3, 0.3
        *   Make sphere red by creating a BulletColor material in /App/Materials, select red in Surface Inputs > Base Map, apply new material to Bullet > Mesh Renderer > Materials > Element 0
        *   Create prefab by dragging bullet into /App/Prefabs
        *   Delete bullet object from SampleScene.
	*   Attach PerformantShoot to FpsCharacter prefab
	*   Add reference to bullet prefab
	*   Set bullet speed to 3
	
## General Server Deployment Notes

After creating your server, get the IP address and update the relevant configs. The following config examples use a fake IP address of 99.99.999.99. You will need to replace this with your own server's IP.


### MstDemo1\Builds\App\Client\application.cfg
```
-mstStartClientConnection=True
-mstMasterIp=99.99.999.99
-mstMasterPort=5000
```

### MstDemo1\Builds\App\MasterAndSpawner\application.cfg

```
-mstStartMaster=True
-mstStartSpawner=True
-mstStartClientConnection=True
-mstMasterIp=99.99.999.99
-mstMasterPort=5000
-mstRoomExe=C:\UnityProjects\MstDemo1\Builds\App\Room\Room.exe
-mstRoomIp=99.99.999.99
-mstRoomRegion=
```

### MstDemo1\Builds\App\Room\application.cfg

```
-mstStartClientConnection=True
-mstMasterIp=99.99.999.99
-mstMasterPort=5000
-mstRoomIp=99.99.999.99
-mstRoomPort=7777
```

## Deploying To Windows Server

After creating the initial Windows server, run the following PowerShell commands...

### Allow SFTP to upload builds

```
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0
Start-Service sshd
Set-Service sshd -StartupType 'Automatic'
Get-NetFirewallRule -Name *ssh*
New-NetFirewallRule -Name sshd -DisplayName 'OpenSSH Server' -Enabled True -Direction Inbound -Protocol TCP -Action Allow -LocalPort 22
```
### Open Master Server Toolkit related ports

```
New-NetFirewallRule -DisplayName 'MST Server TCP' -Action Allow -LocalPort 5000 -Protocol TCP
New-NetFirewallRule -DisplayName 'MST Server UDP' -Action Allow -LocalPort 5000 -Protocol UDP
New-NetFirewallRule -DisplayName 'MST Room TCP' -Action Allow -LocalPort 7777 -Protocol TCP
New-NetFirewallRule -DisplayName 'MST Room UDP' -Action Allow -LocalPort 7777 -Protocol UDP
New-NetFirewallRule -DisplayName 'MST Room Default TCP' -Action Allow -LocalPort 1500 -Protocol TCP
New-NetFirewallRule -DisplayName 'MST Room Default UDP' -Action Allow -LocalPort 1500 -Protocol UDP
```

## Deploying To Linux Server

After creating the initial Linux server (tested with Ubuntu 22.04 x64), run the following commands as root...

### Open Master Server Toolkit related ports
```
# ufw allow 5000
# ufw allow 1500
# ufw allow 7777
```

### Make the binaries executable after copying them to the server

```
# chmod +x /UnityProjects/MstDemo1/Builds/App/Nix/MasterAndSpawner/MasterAndSpawner.x86_64
# chmod +x /UnityProjects/MstDemo1/Builds/App/Nix/Room/Room.x86_64
```

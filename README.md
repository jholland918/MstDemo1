# Master Server Toolkit Demo

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

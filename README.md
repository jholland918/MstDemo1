# Master Server Toolkit Demo

## Fix Old References

1.  Room Scene References
	*   Note: Room//Arena object is a prefab that's outside of the /Assets/App folder. Might not need to copy this one over, just know its still in MST.
	*   Room//--ROOM_SERVER/RoomNetworkManager is referencing MST/Bridges/FishNet/BasicRoomsAndLobbies/Scripts/Room/RoomNetworkManager.cs
        *   This seems more like a framework-level script that users won't have to edit. So to make things less confusing, delete the copied RoomNetworkManager.cs inside /App/Scripts/Room... to remove confusion. 
	*   Room//--ROOM_CLIENT/RoomClientManager is referencing the script inside MST/Bridges/FishNet/BasicRoomsAndLobbies/Scripts/Room/. This seems more like a framework class that wouldn't need any user edits. So I'm deleting the copy made inside the /Assets/App... directory
        *   This seems more like a framework-level script that users won't have to edit. So to make things less confusing, delete the copied RoomNetworkManager.cs inside /App/Scripts/Room... to remove confusion. 

	*   Fix prefab reference in Room Scene\--PLAYER_SPAWNER\PlayerSpawner\Player Prefab. This still references the prefab in MST, change it to the one under /app...
	*   Fix Spawnable Prefabs reference in Room Scene/--ROOM_SERVER/NetworkManager/Spawnable Prefabs. This still references the MST object, change to /App/Data/Demo SinglePrefabsObjects
	*   Switch FpsPrefab reference in /App/Data/Demo SinglePrefabsObjects to the correct one under /App
	*   Double check that the FpsPrefab is still working in Room Scene\--PLAYER_SPAWNER\PlayerSpawner\Player Prefab...
	

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

# Master Server Toolkit Demo

## Lobby Implementation Steps (WIP)

1. Create LobbiesListView based on GamesListView
	-copy script contents to new LobbiesListView script
	-add to game object
	-add same references as original script
	-remove GamesListView from gameobject
	-reposition lobbieslistview in scene editor so it doesn't overlap the gameslistview
1. Create CreateNewLobbyView based on CreateNewRoomView
	-duplicate CreateNewRoomView
	-unpack newly created object
	-rename to CreateNewLobbyView
	-create new script "CreateNewLobbyView" with contents of "CreateNewRoomView" script
	-update namespace
	-attach to gameobject
	-update references like original script
	-remove original script CreateNewRoomView
	-reposition new view in scene editor so it doesn't overlap the original view it was copied from
1. Create LobbyView based on PlayersListView
	-duplicate PlayersListView
	-unpack newly created object
	-rename to LobbyView
	-create new script "LobbyView" with contents of "PlayersListView" script
	-update namespace
	-attach to gameobject
	-update references like original script
	-remove original script PlayersListView
	-reposition new view in scene editor so it doesn't overlap the original view it was copied from
1. Add new "Lobbies List" button to MenuView
	-previous notes...
	-Attach event handler to LobbiesList button click event to LobbiesListView.Show
1. Build master, client, room and run master/spawner. Then test client in editor to ensure Menu > Lobbies List button works
1. Update LobbiesListView to open CreateNewLobbyView
	-Update "Create New Game" button to "Create New Lobby"
	-Rename method LobbiesListView.ShowCreateNewRoomView to LobbiesListView.ShowCreateNewLobbyView
	--fix invoke to call "showCreateLobbyView"
	-Fix On Click handlers to reference root "LobbiesListView" gameobject
	-Update each handler to call correct methods: LobbiesListView.ShowCreateNewLobbyView & LobbiesListView.Hide
	-Update CreateNewLobbyView to listen for "MstEventKeys.showCreateLobbyView"
	-Test the button to see if it shows the create lobby dialog box...
1. Update CreateNewLobbyView to actually create a new lobby
	-add new Game Type Dropdown
	--right-click "container" gameobject > UI > Dropdown - TextMeshPro naming it "gameTypeInputDropdown"
	--Add "Survival" into Dropdown Options (replacing "Option A")
	--Add "OneVsOne" into Dropdown Options (replacing "Option B")
	--Add "TwoVsTwo" into Dropdown Options (replacing "Option C")
	--move the dropdown just above "roomMaxConnectionsInputField"
	--add "Layout Element" to Dropdown gameobject...
	---add Min Height: checked: 45
	---add Preferred Height: checked: 45
	---Select the "CreateNewLobbyView" gameobject and add the Dropdown reference to the "Game Type Input Dropdown" component 
	---Fix the Dropdown child "Label" text to be font size 24 and center aligned
	-remove "roomMaxConnectionsInputField" gameobject
	-update script to remove/replace roomMaxConnectionsInputField fields with gameTypeInputDropdown fields...
	-replace "CreateNewMatch()" logic with the one below:
			public void CreateNewMatch()
			{
				Mst.Events.Invoke(MstEventKeys.showLoadingInfo, "Starting lobby... Please wait!");

				Logs.Debug("Starting lobby... Please wait!");

				Regex roomNameRe = new Regex(@"\s+");

				var options = new MstProperties();
				options.Add(Mst.Args.Names.RoomName, roomNameRe.Replace(RoomName, "_"));
				options.Add(Mst.Args.Names.LobbyId, GameType); //jmh//not sure if this is correct

				if (!string.IsNullOrEmpty(Password))
					options.Add(Mst.Args.Names.RoomPassword, Password);

				MatchmakingBehaviour.Instance.CreateNewLobby(GameType, options, () =>
				{
					Show();
				});
			}
	-Create a "MatchmakingBehaviourExtensions" class to implement MatchmakingBehaviour.Instance.CreateNewLobby() and use namespace MasterServerToolkit.Bridges
			/// <summary>
			/// Sends request to master server to start new lobby
			/// </summary>
			/// <param name="spawnOptions"></param>
			public static void CreateNewLobby(this MatchmakingBehaviour mmb, string factory, MstProperties spawnOptions, UnityAction failCallback = null)
			{
				// Note: This was copied from CreateNewRoom() and converted for lobby creation, so it's a WIP.
				Mst.Events.Invoke(MstEventKeys.showLoadingInfo, "Starting lobby... Please wait!");

				// Can't call mmb.logger from MatchmakingBehaviour because it's protected...
				//mmb.logger.Debug("Starting lobby... Please wait!");
				var logger = Mst.Create.Logger(nameof(MatchmakingBehaviourExtensions));
				logger.Debug("Starting lobby... Please wait!");

				// Custom options that will be given to room directly
				var options = new MstProperties();
				options.Add(Mst.Args.Names.StartClientConnection, true);

				Mst.Client.Lobbies.CreateAndJoin(factory, options, (lobby, error) =>
				{
					if (!string.IsNullOrWhiteSpace(error))
					{
						Mst.Events.Invoke(MstEventKeys.showOkDialogBox, new OkDialogBoxEventMessage($"Create New Lobby failed: {error}", () =>
						{
							failCallback?.Invoke();
						}));

						return;
					}
					else
					{
						// TODO: Assuming this means it worked, write code to use the working lobby.

						// Also, look at MstDictKeys and how to use them:
						/*
							 public struct MstDictKeys
							{
								public const string ROOM_ID = "-roomId";
								public const string LOBBY_FACTORY_ID = "-lobbyFactory";
								public const string LOBBY_NAME = "-lobbyName";
								public const string LOBBY_PASSWORD = "-lobbyPassword";
								public const string LOBBY_TEAM = "-lobbyTeam";
						...
						 */

						//Mst.Events.Invoke(MstEventKeys.showOkDialogBox, new OkDialogBoxEventMessage("Creating a lobby worked!", () =>
						//{
						//    failCallback?.Invoke();
						//}));

						Mst.Events.Invoke(MstEventKeys.showLobbyListView, lobby);

						return;
					}
				});
			}
	- Update "Start room" button 
	--Update lable to read "Start Lobby" 
	--Fix OnClick handler reference to CreateNewLobbyView.CreateNewMatch()
	--Rename button gameobject to "startLobbyUIButton"
1. Add Game Types..
	- Create "Lobbies" folder under App/Scripts...
	- Copy the Lobby factories and the module from other sources...
	- Attach LobbyFactoriesModule script to the `Scene:Master//--MASTER_SERVER/SpawnersModule` gameobject so it can register the gametypes...
1. Rebuild binaries and test creating lobby, it should just be displaying "creating lobby..." at this point..
...
1. Update LobbiesListView to list and join lobbies
1. Update LobbyView
	-to list players
	-to allow players to ready/un-ready
	-to allow player host to start game

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

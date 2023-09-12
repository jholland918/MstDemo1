# Master Server Toolkit Demo

## Lobby Implementation Steps (WIP)

1.  Create LobbiesListView based on GamesListView
    *   Copy script contents to new LobbiesListView script
    *   Add to game object
    *   Add same references as original script
    *   Remove GamesListView from gameobject
    *   Reposition lobbieslistview in scene editor so it doesn't overlap the gameslistview
2.  Create CreateNewLobbyView based on CreateNewRoomView
    *   Duplicate CreateNewRoomView
    *   Unpack newly created object
    *   Rename to CreateNewLobbyView
    *   Create new script "CreateNewLobbyView" with contents of "CreateNewRoomView" script
    *   Update namespace
    *   Attach to gameobject
    *   Update references like original script
    *   Remove original script CreateNewRoomView
    *   Reposition new view in scene editor so it doesn't overlap the original view it was copied from
3.  Create LobbyView based on GamesListView
    *   Duplicate GamesListView
    *   Unpack newly created object
    *   Rename to LobbyView
    *   Create new script "LobbyView" with contents of "GamesListView" script
    *   Update namespace
    *   Attach to gameobject
    *   Update references like original script
    *   Remove original script GamesListView
    *   Reposition new view in scene editor so it doesn't overlap the original view it was copied from
4.  Add new "Lobbies List" button to MenuView
    *   Previous notes...
    *   Attach event handler to LobbiesList button click event to LobbiesListView.Show
5.  Build master, client, room and run master/spawner. Then test client in editor to ensure Menu > Lobbies List button works
6.  Update LobbiesListView to open CreateNewLobbyView
    *   Update "Create New Game" button to "Create New Lobby"
    *   Rename method LobbiesListView.ShowCreateNewRoomView to LobbiesListView.ShowCreateNewLobbyView
        *   Fix invoke to call "showCreateLobbyView"
    *   Fix On Click handlers to reference root "LobbiesListView" gameobject
    *   Update each handler to call correct methods: LobbiesListView.ShowCreateNewLobbyView & LobbiesListView.Hide
    *   Update CreateNewLobbyView to listen for "MstEventKeys.showCreateLobbyView"
    *   Test the button to see if it shows the create lobby dialog box...
7.  Update CreateNewLobbyView to actually create a new lobby
    *   Add new Game Type Dropdown
        *   Right-click "container" gameobject > UI > Dropdown - TextMeshPro naming it "gameTypeInputDropdown"
        *   Add "Survival" into Dropdown Options (replacing "Option A")
        *   Add "OneVsOne" into Dropdown Options (replacing "Option B")
        *   Add "TwoVsTwo" into Dropdown Options (replacing "Option C")
        *   Move the dropdown just above "roomMaxConnectionsInputField"
        *   Add "Layout Element" to Dropdown gameobject...
            *   Add Min Height: checked: 45
            *   Add Preferred Height: checked: 45
            *   Select the "CreateNewLobbyView" gameobject and add the Dropdown reference to the "Game Type Input Dropdown" component 
            *   Fix the Dropdown child "Label" text to be font size 24 and center aligned
    *   Remove "roomMaxConnectionsInputField" gameobject
    *   Update script to remove/replace roomMaxConnectionsInputField fields with gameTypeInputDropdown fields...
    *   Replace "CreateNewMatch()" logic with the one below:
			```cs
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
			```
    *   Create a "MatchmakingBehaviourExtensions" class to implement MatchmakingBehaviour.Instance.CreateNewLobby() and use namespace MasterServerToolkit.Bridges
			```cs
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

						Mst.Events.Invoke(AppEventKeys.showLobbyView, lobby);

						return;
					}
				});
			}
			```
    *   Update "Start room" button 
        *   Update lable to read "Start Lobby" 
        *   Fix OnClick handler reference to CreateNewLobbyView.CreateNewMatch()
        *   Rename button gameobject to "startLobbyUIButton"
8.  Add Game Types..
    *   Create "Lobbies" folder under App/Scripts...
    *   Copy the Lobby factories and the module from other sources...
    *   Attach LobbyFactoriesModule script to the `Scene:Master//--MASTER_SERVER/SpawnersModule` gameobject so it can register the gametypes...
9.  Rebuild binaries and test creating lobby, it should just be displaying "creating lobby..." at this point..
10.  Update LobbiesListView to list and join lobbies
    *   Add JoinLobby method to MatchmakingBehaviourExtensions:
			```cs
			public static void JoinLobby(this MatchmakingBehaviour mmb, GameInfoPacket gameInfo)
			{
				var options = new MstProperties();
				options.Add(Mst.Args.Names.LobbyId, gameInfo.Id); //jmh//not sure if this is correct

				Mst.Client.Lobbies.JoinLobby(gameInfo.Id, (lobby, error) =>
				{
					if (!string.IsNullOrWhiteSpace(error))
					{
						Mst.Events.Invoke(MstEventKeys.showLoadingInfo, $"Join lobby error: [{error}]");
					}
					else
					{
						Mst.Events.Invoke(AppEventKeys.showLobbyView, lobby);
						return;
					}
				});
			}
			```
    *   Open LobbiesListView and update Join button click handler call from `MatchmakingBehaviour.Instance.StartMatch(gameInfo);` to `MatchmakingBehaviour.Instance.JoinLobby(gameInfo);`
11.  Update LobbyView
    *   Update table
        *   Go to LobbyView > panel > container > Scroll View > Viewport > Content > * and edit table columns and rows to be the following:
            *   Row 1: [#][Player name][Team][Is Ready]
                *   Update labels on the first objects and delete #4,5,6
            *   Row 2: [1][Demo player][A   ][True    ]
                *   Update labels, then delete buttons
            *   Update LobbyView > panel > container > Scroll View > Viewport > Content
                *   Data Table Layout Group Component > Cols Info Widths: 50,0,250,250
    *   Add Start Game button
        *   Duplicated from "Cancel" button
        *   Rename gameobject to "startGameButton"
        *   Move above "Cancel" button
        *   Update label to "Start Game"
        *   Update onclick reference to LobbyView gameobject > OnStartGame()
        *   Update "LobbyView" gameobject "Start Game Button" reference to map to "startGameButton" gameobject
    *   Rename "updateButton" to "readyButton"
        *   Change label to "Ready"
        *   Change OnClick handler to LobbyView.OnReadyClick()
    *   Rename "createNewGameButton" to "unreadyButton"
        *   Change label to "Un-Ready"
        *   Change OnClick handler to LobbyView.OnUnreadyClick()
        *   Remove additional, unused click handler
12.  Add MatchTimer script (to end game and return players back to the lobby)
    *   Create MatchTimer.cs script in App/Lobbies/Scripts...
    *   Attach MatchTimer
        *   Open Scene:Room 
        *   Unpack --ROOM_SERVER gameobject
        *   Attach MatchTimer to --ROOM_SERVER gameobject
 
	
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

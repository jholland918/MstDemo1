# Master Server Toolkit Demo

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
-mstMasterIp=66.42.125.93
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

New-NetFirewallRule -DisplayName 'MST Server TCP' -Action Allow -LocalPort 5000 -Protocol TCP
New-NetFirewallRule -DisplayName 'MST Server UDP' -Action Allow -LocalPort 5000 -Protocol UDP
New-NetFirewallRule -DisplayName 'MST Room TCP' -Action Allow -LocalPort 7777 -Protocol TCP
New-NetFirewallRule -DisplayName 'MST Room UDP' -Action Allow -LocalPort 7777 -Protocol UDP
New-NetFirewallRule -DisplayName 'MST Room Default TCP' -Action Allow -LocalPort 1500 -Protocol TCP
New-NetFirewallRule -DisplayName 'MST Room Default UDP' -Action Allow -LocalPort 1500 -Protocol UDP

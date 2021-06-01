$serviceName = "NeuroMedia Analytics Agent"

# verify if the service already exists, and if yes remove it first
if (Get-Service $serviceName -ErrorAction SilentlyContinue)
{
	# using WMI to remove Windows service because PowerShell does not have CmdLet for this
    $serviceToRemove = Get-WmiObject -Class Win32_Service -Filter "name='$serviceName'"
    $serviceToRemove.StopService()
    "service removed"
}
else
{
	# just do nothing
    "service does not exists"
}

$workdir = Split-Path $MyInvocation.MyCommand.Path

$binaryPath = "$workdir\NeuroMedia.Analytics.Agent.exe"

# creating widnows service using all provided parameters
New-Service -name $serviceName -binaryPathName $binaryPath -displayName $serviceName -startupType Automatic

"installation done"
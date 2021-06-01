if (Get-Service neuromedia-analytics-agent -ErrorAction SilentlyContinue) {
  $service = Get-WmiObject -Class Win32_Service -Filter "name='neuromedia-analytics-agent'"
  $service.StopService()
  Start-Sleep -s 1
  $service.delete()
}

"uninstallation done"
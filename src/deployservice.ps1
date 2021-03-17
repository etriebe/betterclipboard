& dotnet build src;

$serviceName = "betterclipboard";
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue;
$serviceFolder = "C:\BetterClipboard";
$binaryName = "betterclipboard.exe";
$binaryPathName = Join-Path -Path $serviceFolder -ChildPath $binaryName;

if ($service -eq $null)
{
    Write-Host "Creating $serviceName service...";
    $service = New-Service -Name $serviceName `
        -BinaryPathName $binaryPathName `
        -DisplayName "Better Clipboard" `
        -StartupType Automatic `
        -Description "Service to help automatically format clipboard items";
    Write-Host "Service created!" -ForegroundColor Green;
}

if ($service.Status -ne "Stopped")
{
    Write-Host "Stopping $serviceName service...";
    $service.Stop();
}

Write-Host "Publishing betterclipboard to $serviceFolder...";
& dotnet publish src -o "$serviceFolder";
Write-Host "Published!" -ForegroundColor Green;

Write-Host "Starting $serviceName service..."
$service.Start();
Write-Host "Service Started!" -ForegroundColor Green;
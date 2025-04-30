<#
.SYNOPSIS
Validate connectivity

.DESCRIPTION
Test-Connectivity tests the connectivity to an AKS cluster.

.EXAMPLE
Test-Connectivity

This command will test the connectivity to an AKS cluster.
#>
[CmdletBinding()]
Param(
    [Parameter(
        Mandatory = $true,
        HelpMessage = 'URL to call')]
    [string] $UrlToTest,
    [Parameter(
        Mandatory = $false,
        HelpMessage = 'Times to retry')]
    [Int]  $RetryTimes = 100,
    [Parameter(
        Mandatory = $false,
        HelpMessage = 'Seconds to wait between attempts')]
    [Int]  $WaitTime = 10
)
$ErrorView = 'NormalView'

for ($Attempt = 1; $Attempt -lt $Retrytimes + 1; $Attempt++) {
    Write-Host "Checking for connectivity to url $UrlToTest. Attempt $Attempt/$Retrytimes"

    try {
        $response = Invoke-WebRequest -Uri $UrlToTest
    }
    catch {
        Write-Host "Error: $_";
    }

    if ($response.StatusCode -eq 200) {
        Write-Host "Connectivity test successful"
        break
    }
    else {
        if ($Attempt -eq $Retrytimes) {
            throw "Connectivity test unsuccessful"
        }
        else {
            Write-Host "Waiting $WaitTime seconds for connectivity..."
            Start-Sleep -Seconds $WaitTime
        }
    }
}
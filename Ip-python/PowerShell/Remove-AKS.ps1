<#
.SYNOPSIS
Removes the Kubernetes Service.

.DESCRIPTION
Removes the Kubernetes Service with the specified name from the specified resource group.

.PREREQUISITES
The specified Kubernetes Service must exist. The principal executing the script must have write permission
on the resource and 'owner' role on the resource group to be able to remove the locks.

.OUTPUTS
Success or error message
#>

[CmdletBinding()]
Param (
    [Parameter(Mandatory = $true)]
    [String] $ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [String] $KubernetesClusterName,

    [Parameter(Mandatory = $false)]
    [Bool] $Wait = $false
)

# In case of an error, display the error message and stop execution
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Making sure module Az.Aks is installed so Remove-AzAks cmdlet is available
If (-not (Get-Module -ListAvailable -Name 'Az.Aks')) {
    Write-Host "Az.Aks is not installed, installing..."
    Install-Module -Name Az.Aks -RequiredVersion 1.0.1 -Force -Scope CurrentUser -WarningAction SilentlyContinue

    Write-Host "Import module Az.Aks."
    Import-Module -Name 'Az.Aks' -Force
}

Try {
    if ($Wait) {
        $Resource = az aks show -n $KubernetesClusterName -g $ResourceGroupName
        az aks wait --deleted -n $KubernetesClusterName -g $ResourceGroupName
    }
    else {
        az aks delete -n $KubernetesClusterName -g $ResourceGroupName
    }
}
Catch {
    Write-Warning "An error occured while checking for Kubernetes Service named '$KubernetesClusterName'. $_"
    Return
}

Write-Host "Removed Kubernetes Service with name '$KubernetesClusterName' from resource group '$ResourceGroupName'"
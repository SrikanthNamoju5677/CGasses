<#
.SYNOPSIS
Validate AKS environment

.DESCRIPTION
Test-AksDeployment validates the deployment of an AKS cluster.

.EXAMPLE
Test-AksDeployment

This command will validate the AKS cluster.
#>
[CmdletBinding()]
Param(
    [Parameter(
        Mandatory = $true,
        HelpMessage = 'AKS cluster name')]
    [string] $AksClusterName,
    [Parameter(
        Mandatory = $true,
        HelpMessage = 'AKS resource group name')]
    [string] $AksResourceGroupName
)
$ErrorView = 'NormalView'

$SubscriptionId = $(az account show --query id -o tsv)
Write-Output "Subscription Id: $SubscriptionId"

$SubscriptionName = $(az account show --query name -o tsv)
Write-Output "Subscription Name: $SubscriptionName"

$TenantId = $(az account show --query tenantId -o tsv)
Write-Output "Tenant Id: $TenantId"

$Username = $(az account show --query user.name -o tsv)
Write-Output "Service Principal Name or ID: $Username"

# Relative path for kube config file
$KubeConfigPath = "./.kube/config"

Write-Output "Get credentials and set up for kubectl to use"
az aks get-credentials -g "$AksResourceGroupName" -n "$AksClusterName" -a -f "$KubeConfigPath"

Write-Output "Get kubectl version info"
kubectl --kubeconfig "$KubeConfigPath" version short

$KubectlOutput = (kubectl get pods -o json)
Write-Host "Kubectl output"
Write-Host $KubectlOutput
$Data = $KubectlOutput | ConvertFrom-Json

ForEach ($Item in $Data.items) {
    Write-Host "Ingress: "$Item.metadata.name

    ForEach ($ContStatus in $Item.status.containerStatuses) {
        Write-Host " > Checking: $($ContStatus.name)"
        if ($ContStatus.ready -eq $false) {
            throw "Failure: $($ContStatus.name) is not ready";
        }
        Write-Host "$($ContStatus.name) is ready"
    }
}

Write-Output "Remove kubectl config file"
Remove-Item -Path "$KubeConfigPath"
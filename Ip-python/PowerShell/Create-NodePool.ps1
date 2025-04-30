<#
.SYNOPSIS
	Creates a Node pool to existing Azure Kubernetes Service

.DESCRIPTION
	Creates a Node pool to existing Azure Kubernetes Service in the specified resource group

.PREREQUISITES
	There should be an existing virtual network,subnet,Azure Kubernetes Service

.OUTPUTS
    Information on the Created node pool details

.PARAMETER ResourceGroupName
    Name of the Resource Group where the AKS cluster is present

.PARAMETER ClusterName
    Name of the existing AKS cluster

.PARAMETER Location
    Location for the deployed AKS cluster

.PARAMETER VirtualNetworkName
    Name of the Virtual Network to be used

.PARAMETER VirtualNetworkRGName
    Name of the Resource Group where the Virtual Network resides

.PARAMETER SubnetName
    Name of the subnet to be used for node pool

.PARAMETER AgentCount
    Number of worker node instances

.PARAMETER WorkerNodeVMSize
    workerNodeVMSize is the size of nodes

.PARAMETER OsType
    osType worker node for AKS cluster

.PARAMETER DeploymentDebugLevel
    DeploymentDebugLevel is None

.PARAMETER AgentPoolName
    agent pool name for AKS worker nodes
#>
[CmdletBinding()]
Param (
    [Parameter(Mandatory = $true)]
    [String] $ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [String] $ClusterName,

    [Parameter(Mandatory = $true)]
    [ValidateSet("westeurope", "northeurope", "westus2", "westus", "westus3", "eastus2", "centralus", "westcentralus", "japaneast", "japanwest")]
    [String] $Location,

    [Parameter(Mandatory = $true)]
    [String] $VirtualNetworkRGName,

    [Parameter(Mandatory = $true)]
    [String] $VirtualNetworkName,

    [Parameter(Mandatory = $true)]
    [String] $SubnetName,

    [Parameter(Mandatory = $true)]
    [Int] $AgentCount,

    [Parameter(Mandatory = $true)]
    [String] $WorkerNodeVMSize,

    [Parameter(Mandatory = $true)]
    [ValidateSet("linux", "windows")]
    [String] $OsType,

    [Parameter(Mandatory = $true)]
    [ValidateSet('None')]
    [String] $DeploymentDebugLevel,

    [Parameter(Mandatory = $true)]
    [String] $AgentPoolName,

    [Parameter(Mandatory = $false)]
    [Int] $MaxPods = 30
)

$TemplateName = 'aksAddAgentpool.json'
Write-Verbose 'Creating parameters object for ARM Template.'
$ParameterARM = @{
    clusterName          = $ClusterName
    location             = $Location
    virtualNetworkRGName = $VirtualNetworkRGName
    virtualNetworkName   = $VirtualNetworkName
    subnetName           = $SubnetName
    agentCount           = $AgentCount
    workerNodeVMSize     = $WorkerNodeVMSize
    osType               = $OsType
    agentPoolName        = $AgentPoolName
    maxPods              = $MaxPods
}
Write-Host 'Get template for AKS node pool add.'
$TemplateFile = Join-Path -Path $PSScriptRoot -ChildPath "..\arm\$TemplateName"
$TemplateName = (Get-ChildItem $TemplateFile).BaseName
$DeploymentName = "ARM-deployment-$( -join ((0x30..0x39) + ( 0x41..0x5A) + ( 0x61..0x7A) | Get-Random -Count 48  | ForEach-Object {[char]$_}) )"

$DeploymentParams = @{
    Name                    = $DeploymentName
    ResourceGroupName       = $ResourceGroupName
    TemplateFile            = $TemplateFile
    TemplateParameterObject = $ParameterARM
    DeploymentDebugLogLevel = $DeploymentDebugLevel
    ErrorVariable           = 'errorMessages'
    ErrorAction             = 'SilentlyContinue'
    Verbose                 = $true
}
Write-Verbose 'Deploying ARM Template.'
New-AzResourceGroupDeployment @DeploymentParams
If ($errorMessages) {
    Write-Error "Template deployment returned the following errors: $errorMessages"
}
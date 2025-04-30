<#
.SYNOPSIS
	Creates an Azure Kubernetes Service

.DESCRIPTION
	Creates an Azure Kubernetes Service in the specified resource group

.PREREQUISITES
	There should be an existing virtual network and a subnet for AKS

.OUTPUTS
    Information on the Created Resource

.PARAMETER ResourceGroupName
    Name of the Resource Group where the AKS cluster will be created

.PARAMETER ClusterName
    Name of the AKS cluster

.PARAMETER ClusterSKUTier
    SKU Tier of the AKS cluster

.PARAMETER Location
    Location for deploying the AKS

.PARAMETER VirtualNetworkName
    Name of the Virtual Network to be used

.PARAMETER VirtualNetworkRGName
    Name of the Resource Group where the Virtual Network resides

.PARAMETER SubnetName
    Name of the subnet to be used

.PARAMETER ServiceCidr
    CIDR range of IP address for AKS services

.PARAMETER DnsServiceIP
    Internal DNS seerver for AKS (EX- 172.17.0.10)

.PARAMETER OsDiskSizeGB
    Worked Node OSDisk Size in GB , default value is 0

.PARAMETER DataDiskSizeGB
    Worked Node DataDisk Size in GB , default value is 0

.PARAMETER AgentCount
    Number of worker node instances

.PARAMETER WorkerNodeVMSize
    workerNodeVMSize is the size of nodes

.PARAMETER LinuxAdminUsername
    linuxAdminUsername is the username for Linux VMs

.PARAMETER SshRSAPublicKey
    SSH key for Linux vm login

.PARAMETER OsType
    osType worker node for AKS cluster

.PARAMETER KubernetesVersion
    AKS cluster version

.PARAMETER EnableHttpApplicationRouting
    enableHttpApplicationRouting for AKS cluster

.PARAMETER NetworkPlugin
    Type of Network for AKS cluster

.PARAMETER DeploymentDebugLevel
    DeploymentDebugLevel is None

.PARAMETER nodeRGName
    Resource group name for backend nodepool

.PARAMETER EnablePodSecurityPolicy
    pod security

.PARAMETER  AgentPoolName
    agent pool name for AKS worker nodes

.PARAMETER NetworkPolicy
    network policy for pods

.PARAMETER UpgradeChannel
    aks cluster automatic upgrade channel

.PARAMETER NodeOSUpgradeChannel
    aks node os automatic upgrade channel

#>

[CmdletBinding()]

Param (

    [Parameter(Mandatory = $true)]
    [String] $ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [String] $ClusterName,

    [Parameter(Mandatory = $true)]
    [String] $ClusterSKUTier,

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
    [String] $ServiceCidr,

    [Parameter(Mandatory = $true)]
    [String] $DnsServiceIP,

    [Parameter(Mandatory = $false)]
    [Int] $OsDiskSizeGB,

    [Parameter(Mandatory = $false)]
    [Int] $DataDiskSizeGB,

    [Parameter(Mandatory = $true)]
    [Int] $AgentCount,

    [Parameter(Mandatory = $true)]
    [String] $WorkerNodeVMSize,

    [Parameter(Mandatory = $true)]
    [String] $LinuxAdminUsername,

    [Parameter(Mandatory = $true)]
    [String] $SshRSAPublicKey,

    [Parameter(Mandatory = $true)]
    [String] $WindowsAdminUsername,

    [Parameter(Mandatory = $true)]
    [String] $WindowsAdminPassword,

    [Parameter(Mandatory = $true)]
    [String] $OsType,

    [Parameter(Mandatory = $true)]
    [String] $KubernetesVersion,

    [Parameter(Mandatory = $true)]
    [Bool] $EnableHttpApplicationRouting,

    [Parameter(Mandatory = $true)]
    [Bool] $EnableKeyvaultSecretsProvider,

    [Parameter(Mandatory = "true")]
    [String] $EnableKeyvaultSecretsRotation,

    [Parameter(Mandatory = $true)]
    [string] $KeyvaultSecretsRotationInterval,

    [Parameter(Mandatory = $true)]
    [String] $NetworkPlugin,

    [Parameter(Mandatory = $false)]
    [String] $NetworkPluginMode,

    [Parameter(Mandatory = $true)]
    [String] $nodeRGName,

    [Parameter(Mandatory = $true)]
    [Bool] $EnablePodSecurityPolicy,

    [Parameter(Mandatory = $true)]
    [ValidatePattern({^[a-z 0-9]{0,8}$})]
    [String] $AgentPoolName,

    [Parameter(Mandatory = $true)]
    [String] $NetworkPolicy,

    [Parameter(Mandatory = $true)]
    [String] $ApplicationGatewayName,

    [Parameter(Mandatory = $true)]
    [Bool] $EnableIngressApplicationGateway,

    [Parameter(Mandatory = $true)]
    [String] $AKSManagedIdentity,

    [Parameter(Mandatory = $false)]
    [Int] $MaxPods = 30,

    [Parameter(Mandatory = $false)]
    [String] $DeploymentDebugLevel = 'None',

    [Parameter(Mandatory = $false)]
    [String] $PodCidr,

    [Parameter(Mandatory = $false)]
    [String] $AdminGroupObjectId,

    [Parameter(Mandatory = $false)]
    [String] $UpgradeChannel = 'stable',

    [Parameter(Mandatory = $false)]
    [String] $NodeOSUpgradeChannel = 'NodeImage'

)

$TemplateName = 'aksDeployMI.json'
Write-Verbose 'Creating parameters object for ARM Template.'

$ParameterARM = @{
    clusterName                      = $ClusterName
    clusterSKUTier                   = $ClusterSKUTier
    location                         = $Location
    virtualNetworkRGName             = $VirtualNetworkRGName
    virtualNetworkName               = $VirtualNetworkName
    subnetName                       = $SubnetName
    serviceCidr                      = $ServiceCidr
    dnsServiceIP                     = $DnsServiceIP
    osDiskSizeGB                     = $OsDiskSizeGB
    dataDiskSizeGB                   = $DataDiskSizeGB
    agentCount                       = $AgentCount
    workerNodeVMSize                 = $WorkerNodeVMSize
    linuxAdminUsername               = $LinuxAdminUsername
    sshRSAPublicKey                  = $SshRSAPublicKey
    windowsAdminUsername             = $WindowsAdminUsername
    windowsAdminPassword             = $WindowsAdminPassword
    osType                           = $OsType
    kubernetesVersion                = $KubernetesVersion
    enableKeyvaultSecretsProvider    = $EnableKeyvaultSecretsProvider
    enableKeyvaultSecretsRotation    = $EnableKeyvaultSecretsRotation
    keyvaultSecretsRotationInterval  = $KeyvaultSecretsRotationInterval
    enableHttpApplicationRouting     = $EnableHttpApplicationRouting
    networkPlugin                    = $NetworkPlugin
    nodeRGName                       = $nodeRGName
    enablePodSecurityPolicy          = $EnablePodSecurityPolicy
    agentPoolName                    = $AgentPoolName
    networkPolicy                    = $NetworkPolicy
    networkPluginMode                = $NetworkPluginMode
    applicationGatewayName           = $ApplicationGatewayName
    enableIngressApplicationGateway  = $EnableIngressApplicationGateway
    maxPods                          = $MaxPods
    podCidr                          = "$PodCidr"
    aksManagedIdentity               = $AKSManagedIdentity
    adminGroupObjectID               = $AdminGroupObjectId
    upgradeChannel                   = $UpgradeChannel
    nodeOSUpgradeChannel             = $NodeOSUpgradeChannel
}

Write-Host 'Get template for AKS.'

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
New-AzResourceGroupDeployment @DeploymentParams -DeploymentDebugLogLevel All

If ($errorMessages) {
    Write-Error "Template deployment returned the following errors: $errorMessages"
}
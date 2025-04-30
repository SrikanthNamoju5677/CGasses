<#
.SYNOPSIS
	Removes an Application Gateway.

.DESCRIPTION
	Removes the specified Application Gateway in the specified resource group.

.PREREQUISITES
	The specified Application Gateway must exist. The principal executing the script must have removal permissions on the resource and owner permissions on any locks.

.OUTPUTS
	Success or error message
#>

[CmdletBinding()]
Param (
    [Parameter(Mandatory = $true)]
    [String] $ResourceGroupName,

    [Parameter(Mandatory = $true)]
    [String] $ApplicationGatewayName,

    [Parameter(Mandatory = $true)]
    [String] $KeyVaultName,

    [Bool] $RemoveRelatedServices

)

$ErrorActionPreference = "Stop"

Try {
    # This throws an error if the resource is not found, even if the error action is silently continue
    $AG = Get-AzApplicationGateway -Name $ApplicationGatewayName -ResourceGroupName $ResourceGroupName

    Remove-AzResource -resourceid $AG.Id -Force | Out-Null
    Write-Host "Removed Application Gateway '$ApplicationGatewayName' from resource group '$ResourceGroupName'."
    Remove-AzKeyVault -Name $KeyVaultName -ResourceGroupName $ResourceGroupName -Force
    Write-Host "Removed application gateway keyvault '$KeyVaultName'"
}
Catch {
    Write-Warning "Application Gateway '$ApplicationGatewayName' cannot be removed from '$ResourceGroupName'. $($_.Exception.Message)"
}

if ($RemoveRelatedServices -eq $true) {
    # Remove Public IP address
    foreach ($fip in $AG.FrontendIPConfigurations) {
        if ($fip.PublicIPAddress) {
            $PIPResourceID = $fip.PublicIPAddress.Id
            Try {
                # This throws an error if the resource is not found, even if the error action is silently continue
                Remove-AzResource -resourceid $PIPResourceID -Force | Out-Null
                Write-Host "Removed Public IP from resource group '$ResourceGroupName'."
            }
            Catch {
                Write-Warning "Public IP cannot be removed from '$ResourceGroupName'. $($_.Exception.Message)"
            }
        }
    }
}
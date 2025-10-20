# Description: This script automatically updates an AWS security group inbound rule with your current public IP address.

# --- CONFIGURATION ---
$securityGroupId = "sg-00a4e132afbeed583"
$port = 5432
$ruleDescription = "Dev personal IP access"

# --- SCRIPT ---

# 1. Get current public IP
try {
    $currentIp = (Invoke-RestMethod -Uri 'https://checkip.amazonaws.com').Trim()
    Write-Host "Your current public IP is: $currentIp"
} catch {
    Write-Error "Failed to get current public IP. Please check your internet connection."
    exit 1
}

$newIpCidr = "$currentIp/32"

# 2. Find the existing rule
Write-Host "Checking for existing rule for port $port in security group $securityGroupId..."
try {
    $rule = aws ec2 describe-security-group-rules --filters "Name=group-id,Values=$securityGroupId" "Name=ip-permission.to-port,Values=$port" "Name=ip-permission.from-port,Values=$port" "Name=ip-permission.protocol,Values=tcp" --query "SecurityGroupRules[?Description=='$ruleDescription'] | [0]" | ConvertFrom-Json
} catch {
    Write-Error "Failed to query security group rules. Is AWS CLI configured? Error: $_"
    exit 1
}


# 3. Compare and update if necessary
if ($null -ne $rule -and $rule.CidrIpv4 -eq $newIpCidr) {
    Write-Host "IP address has not changed. No update needed."
} else {
    # Revoke old rule if it exists
    if ($null -ne $rule) {
        $oldIpCidr = $rule.CidrIpv4
        Write-Host "IP has changed from $oldIpCidr to $currentIp. Updating rule..."
        try {
            aws ec2 revoke-security-group-ingress --group-id $securityGroupId --protocol tcp --port $port --cidr $oldIpCidr | Out-Null
            Write-Host "Successfully revoked old rule for $oldIpCidr."
        } catch {
            Write-Error "Failed to revoke old security group rule. Error: $_"
            exit 1
        }
    } else {
        Write-Host "No previous rule found with description '$ruleDescription'. Creating a new one."
    }

    # Authorize new rule
    try {
        aws ec2 authorize-security-group-ingress --group-id $securityGroupId --protocol tcp --port $port --cidr $newIpCidr --description $ruleDescription | Out-Null
        Write-Host "Successfully authorized new rule for $newIpCidr."
    } catch {
        Write-Error "Failed to authorize new security group rule. Error: $_"
        exit 1
    }

    Write-Host "Security group update complete!"
}

$ErrorActionPreference = 'Stop'

$failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure([string] $Message) {
    $failures.Add($Message)
    Write-Host "::error::$Message"
}

function Test-GitPath([string] $Revision, [string] $Path) {
    & git cat-file -e "${Revision}:$Path" 2>$null
    return $LASTEXITCODE -eq 0
}

function Get-ChangedPaths([string] $Filter, [string] $Base, [string] $Head) {
    $paths = @(& git diff --name-only "--diff-filter=$Filter" "$Base...$Head")
    if ($LASTEXITCODE -ne 0) {
        throw "git diff failed while reading changed paths with filter $Filter."
    }

    return $paths
}

$baseSha = $env:BASE_SHA
$headSha = $env:HEAD_SHA
$headRef = $env:HEAD_REF
$prTitle = $env:PR_TITLE
$prBody = $env:PR_BODY

if ([string]::IsNullOrWhiteSpace($baseSha) -or [string]::IsNullOrWhiteSpace($headSha)) {
    throw 'BASE_SHA and HEAD_SHA are required.'
}

& git rev-parse --is-inside-work-tree *> $null
if ($LASTEXITCODE -ne 0) {
    throw 'Repository policy must run from inside the checked-out Git repository.'
}

$branchPattern = '^(feature|fix|spike|docs|chore)/[0-9]+-[a-z0-9]+(?:-[a-z0-9]+)*$'
if ($headRef -notmatch $branchPattern) {
    Add-Failure "Branch '$headRef' must match <feature|fix|spike|docs|chore>/<issue>-<lowercase-slug>."
}

$titlePattern = '^(feat|fix|docs|test|refactor|perf|build|ci|chore|revert)(\([a-z0-9-]+\))?!?: .+'
if ($prTitle -notmatch $titlePattern) {
    Add-Failure "Pull request title '$prTitle' must follow Conventional Commits."
}

$closingIssuePattern = '(?im)\b(close[sd]?|fix(e[sd])?|resolve[sd]?)\s+#[0-9]+\b'
if ($prBody -notmatch $closingIssuePattern) {
    Add-Failure 'Pull request body must close an issue with Closes #<number>, Fixes #<number>, or Resolves #<number>.'
}

$activePaths = @(Get-ChangedPaths 'ACMR' $baseSha $headSha)
$deletedPaths = @(Get-ChangedPaths 'D' $baseSha $headSha)
$allPaths = @($activePaths + $deletedPaths | Sort-Object -Unique)

$forbiddenPattern = '^(Library|Temp|Logs|obj|UserSettings|\.vs)/'
$credentialFilePattern = '(^|/)(ProjectBrickToken(\.pub)?|id_(rsa|dsa|ecdsa|ed25519)(\.pub)?|[^/]+\.(pem|key|p12|pfx))$'
foreach ($path in $allPaths) {
    if ($path -match $forbiddenPattern) {
        Add-Failure "Generated or user-local path must not be committed: $path"
    }

    if ($path -match $credentialFilePattern) {
        Add-Failure "Credential-like file must not be committed: $path"
    }
}

$privateKeyMarkers = @(
    ('-----BEGIN ' + 'OPENSSH PRIVATE KEY-----'),
    ('-----BEGIN ' + 'RSA PRIVATE KEY-----'),
    ('-----BEGIN ' + 'EC PRIVATE KEY-----'),
    ('-----BEGIN ' + 'DSA PRIVATE KEY-----'),
    ('-----BEGIN ' + 'PRIVATE KEY-----')
)

$grepArguments = @('grep', '-I', '-l', '-F')
foreach ($marker in $privateKeyMarkers) {
    $grepArguments += @('-e', $marker)
}
$grepArguments += @($headSha, '--', '.')

$privateKeyPaths = @(& git @grepArguments 2>$null)
$grepExitCode = $LASTEXITCODE
if ($grepExitCode -eq 0) {
    foreach ($path in $privateKeyPaths) {
        Add-Failure "Private-key material detected in repository file: $path"
    }
}
elseif ($grepExitCode -ne 1) {
    throw 'git grep failed while scanning for private-key material.'
}

$maximumFileSize = 50MB
foreach ($path in $activePaths) {
    $objectName = "${headSha}:$path"
    $sizeText = & git cat-file -s $objectName 2>$null
    if ($LASTEXITCODE -eq 0 -and [long]$sizeText -gt $maximumFileSize) {
        Add-Failure "New or changed file exceeds 50 MB and needs an approved storage strategy: $path"
    }
}

foreach ($path in $activePaths) {
    if ($path -notlike 'Assets/*') {
        continue
    }

    if ($path.EndsWith('.meta', [System.StringComparison]::OrdinalIgnoreCase)) {
        $assetPath = $path.Substring(0, $path.Length - 5)
        if (-not (Test-GitPath $headSha $assetPath)) {
            Add-Failure "Unity metadata has no matching asset: $path"
        }
    }
    elseif (-not (Test-GitPath $headSha "$path.meta")) {
        Add-Failure "Unity asset has no matching .meta file: $path"
    }
}

foreach ($path in $deletedPaths) {
    if ($path -notlike 'Assets/*') {
        continue
    }

    if ($path.EndsWith('.meta', [System.StringComparison]::OrdinalIgnoreCase)) {
        $assetPath = $path.Substring(0, $path.Length - 5)
        if (Test-GitPath $headSha $assetPath) {
            Add-Failure "Deleted Unity metadata still has a matching asset: $assetPath"
        }
    }
    elseif (Test-GitPath $headSha "$path.meta") {
        Add-Failure "Deleted Unity asset left its .meta file behind: $path.meta"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "Repository policy failed with $($failures.Count) error(s)."
    exit 1
}

Write-Host 'Repository policy checks passed.'

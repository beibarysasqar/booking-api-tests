param(
    [string]$Filter = "TestCategory!=KnownBug",
    [string]$ResultsDir = "TestResults"
)

$repoRoot = Join-Path $PSScriptRoot ".."
$solution = Join-Path $repoRoot "booking-api-tests.sln"
$resultsPath = Join-Path $repoRoot $ResultsDir
$trxPath = Join-Path $resultsPath "results.trx"

# clean up first, so a failed build doesn't pick up results.trx from the last run
Remove-Item $resultsPath -Recurse -Force -ErrorAction SilentlyContinue

$testArgs = @("test", $solution, "--logger", "trx;LogFileName=results.trx", "--results-directory", $resultsPath)
if ($Filter) {
    $testArgs += @("--filter", $Filter)
}

Write-Host "Running: dotnet $($testArgs -join ' ')"
dotnet @testArgs
$dotnetExitCode = $LASTEXITCODE

if (-not (Test-Path $trxPath)) {
    Write-Host "TRX file not found: $trxPath (build error?)" -ForegroundColor Red
    exit 1
}

[xml]$trx = Get-Content $trxPath
$counters = $trx.TestRun.ResultSummary.Counters
$failed = [int]$counters.failed

Write-Host ""
Write-Host "Total:   $($counters.total)"
Write-Host "Passed:  $($counters.passed)" -ForegroundColor Green
Write-Host "Failed:  $failed" -ForegroundColor ($failed -gt 0 ? "Red" : "Green")
# skipped tests are called "notExecuted" in TRX
Write-Host "Skipped: $($counters.notExecuted)"

# zero tests is almost always a typo in the filter - don't let that pass as green
if ([int]$counters.total -eq 0) {
    Write-Host "No tests were run, check the filter: '$Filter'" -ForegroundColor Red
    exit 1
}

if ($failed -gt 0) {
    Write-Host ""
    Write-Host "Failed tests:" -ForegroundColor Red
    $trx.TestRun.Results.UnitTestResult |
        Where-Object { $_.outcome -eq "Failed" } |
        ForEach-Object { Write-Host "  $($_.testName)" }
}

if ($failed -gt 0 -or $dotnetExitCode -ne 0) {
    exit 1
}
exit 0

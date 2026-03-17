Param(
    [switch]$Watch
)

Write-Host "=== TechPro quick runner ===" -ForegroundColor Cyan

$apiPath = "TechPro.API"
$mvcPath = "TechPro.MVC"

if (-not (Test-Path $apiPath) -or -not (Test-Path $mvcPath)) {
    Write-Error "Không tìm thấy thư mục TechPro.API hoặc TechPro.MVC. Vui lòng chạy script trong thư mục gốc dự án (d:\TechPro\full)."
    exit 1
}

$apiCmd = if ($Watch) { "dotnet watch run" } else { "dotnet run" }
$mvcCmd = if ($Watch) { "dotnet watch run" } else { "dotnet run" }

Write-Host "Mở cửa sổ API..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PWD\$apiPath`"; $apiCmd"

Start-Sleep -Seconds 2

Write-Host "Mở cửa sổ MVC..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$PWD\$mvcPath`"; $mvcCmd"

Write-Host ""
Write-Host "API và MVC đã được chạy trong 2 cửa sổ PowerShell riêng." -ForegroundColor Green
Write-Host "Dùng tham số -Watch để chạy 'dotnet watch run' (tự reload khi đổi code):" -ForegroundColor DarkCyan
Write-Host "  .\run-all.ps1 -Watch" -ForegroundColor White


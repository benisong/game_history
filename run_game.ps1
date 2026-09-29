$PSScriptRoot_Donghan = Join-Path $PSScriptRoot "donghan\Frontend"
$FrontendCsproj = Join-Path $PSScriptRoot_Donghan "DonghanFrontend.csproj"

Write-Host "====== [Step 1/2] Compiling C# Core and Frontend... ======" -ForegroundColor Green
dotnet build $FrontendCsproj -c Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed! Please check your C# code." -ForegroundColor Red
    Exit 1
}

Write-Host "====== [Step 2/2] Launching Godot Engine... ======" -ForegroundColor Green
& "C:\Users\beni3\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64.exe" --path $PSScriptRoot_Donghan

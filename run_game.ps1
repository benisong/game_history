Write-Host "====== [Step 1/2] Compiling C# Core and Frontend... ======" -ForegroundColor Green
dotnet build "C:\Users\beni3\opencode\donghan\Frontend\DonghanFrontend.csproj" -c Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build failed! Please check your C# code." -ForegroundColor Red
    Exit 1
}

Write-Host "====== [Step 2/2] Launching Godot Engine... ======" -ForegroundColor Green
& "C:\Users\beni3\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64\Godot_v4.6.3-stable_mono_win64.exe" --path "C:\Users\beni3\opencode\donghan\Frontend"

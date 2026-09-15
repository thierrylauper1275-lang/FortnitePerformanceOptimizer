# Build configuration for Release

Dotnet version: 8.0+

## Release Build Steps

1. Clean previous builds:
```bash
dotnet clean
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build Release (x64):
```bash
dotnet build -c Release -r win-x64 /p:SelfContained=false
```

4. Publish (creates standalone executable):
```bash
dotnet publish -c Release -r win-x64 -o ./publish
```

## Output

- Debug: `bin/Debug/net8.0-windows/FortnitePerformanceOptimizer.exe`
- Release: `bin/Release/net8.0-windows/FortnitePerformanceOptimizer.exe`
- Publish: `publish/FortnitePerformanceOptimizer.exe`

## Running Tests

```bash
dotnet test
```

## Application Logs

Logs are stored at:
```
%APPDATA%\FortnitePerformanceOptimizer\logs\fpo_YYYY-MM-DD_HH-mm-ss.log
```

## System Requirements for Running

- Windows 10 or Windows 11 (x64)
- .NET 8.0 Runtime
- Administrator privileges (for some optimizations)
- NVIDIA GPU (for NVIDIA-specific features)

rem Start Kestrel web server bw 2020-06-02
rem See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-3.1

cd %~dp0

rem For windows
set ASPNETCORE_ENVIRONMENT=Production
start dotnet run --no-launch-profile

rem For macos
rem ASPNETCORE_ENVIRONMENT=Production dotnet run

rem pause
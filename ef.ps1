param($param1)

dotnet-ef.exe migrations add $param1 --project .\Core\Core.csproj --startup-project .\BlizzTrack\BlizzTrack.csproj
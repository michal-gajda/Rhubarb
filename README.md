# Rhubarb

```powershell
git init
dotnet new gitignore
dotnet new sln --name Rhubarb
dotnet new web --framework net10.0 --no-https --use-program-main --output src/Web --name Rhubarb.Web
dotnet sln add src/Web
```

```powershell
dotnet new classlib --framework net10.0 --output src/Shared --name Rheum.Shared
dotnet add src/Web reference src/Shared
dotnet sln add src/Shared
```

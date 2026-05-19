FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./nuget.config .
COPY ./packages ./packages
COPY src/Shared/Rheum.Shared.csproj Shared/Rheum.Shared.csproj
COPY src/Web/Rhubarb.Web.csproj Web/Rhubarb.Web.csproj
RUN dotnet new sln --name Rhubarb
RUN dotnet sln add Web/Rhubarb.Web.csproj
RUN dotnet restore

COPY src/ .
COPY Directory.Build.props .
RUN dotnet publish Web/Rhubarb.Web.csproj --configuration Release --no-restore --output /app/build

FROM mcr.microsoft.com/dotnet/aspnet:10.0-azurelinux3.0
WORKDIR /app

USER app:app

COPY --chown=app:app --from=build /app/build .

ENV ASPNETCORE_HTTP_PORTS=5080
EXPOSE 5080

HEALTHCHECK --interval=5s --timeout=10s --retries=3 CMD curl --fail http://localhost:5080/healthz || exit 1

ENTRYPOINT ["dotnet", "Rhubarb.Web.dll"]

build:
	dotnet restore
	dotnet build --no-restore

run:
	dotnet run --project src/Web/Rhubarb.Web.csproj

image:
	docker compose build

push: image
	docker push gajdaltd/rhubarb:latest

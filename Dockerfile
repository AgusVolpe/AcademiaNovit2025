FROM mcr.microsoft.com/dotnet/sdk:9.0.303-noble-amd64 AS build-app
WORKDIR /publish

COPY ./AcademiaNovit/*.csproj ./
RUN dotnet restore

COPY ./AcademiaNovit ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0.7-noble-amd64 AS runtime-app
WORKDIR /publish

COPY --from=build-app /publish/out ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "AcademiaNovit.dll"]
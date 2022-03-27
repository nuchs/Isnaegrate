FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY *.csproj ./
COPY *.config ./
RUN dotnet restore --configfile nuget.config
COPY . ./
RUN dotnet publish -c Release -o out

FROM base
COPY --from=build /src/out .
ENTRYPOINT ["dotnet", "JaundicedSage.dll"]


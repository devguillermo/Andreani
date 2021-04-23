#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Api_Geo/Api_Geo.csproj", "Api_Geo/"]
RUN dotnet restore "Api_Geo/Api_Geo.csproj"
COPY . .
WORKDIR "/src/Api_Geo"
RUN dotnet build "Api_Geo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api_Geo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api_Geo.dll"]
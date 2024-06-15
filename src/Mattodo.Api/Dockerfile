FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5058

ENV ASPNETCORE_URLS=http://+:5058

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/Mattodo.Api/Mattodo.Api.csproj", "src/Mattodo.Api/"]
RUN dotnet restore "src/Mattodo.Api/Mattodo.Api.csproj"
COPY . .
WORKDIR "/src/src/Mattodo.Api"
RUN dotnet build "Mattodo.Api.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Mattodo.Api.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mattodo.Api.dll"]

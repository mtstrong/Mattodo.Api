FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7260

ENV ASPNETCORE_URLS=http://+:7260

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Mattodo.Api.csproj", "./"]
RUN dotnet restore "Mattodo.Api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Mattodo.Api.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Mattodo.Api.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mattodo.Api.dll"]

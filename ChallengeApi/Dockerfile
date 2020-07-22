FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5001
ENV ASPNETCORE_URLS=http://*:5001
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["ChallengeApi.csproj", "./"]
RUN dotnet restore "./ChallengeApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ChallengeApi.csproj" -o /app/build
#-c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChallengeApi.csproj" -o /app/publish
#-c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChallengeApi.dll"]

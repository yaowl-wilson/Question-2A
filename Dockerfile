#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["MessagingProducerAPI.csproj", ""]
RUN dotnet restore "./MessagingProducerAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MessagingProducerAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MessagingProducerAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessagingProducerAPI.dll"]
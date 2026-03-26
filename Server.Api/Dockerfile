FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Server.Api/Server.Api.csproj", "Server.Api/"]
COPY ["Server.Application/Server.Application.csproj", "Server.Application/"]
COPY ["Server.Domain/Server.Domain.csproj", "Server.Domain/"]
COPY ["Server.Infrastructure/Server.Infrastructure.csproj", "Server.Infrastructure/"]
RUN dotnet restore "Server.Api/Server.Api.csproj"

COPY . .
RUN dotnet publish "Server.Api/Server.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Server.Api.dll"]

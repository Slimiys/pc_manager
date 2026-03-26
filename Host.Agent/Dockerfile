FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Host.Agent/Host.Agent.csproj", "Host.Agent/"]
COPY ["Server.Application/Server.Application.csproj", "Server.Application/"]
COPY ["Server.Domain/Server.Domain.csproj", "Server.Domain/"]
COPY ["Server.Infrastructure/Server.Infrastructure.csproj", "Server.Infrastructure/"]
RUN dotnet restore "Host.Agent/Host.Agent.csproj"

COPY . .
RUN dotnet publish "Host.Agent/Host.Agent.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8090
EXPOSE 8090
ENTRYPOINT ["dotnet", "Host.Agent.dll"]

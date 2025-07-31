FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EDAI.Server/EDAI.Server.csproj", "EDAI.Server/"]
COPY ["EDAI.Shared/EDAI.Shared.csproj", "EDAI.Shared/"]
RUN dotnet restore "EDAI.Server/EDAI.Server.csproj"
COPY EDAI.Server/ EDAI.Server/
COPY EDAI.Shared/ EDAI.Shared/
WORKDIR "/src/EDAI.Server"
RUN dotnet build "EDAI.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EDAI.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:80 
COPY --from=publish /app/publish .
COPY /wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh
RUN chown -R $APP_UID:$APP_UID /app
USER $APP_UID
ENTRYPOINT ["/wait-for-it.sh", "postgres:5432", "--", "dotnet", "EDAI.Server.dll"]

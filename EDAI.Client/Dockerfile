FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EDAI.Client/EDAI.Client.csproj", "EDAI.Client/"]
COPY ["EDAI.Shared/EDAI.Shared.csproj", "EDAI.Shared/"]
RUN dotnet restore "EDAI.Client/EDAI.Client.csproj"
COPY EDAI.Client/ EDAI.Client/
COPY EDAI.Shared/ EDAI.Shared/
WORKDIR "/src/EDAI.Client"

#RUN dotnet build "EDAI.Client.csproj" -c $BUILD_CONFIGURATION -o /app/build

#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EDAI.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish
# RUN ls -lh /app/publish && ls -lh /app/publish/wwwroot

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80

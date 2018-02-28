FROM microsoft/aspnetcore-build:2.0
WORKDIR /app

# copy csproj and restore as distinct layers
COPY dotnet-event-elastic-bridge/*.csproj ./
RUN dotnet restore

# copy everything else and build
COPY dotnet-event-elastic-bridge ./
RUN dotnet publish -c Release -o out

CMD ["dotnet", "run"]
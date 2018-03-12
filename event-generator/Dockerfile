FROM microsoft/aspnetcore-build:2.0
WORKDIR /app

# copy csproj and restore as distinct layers
COPY event-generator/*.csproj ./
RUN dotnet restore

# copy everything else and build
COPY event-generator ./
RUN dotnet publish -c Release -o out

CMD ["dotnet", "run"]
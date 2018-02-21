FROM microsoft/aspnetcore-build AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY *.csproj .
RUN dotnet restore

# copies the rest of your code
COPY . .
RUN dotnet publish --output /app/ --configuration Release

# build runtime image
FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=builder /app .
EXPOSE 8080

ENTRYPOINT ["dotnet", "captureorderack.dll"]

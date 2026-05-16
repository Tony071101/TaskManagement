FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy và restore
COPY *.sln .
COPY *.csproj .
RUN dotnet restore

# Copy toàn bộ và build
COPY . .
RUN dotnet publish -c Release -o out

# Chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "TaskManagement.dll"]
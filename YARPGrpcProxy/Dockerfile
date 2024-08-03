# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET SDK as a build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["YARPGrpcProxy.csproj", "."]
RUN dotnet restore "YARPGrpcProxy.csproj"

# Copy the entire project and build it
COPY . .
WORKDIR "/src/."
RUN dotnet build "YARPGrpcProxy.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "YARPGrpcProxy.csproj" -c Release -o /app/publish

# Use the base image to run the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YARPGrpcProxy.dll"]


# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar csproj e restaurar pacotes
COPY *.csproj ./
RUN dotnet restore

# Copiar todo o código e publicar
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expõe porta usada pelo ASP.NET Core
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# Comando para rodar a aplicação
ENTRYPOINT ["dotnet", "ConstrutoraApp.dll"]

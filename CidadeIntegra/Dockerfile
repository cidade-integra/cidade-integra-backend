# Etapa única: SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0
WORKDIR /app

# Copiar csprojs
COPY CidadeIntegra.API/CidadeIntegra.API.csproj CidadeIntegra.API/
COPY CidadeIntegra.Application/CidadeIntegra.Application.csproj CidadeIntegra.Application/
COPY CidadeIntegra.Infra.IoC/CidadeIntegra.Infra.IoC.csproj CidadeIntegra.Infra.IoC/
COPY CidadeIntegra.Infra.Data/CidadeIntegra.Infra.Data.csproj CidadeIntegra.Infra.Data/

RUN dotnet restore CidadeIntegra.API/CidadeIntegra.API.csproj

# Copiar toda a solução
COPY . ./

# Publicar
RUN dotnet publish CidadeIntegra.API/CidadeIntegra.API.csproj -c Release -o out

WORKDIR /app/out
COPY CidadeIntegra.API/firebase-key.json ./firebase-key.json
EXPOSE 80
ENTRYPOINT ["dotnet", "CidadeIntegra.API.dll"]
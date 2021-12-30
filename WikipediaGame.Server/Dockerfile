#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM node:14.16.0-alpine as clientbulid
WORKDIR /app
ENV PATH /app/node_modules/.bin:$PATH
COPY WikipediaGame.Server/ClientApp/package.json ./
COPY WikipediaGame.Server/ClientApp/yarn.lock ./
RUN yarn --network-timeout 1000000 --frozen-lockfile --prod 
COPY WikipediaGame.Server/ClientApp/ ./
RUN yarn build

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WikipediaGame.Server/WikipediaGame.Server.csproj", "WikipediaGame.Server/"]
RUN dotnet restore "WikipediaGame.Server/WikipediaGame.Server.csproj"
COPY . .
WORKDIR "/src/WikipediaGame.Server"
RUN dotnet build "WikipediaGame.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WikipediaGame.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=clientbulid /app/build ./wwwroot

ENTRYPOINT ["dotnet", "WikipediaGame.Server.dll"]
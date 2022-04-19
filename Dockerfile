FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /app
COPY openspy-web-backend openspy-web-backend
COPY openspy-web-backend.sln .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "CoreWeb.dll"]
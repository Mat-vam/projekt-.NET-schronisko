FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY "projektNETschronisko.csproj" ./
RUN dotnet restore "projektNETschronisko.csproj"

COPY . ./
RUN dotnet publish "projektNETschronisko.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "projektNETschronisko.csproj.dll"]
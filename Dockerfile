FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source


COPY *.sln .
COPY Cars4Sale/*.csproj ./Cars4Sale/
RUN dotnet restore


COPY Cars4Sale/. ./Cars4Sale/
WORKDIR /source/Cars4Sale
RUN dotnet publish -c release -o /app --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Cars4Sale.dll"]
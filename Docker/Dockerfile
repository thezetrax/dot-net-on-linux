# Build Step
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /shoestore
COPY Shoestore.mvc/*.csproj .
RUN dotnet restore
COPY ./Shoestore.mvc/* .
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "shoestore.dll" ]

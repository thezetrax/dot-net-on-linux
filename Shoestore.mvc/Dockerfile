# Build Step
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /shoestore
COPY Shoestore.mvc/*.csproj ./
RUN dotnet restore
COPY Shoestore.mvc/* ./
RUN dotnet build -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/build ./
# Assets and Views
COPY Shoestore.mvc/Views ./Views
COPY Shoestore.mvc/wwwroot ./wwwroot
ENTRYPOINT [ "dotnet", "Shoestore.mvc.dll" ]

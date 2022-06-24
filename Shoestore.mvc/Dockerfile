FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /shoestore

# Build Step
FROM builder AS build
COPY Shoestore.mvc/*.csproj ./
RUN dotnet restore
COPY Shoestore.mvc/* ./
RUN dotnet build -c Release -o /app/build

# Build Migration Program
FROM builder AS migration
COPY . ./
RUN dotnet restore "Shoestore.migrations/Shoestore.migrations.csproj"
COPY . ./
WORKDIR /shoestore/Shoestore.migrations
RUN dotnet build "Shoestore.migrations.csproj" -c Release -o /shoestore/migration

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/build ./
# Assets and Views
COPY Shoestore.mvc/Views ./Views
COPY Shoestore.mvc/wwwroot ./wwwroot
# Migration Helper Program
COPY --from=migration /shoestore/migration /migration/
# Script to ensure migrating the 
# database before running our app
COPY scripts/entrypoint.sh ./
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh

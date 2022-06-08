FROM mcr.microsoft.com/dotnet/aspnet:6.0 as base
WORKDIR /app
EXPOSE 80

# Build Step
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /shoestore
COPY Shoestore.mvc/*.csproj .
RUN dotnet restore
COPY Shoestore.mvc/* .
RUN dotnet build -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS migration
WORKDIR /shoestore
COPY . .
RUN dotnet restore "Shoestore.migrations/Shoestore.migrations.csproj"
COPY . .
WORKDIR /shoestore/Shoestore.migrations
RUN dotnet build "Shoestore.migrations.csproj" -c Release -o /shoestore/migration

# FROM base as migration-final
# WORKDIR /migration
# COPY --from=migration /shoestore/migration .

FROM base
COPY --from=build /app/build .
COPY --from=migration /shoestore/migration /migration
COPY Shoestore.mvc/entrypoint.sh .
RUN chmod +x ./entrypoint.sh 
CMD /bin/bash ./entrypoint.sh

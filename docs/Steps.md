# First Create a Solution
We need to first create a directory for the solution.
Dotnet doesn't create solutions inside their own directory.

```bash
mkdir Shoestore && cd Shoestore
dotnet new sln
```

# Create the Asp.Net project in the solution

The previous also applies to packages. The `--use-program-main` creates the Asp.Net project
with the program.cs file in a class form with explicit main function, rather than the new simplified
top level statements. Optional, but I prefer to use this approach.

```bash
mkdir Shoestore.mvc && cd Shoestore.mvc
dotnet new mvc --use-program-main=true
```

# Add the mvc project to the solution

Now that we have the mvc package, we need to add it to our solution, even though
we have the package inside the solution directory, that doesn't necessarily mean
it's added to the solution.

```
# Go to the root of the solution.
cd ..
dotnet sln add Shoestore.mvc/
```

# Restore and Build the project

Restore packages and build the package for checking everything is working.

```
cd Shoestore.mvc/
dotnet restore
dotnet build
```

# Add a Dockerfile

Now we have added a solution and a mvc package to our solution we can start
to setup the Dockerfile and docker-compose files. Let's start with out Dockerfile.

## Create a Dockerfile inside our Shoestore.mvc package

```bash
cd Shoestore.mvc\
touch Dockerfile
```

## Start Defining build step 

Asp.Net project needs to be built to properly run inside a container. so first we need
to build the project using the DotNet SDK image and then use the DotNet Runtime image
to run our application binary. So let's start by defining the build step.

```Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /shoestore
COPY Shoestore.mvc/*.csproj .
RUN dotnet restore
COPY Shoestore.mvc/* .
RUN dotnet build -c Release -o /app/build
```

## Add the Final Step

We have added the build step, we now can take the built binaries of our
project from the build process to run on our final step. This final step
is the one running inside the container after building the image.

```Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/build .
ENTRYPOINT [ "dotnet", "Shoestore.mvc.dll" ]
```

Multi-Stage builds also help by reducing the size of our
image, our build stage is only going to be used for the building process
and finally our final step will only be left when the image is built.

## Recap, what our final Dockerfile will look like.

Finally our Dockerfile should look like so.

```Dockerfile
# Shoestore/Shoestore.mvc/Dockerfile
# Build Step
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /shoestore
COPY Shoestore.mvc/*.csproj .
RUN dotnet restore
COPY Shoestore.mvc/* .
RUN dotnet build -c Release -o /app/build

# Final Step
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/build .
ENTRYPOINT [ "dotnet", "Shoestore.mvc.dll" ]
```

# Add a docker-compose file

We have our Dockerfile, now let's use a docker-compose file to setup the
web application and SQLServer db services. We start by creating the `docker-compose.yml`
in our solution root directory.

```bash
# Go to solution root
cd ..
touch docker-compose.yml
```

Let's start working on our web application service.

```yml
# Shoestore/docker-compose.yml
version: "3.9"
services:
  web:
    build:
      context: .
      dockerfile: Shoestore.mvc/Dockerfile
    ports:
      - "8080:80"
  db:
    image: "mcr.microsoft.com/mssql/server"
    environment:
      SA_PASSWORD: "custom_password_123"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
```

Now we have our web service and our db service setup, let's continue by adding a `.dockerignore` file
before checking if everything is working as expected.

## Add a .dockerignore file

This file will be used to exclude files from being copied into our image in the build process. We
will be adding this file to the root of our solution folder, like so `Shoestore/.dockerignore`. Let's
start by creating our ignore file.

```bash
# Solution root
cd ..
touch .dockerignore
```

Populate your ignore file like so.

```dockerignore
**/.classpath
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md
```

Let's check if our docker configuration is working as expected. When the build is done
we should see message saying our service is available on `http://localhost:80`.

```bash
# From the root of the solution
docker compose up --build
```

# Add DotNet CLI tools to our solution.

Now our services are working as expected, we can continue by working on
configuring our project to use the SQLServer database, add Shoe model
and controllers. Let's start with adding CLI(Command-line Interface) tools,
before adding these tools, we need to have a `tool-manifest` this is a file
that contains a list of the tools we are using in our project.

```bash
# Go to solution root
cd ..
dotnet new tool-manifest
```

We are going to be using these tools for scaffolding and generating code in
our project. We could have also installed these tools globally, but I prefer
adding them locally to this project so if I plan to send this project to someone
else they can have all the tools I am using easily.

# Add dotnet-ef tool locally to the solution

The dotnet-ef tool is used for managing and applying migrations of our project.

```bash
# Go to solution root
cd ..
dotnet tool install dotnet-ef
```

We can check if the tool is installed properly by running `dotnet ef`,
we should see an output and a cool looking unicorn :).

# Add SQLServer and EF packages to the mvc project

Now let's go to our mvc directory and start adding SQLServer and EF packages.
These packages are useful to have model generations and connecting our
application to SQLServer.

```bash
cd Shoestore.mvc/
dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore && \
dotnet add package Microsoft.EntityFrameworkCore.SqlServer && \
dotnet add package Microsoft.EntityFrameworkCore.Tools && \
dotnet restore
```

# Add a DBContext to the mvc package

Our next adventure is to add a database context to our project. A database context is a
class, when instantiated we can use it to connect to our database, query our database
and open/close database connections. Let's go to our mvc directory and create a Data
directory, we'll put our database context inside the Data directory.

```bash
cd Shoestore.mvc/
mkdir Data && touch Data/ApplicationDBContext.cs
```

Now we can start working on our DBContext class. Open our `Data/ApplicationDBContext.cs` file.

```csharp
using Microsoft.EntityFrameworkCore;

namespace Shoestore.mvc.Data;

public class ApplicationDBContext : DbContext
{
  public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }
}
```

# Setup the DBContext to be used by the Asp.Net application

We should now be able to connect our DBContext class into our application. We do
that by going to `Program.cs` file and add the following changes.

```csharp
using Microsoft.EntityFrameworkCore;
using Shoestore.mvc.Data;

namespace Shoestore.mvc;

// ...

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = @"Data Source=127.0.0.1,1433;Database=master;User=sa;Password=custom_password_123;";
        builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // ...

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        } else {
            app.UseMigrationsEndPoint();
        }

        app.UseHttpsRedirection();

        // ...
```

# Build and test

Now we have our database configured, we can test out our progress and check if everything is working.

```bash
// Go to mvc directory
cd Shoestore.mvc
dotnet restore
dotnet build
```

# Add a Model to be used for Shoes

Let's add a Shoe model, let's start by creating the file.

```bash
cd Shoestore.mvc\
touch Shoe.cs
```

Now let's work on our model class.

```csharp
namespace Shoestore.mvc.Models;

public class Shoe {
    public int ID { get; set; }
    public string? Name { get; set; }
    public int? Price { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

# Use the Shoe Model inside the DBContext

We have our Shoe model class, but we have to add it to our application DBContext class
so our application can start using it.

```csharp
using Microsoft.EntityFrameworkCore;

namespace Shoestore.mvc.Data;

public class ApplicationDBContext : DbContext
{
  public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

  // We can add our Shoe model as a DBSet here.
  public DbSet<Shoestore.mvc.Models.Shoe> Shoe { get; set; }
}
```

# Create an initial migration

Seems like we are done with setting up database and models for our database. Our final step
is going to be creating a migration using `dotnet-ef` CLI tool we installed earlier. This
tool is going to generate our migration code for us, and can also be used to manage and update
our migration when make changes to our model classes in the future.

## Disclaimer

Before proceeding to create the migration, since the docker container doesn't handle migrations yet,
we can run a SQLServer instance to have the migration work. (Temporary)

```bash
# This is a temporary setup, we should use this container as a SQLServer database for testing purposes 
# starts up SQLServer in the background.
docker run \
        -e "ACCEPT_EULA=Y" \
        -e "SA_PASSWORD=custom_password_123" \
        -p 1433:1433 \
        -d mcr.microsoft.com/mssql/server
```

## --- Continuing On ---


```bash
cd Shoestore.mvc/
dotnet ef migrations add InitialCreate
```

# Add dotnet-aspnet-codegenerator into the solution for code generation and scaffolding

Now we have a working database connection, a Shoe model and migration. We can work on adding
our CRUD functionality. This will be easily code generated using a dotnet CLI tool. Let's start
by adding the tool to our solution.

```bash
# Go to solution root
cd ..
dotnet tool install dotnet-aspnet-codegenerator
```

# Create a Shoe controller using the codegenerator

Now let's continue by creating our CRUD controller using the dotnet-aspnet-codegenerator tool. We
need to first go to our mvc directory and add some packages that will help the codegenerator tool
to understand what kind of code to generate. These are called design packages.

## Adding Design Packages

```bash
cd Shoestore.mvc\
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design && \
dotnet add package Microsoft.EntityFrameworkCore.Design && \
dotnet restore
```

## Adding our Shoe Controller using the dotnet-aspnet-codegenerator CLI tool

Finally we are going to generate our Shoe controller using the codegenerator tool, to generate
our controller we will provide the codegenerator with the model class, DBContext and path where
the controller is supposed to be added.

```
cd Shoestore.mvc\
dotnet dotnet-aspnet-codegenerator controller \
        -name ShoesController \
        -m Shoe \
        -dc ApplicationDBContext \
        --relativeFolderPath Controllers \
        --useDefaultLayout \
        --referenceScriptLibraries
```

We should now have a controller ready, we take a look at the `Shoestore.mvc/Controllers/ShoesController.cs` file and
that should have a class with the CRUD functionality added.

## TO BE CONTINUED


## Reasons for adding a Migration Helper CLI program to our project

When our container runs we should have the SQLServer database setup. We should be able to execute the migrations
we created prior. But there is one issue, since the migration is in the source code we will have difficulty running
the migration from our source in our container. Reasons being

1. Source code is not included in our container, so we don't have access to the migrations
2. Even if the migrations were available from source, we can't run them because our container
  image is using the .NET RUNTIME instead of the .NET SDK.

So the solution for this issue would be create a simple CLI program that contains the migrations
at build time. So in theory we can copy the binary of the migration helper program to our final image
and run the migration helper CLI program when the container starts up.

## Let's create our migration helper
Let's go to the root of our project, and our migration helper.

```bash
mkdir Shoestore.migrations && cd Shoestore.migrations
dotnet new console --use-program-main
```

## Adding packages required for our migration helper.

Before building our migration program, we should include the packages we are going to use in the program.
`Microsoft.AspNetCore.Hosting` is going to be be used to create a simple web application. `Microsoft.Extensions.Configuration` 
and `Microsoft.Extensions.Configuration.Json` are going to enable our program to read json configuration files.

```bash
dotnet add package Microsoft.AspNetCore.Hosting && \
dotnet add package Microsoft.Extensions.Configuration && \
dotnet add package Microsoft.Extensions.Configuration.Json
```

We also need to include our mvc app into our migration app since we will be using the `appsettsings.json` and the DBContext
from our mvc app in our migration helper file. Go to our migration program root directory.

```bash
cd Shoestore/Shoestore.migrations
dotnet add reference ../Shoestore.mvc
```

## Writing the Migration helper

Our migration helper is going to include two classes, one is a Startup class similar to the 
startup class in our mvc app and another one that is our where our main function is going to be
located. Let's create our `MigrationStartup.cs` class.

## MigrationStartup.cs class

The `MigrationStartup` class is going to setting up the configuration

```csharp
// Shoestore/Shoestore.migrations/MigrationStartup.cs
using Shoestore.mvc.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shoestore.migrations;
public class MigrationStartup {
  public MigrationStartup()
  {
      var builder = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables();
      Configuration = builder.Build();

      //.. for test
      Console.WriteLine(Configuration.GetConnectionString("Shoestore"));
  }

  public IConfiguration Configuration { get; }

  public void ConfigureServices(IServiceCollection services)
  {
      services.AddDbContext<ApplicationDBContext>(options =>
      {
          options.UseSqlServer(Configuration.GetConnectionString("Shoestore"));
      });
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env){}
}
```

Now let's go to our `Program.cs` file and start using this project. Now we can create a
`WebApplicationBuilder`, this web application builder will use our `MigrationStartup` class
to build a configuration for our migration program.

```csharp
// Shoestore/Shoestore.migrations/Program.cs
using Shoestore.mvc.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Shoestore.migrations;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Migration...");
        var webhost = new WebHostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<MigrationStartup>()
            .Build();

        using (var context = (ApplicationDBContext) (
            webhost.Services.GetService(typeof(ApplicationDBContext)) ??
            throw new InvalidOperationException("Context shouldn't be null")
        ))
        {
            context.Database.Migrate();
        }
        Console.WriteLine("Migration Done");
    }
}
```

## Building our migration app

Now we have everything setup on our migration helper program. Let's build it to see if it's working.

```bash
cd Shoestore/Shoestore.migrations
dotnet restore
dotnet build
```

## Adding our migration helper into our Dockerfile

Let's add our migration helper into our project. Our migraiton helper is going to be built on it's own
stage and the resulting aplication binary will be copied over on our final stage.

```Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /shoestore

# Build Step
FROM builder AS build
COPY Shoestore.mvc/*.csproj .
RUN dotnet restore
COPY Shoestore.mvc/* .
RUN dotnet build -c Release -o /app/build

# Build Migration Program
FROM builder AS migration
COPY . .
RUN dotnet restore "Shoestore.migrations/Shoestore.migrations.csproj"
COPY . .
WORKDIR /shoestore/Shoestore.migrations
RUN dotnet build "Shoestore.migrations.csproj" -c Release -o /shoestore/migration

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/build .
COPY --from=migration /shoestore/migration /migration
ENTRYPOINT ["dotnet", "Shoestore.mvc.dll"]
```

But this is not done yet, because we are not running the migration helper program when our container starts up.
In order to ensure our migraion runs on startup we would create a simple `bash script`. This bash script will run
our migration and if the migration was successful we will continue with starting up our app. Let's go ahead to our
solution root and create our script.

```bash
cd Shoestore
mkdir scripts
touch scripts/entrypoint.sh
chmod +x scripts/entrypoint.sh
```

Our bash script should contain, the following commands. This will basically be our entrypoint on our app.

```bash
# Shoestore/scripts/entrypoint.sh
#!/bin/bash
set -e

until dotnet /migration/Shoestore.migrations.dll; do
  sleep 1
done

>&2 echo "Starting Server"
dotnet Shoestore.mvc.dll
```
## Using entrypoint bash script in our final step of our container

```Dockerfile
# ... The rest of our Dockerfile

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build /app/build .
# Assets and Views
COPY Shoestore.mvc/Views ./Views
COPY Shoestore.mvc/wwwroot ./wwwroot
# Migration Helper Program
COPY --from=migration /shoestore/migration /migration
# Script to ensure migrating the 
# database before running our app
COPY scripts/entrypoint.sh .
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh
```

```Dockerfile
# Shoestore\Shoestore.mvc\Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /shoestore

# Build Stage
FROM builder AS build
COPY Shoestore.mvc/*.csproj .
# Restore project packages
RUN dotnet restore
COPY Shoestore.mvc/* .
# Create a release build
RUN dotnet build -c Release -o /app/build

# Run the application and make it available on port 80
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
# Assets and Views
COPY Shoestore.mvc/Views ./Views
COPY Shoestore.mvc/wwwroot ./wwwroot
COPY --from=build /app/build .
ENTRYPOINT [ "dotnet", "Shoestore.mvc.dll" ]

```

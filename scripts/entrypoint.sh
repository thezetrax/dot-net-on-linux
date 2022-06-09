#!/bin/bash
set -e

until dotnet /migration/Shoestore.migrations.dll; do
sleep 1
done

>&2 echo "Starting Server"
dotnet Shoestore.mvc.dll

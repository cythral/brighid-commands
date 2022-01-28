#!/bin/bash

CWD=$(dirname ${BASH_SOURCE[0]:-0})
PROJECT_ROOT=$CWD/../
MIGRATION_NAME=$1

cd $PROJECT_ROOT
dotnet tool restore >/dev/null
dotnet ef migrations add $MIGRATION_NAME \
    --project ${PROJECT_ROOT}src/Service/Service.csproj \
    --msbuildprojectextensionspath ${PROJECT_ROOT}obj/Service
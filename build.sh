#!/usr/bin/env bash
dotnet restore && dotnet test ./ASC.Tests/ASC.Tests.csproj && dotnet build
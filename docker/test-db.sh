#!/bin/bash

# Test Azure SQL Database Connection

echo "🔍 Testing Azure SQL Database Connection..."

cd ../viaggia-server

# Add dotnet tools to PATH
export PATH="$PATH:/home/victorhpmelo/.dotnet/tools"

echo "📋 Connection details:"
echo "Server: tcp:YOUR_SQL_SERVER.database.windows.net,1433"
echo "Database: viaggia-db"
echo "User: viaggia_admin"
echo "Password: ViaggiaAzure123!"
echo ""

echo "🔧 Testing EF Core connection..."
dotnet ef dbcontext info --verbose

echo ""
echo "💡 If this fails, you need to:"
echo "1. Connect to your Azure SQL Database with Azure Data Studio"
echo "2. Run this SQL command:"
echo "   CREATE USER viaggia_admin WITH PASSWORD = 'ViaggiaAzure123!';"
echo "   ALTER ROLE db_owner ADD MEMBER viaggia_admin;"
echo ""

cd ../docker

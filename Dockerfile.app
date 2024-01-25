# Use Microsoft's official .NET runtime image.
# Adjust the version as needed.
FROM mcr.microsoft.com/dotnet/runtime:8.0

# Set the working directory in the container
WORKDIR /app

# Copy the app's binaries from your host machine to the container
COPY bin/Release/net8.0/publish/ /app/

# Command to run the application
ENTRYPOINT ["dotnet", "YourAppName.dll"]

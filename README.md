Contains the backend API code

# DEV NOTES

To add GitHub Packages as a NuGet Repository (This is a system wide change):

    dotnet nuget add source --username <USERNAME_HERE> --password <TOKEN_HERE_>  --store-password-in-clear-text --name "Roman015Github" "https://nuget.pkg.github.com/roman015-com/index.json"
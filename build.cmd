dotnet restore
dotnet lambda package --project-location ./SocialCareCaseViewerApi --configuration release --framework netcoreapp3.1 --output-package ./SocialCareCaseViewerApi/bin/release/netcoreapp3.1/social-care-case-viewer-api.zip
dotnet lambda package --project-location ./MongoDBImport --configuration release --framework netcoreapp3.1 --output-package ./MongoDBImport/bin/release/netcoreapp3.1/mongodb-import.zip

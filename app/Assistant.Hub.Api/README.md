## Build and Push Docker Image

```bash
# Configure the appsettings variables. Copy appsettings.Template.json to appsettings.Development.json
cp ./app/Assistant.Hub.Api/appsettings.Template.json ./app/Assistant.Hub.Api/appsettings.Development.json


[ -f .env ] && while IFS= read -r line; do [[ $line =~ ^[^#]*= ]] && eval "export $line"; done < .env
az login --tenant $AZURE_TENANT_ID --use-device-code

project_root=$(git rev-parse --show-toplevel)
dockerfile_root="${project_root}/app/Assistant.Hub.Api"
dockerfile_path="${dockerfile_root}/Dockerfile"
image_name="ai.doc.eval.dotnet_app"

docker build \
    --build-arg "BUILD_CONFIGURATION=Development" \
    -t "${image_name}.dev" \
    -f "${dockerfile_path}" \
    "${dockerfile_root}"

# ./script/devops.sh build_image --name "${image_name}.dev" --version "latest" --namespace "infra" --dockerfile "$dockerfile_path"

# Run container locally
docker run -p 8080:8080 -p 8081:8081 -p 32771:32771 "${image_name}.dev"

# Interactive shell
docker run -it --entrypoint /bin/sh  -p 8080:8080 -p 8081:8081 -p 32771:32771 "${image_name}.dev"
# Start service in container
$ dotnet Assistant.Hub.Api.dll

curl -X POST -v http://localhost:8080/api/chat/weather \
     -H "X-Api-key: $DOTNET_APP_API_KEY" \
     -H "Content-Type: application/json" \
     -d '[{ "user": "What is the forecast for Mankato MN" }]'
```

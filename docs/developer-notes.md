# Developers

You'll need to set up a development environment if you want to develop a new feature or fix issues. The project uses a docker based devcontainer to ensure a consistent development environment.

- Open the project in VSCode and it will prompt you to open the project in a devcontainer. This will have all the required tools installed and configured.

## Setup local dev environment

If you want to develop outside of a docker devcontainer you can use the following commands to setup your environment.

```bash
# Configure the environment variables. Copy example.env to .env and update the values
cp example.env .env

# load .env vars
# [ ! -f .env ] || export $(grep -v '^#' .env | xargs)
# or this version allows variable substitution and quoted long values
# [ -f .env ] && while IFS= read -r line; do [[ $line =~ ^[^#]*= ]] && eval "export $line"; done < .env

# Create and activate a python virtual environment
# Windows
# virtualenv \path\to\.venv -p path\to\specific_version_python.exe
# C:\Users\!Admin\AppData\Local\Programs\Python\Python312\python.exe -m venv .venv
# .venv\scripts\activate

# Linux
# virtualenv .venv /usr/local/bin/python3.12
# python3.12 -m venv .venv
# python3 -m venv .venv
python3 -m venv .venv
source .venv/bin/activate

# Update pip
python -m pip install --no-cache-dir --upgrade pip

# Install dependencies
pip install -r requirements_dev.txt

# Configure linting and formatting tools
sudo apt-get update
sudo apt-get install -y shellcheck
pre-commit install

# Configure .net app
# Configure the appsettings variables. Copy appsettings.Template.json to appsettings.Development.json
cp ./app/Assistant.Hub.Api/appsettings.Template.json ./app/Assistant.Hub.Api/appsettings.Development.json

```

## Run the Application

```bash
# load .env vars
[ ! -f .env ] || export $(grep -v '^#' .env | xargs)
# or this version allows variable substitution and quoted long values
[ -f .env ] && while IFS= read -r line; do [[ $line =~ ^[^#]*= ]] && eval "export $line"; done < .env

# Build the aspnet app
# Build Docker image
project_root=$(git rev-parse --show-toplevel)
dockerfile_root="${project_root}/app/Assistant.Hub.Api"
dockerfile_path="${dockerfile_root}/Dockerfile"
image_name="ai.doc.eval.dotnet_app"

docker build --build-arg "BUILD_CONFIGURATION=Development" -t "${image_name}.dev" -f "${dockerfile_path}" "${dockerfile_root}"

# Run container locally
docker run -p 8080:8080 -p 8081:8081 -p 32771:32771 "${image_name}.dev"

# Interactive shell
docker run -it --entrypoint /bin/bash -p 8080:8080 -p 8081:8081 -p 32771:32771 "${image_name}.dev"
# Start service in container
$ dotnet Assistant.Hub.Api.dll

# Connect to running image
docker exec -it $(docker ps --filter "ancestor=${image_name}.dev"  -q ) /bin/bash

# Test endpoint
curl -X POST -v http://localhost:8080/api/chat/weather \
     -H "X-Api-key: $DOTNET_APP_API_KEY" \
     -H "Content-Type: application/json" \
     -d '[{ "user": "What is the forecast for Mankato MN" }]'
```

Run the python api

```bash
uvicorn api_python.app:app --reload
uvicorn api_python.wsgi:app --reload

# Test endpoint
curl -p "127.0.0.1:8000/health"
```

## Style Guidelines

This project enforces quite strict [PEP8](https://www.python.org/dev/peps/pep-0008/) and [PEP257 (Docstring Conventions)](https://www.python.org/dev/peps/pep-0257/) compliance on all code submitted.

We use [Black](https://github.com/psf/black) for uncompromised code formatting.

Summary of the most relevant points:

- Comments should be full sentences and end with a period.
- [Imports](https://www.python.org/dev/peps/pep-0008/#imports) should be ordered.
- Constants and the content of lists and dictionaries should be in alphabetical order.
- It is advisable to adjust IDE or editor settings to match those requirements.

### Use new style string formatting

Prefer [`f-strings`](https://docs.python.org/3/reference/lexical_analysis.html#f-strings) over `%` or `str.format`.

```python
# New
f"{some_value} {some_other_value}"
# Old, wrong
"{} {}".format("New", "style")
"%s %s" % ("Old", "style")
```

One exception is for logging which uses the percentage formatting. This is to avoid formatting the log message when it is suppressed.

```python
_LOGGER.info("Can't connect to the webservice %s at %s", string1, string2)
```

## Testing

Ideally, all code is checked to verify the following:

All the unit tests pass All code passes the checks from the linting tools To run the linters, run the following commands:

```bash
# Use pre-commit scripts to run all linting
pre-commit run --all-files

# Run a specific linter via pre-commit
pre-commit run --all-files codespell
pre-commit run --all-files prettier

# Run linters outside of pre-commit
codespell .
shellcheck -x ./script/*.sh

# Check for window line endings
find **/ -not -type d -exec file "{}" ";" | grep CRLF
# Fix with any issues with:
# sed -i.bak 's/\r$//' ./path/to/file
# Or Remove them
# find . -name "*.Identifier" -exec rm "{}" \;

# Run Unit tests
python -m pytest ./tests/api_python
python -m pytest --cov-report term-missing --cov=api_python ./tests/api_python
```

## Git Development Tips

Read the [Git tips](git_tips.md) document for some helpful tips on how to use best Git.

Read the [Pull Request Guidelines](pr_standards.md) document for some helpful tips on how to use utilize Pull Requests.
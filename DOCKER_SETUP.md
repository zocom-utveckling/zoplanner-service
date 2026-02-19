# Running .NET Service in Docker Container

This guide explains how to build and run the `zoplanner-service` .NET application in a Docker container.

## Prerequisites

- Docker Desktop installed and running
- Spring Boot backend running on your local machine at `http://localhost:8080`

## Quick Start

### 1. Navigate to the project directory

```bash
cd zoplannerservice
```

### 2. Build the Docker image

```bash
docker build -t zoplanner-service .
```

This command:
- Builds a Docker image using the `Dockerfile` in the current directory
- Tags the image as `zoplanner-service`
- Uses multi-stage build (build + runtime) for optimal image size

### 3. Run the container

```bash
docker run -d -p 5027:5027 --name zoplanner-container -e SpringApi__BaseUrl="http://host.docker.internal:8080/api" zoplanner-service
```

**Command breakdown:**
- `-d` - Run container in detached mode (background)
- `-p 5027:5027` - Map port 5027 from container to your host machine
- `--name zoplanner-container` - Give the container a friendly name
- `-e SpringApi__BaseUrl="http://host.docker.internal:8080/api"` - Set environment variable to connect to Spring Boot on your host machine
- `zoplanner-service` - The image name to use

### 4. Verify the application is running

Check container logs:
```bash
docker logs zoplanner-container
```

You should see:
```
Config peek SpringApi:BaseUrl = http://host.docker.internal:8080/api
Now listening on: http://[::]:5027
Application started. Press Ctrl+C to shut down.
```

### 5. Access the application

Open your browser and navigate to:
- **Swagger UI**: http://localhost:5027/swagger/index.html
- **API endpoints**: http://localhost:5027/api/*

## Important Notes

### Why `host.docker.internal`?

When the .NET app runs inside a Docker container, `localhost` refers to the container itself, not your computer. To access services running on your host machine (like Spring Boot on port 8080), use the special DNS name `host.docker.internal`.

### Connecting to Spring Boot

Make sure your Spring Boot application is running on `http://localhost:8080` **before** starting the .NET container. The .NET service needs to communicate with Spring Boot to function properly.

## Common Docker Commands

### Stop the container
```bash
docker stop zoplanner-container
```

### Start the container again
```bash
docker start zoplanner-container
```

### Remove the container
```bash
docker rm zoplanner-container
```

### View running containers
```bash
docker ps
```

### View all containers (including stopped)
```bash
docker ps -a
```

### Rebuild and restart (after code changes)

If you make changes to the code, rebuild the image and restart:

```bash
# Stop and remove old container
docker stop zoplanner-container
docker rm zoplanner-container

# Rebuild image
docker build -t zoplanner-service .

# Run new container
docker run -d -p 5027:5027 --name zoplanner-container -e SpringApi__BaseUrl="http://host.docker.internal:8080/api" zoplanner-service
```

## Troubleshooting

### Problem: "Connection refused (localhost:8080)"

**Solution**: Make sure Spring Boot is running on your host machine and you're using `host.docker.internal:8080` in the environment variable.

### Problem: "port is already allocated"

**Solution**: Stop any existing containers or processes using port 5027:
```bash
docker stop zoplanner-container
docker rm zoplanner-container
```

### Problem: Container exits immediately

**Solution**: Check the logs to see what went wrong:
```bash
docker logs zoplanner-container
```

## Architecture

```
┌─────────────────────┐
│   Your Computer     │
│                     │
│  Spring Boot        │◄─────────┐
│  (Port 8080)        │          │
│                     │          │
│  ┌────────────────┐ │          │
│  │ Docker         │ │          │
│  │ Container      │ │          │
│  │                │ │          │
│  │ .NET Service   │─┘          │
│  │ (Port 5027)    │            │
│  └────────────────┘ │          │
│         │           │          │
└─────────┼───────────┘          │
          │                      │
          │ host.docker.internal │
          └──────────────────────┘
```

The .NET service runs inside Docker and communicates with Spring Boot on your host machine using `host.docker.internal`.

## Environment Variables

You can override configuration using environment variables:

| Variable | Default | Description |
|----------|---------|-------------|
| `SpringApi__BaseUrl` | `http://localhost:8080/api` | Spring Boot API base URL |
| `ASPNETCORE_URLS` | `http://+:5027` | URL the .NET app listens on |

Example with multiple variables:
```bash
docker run -d -p 5027:5027 --name zoplanner-container \
  -e SpringApi__BaseUrl="http://host.docker.internal:8080/api" \
  -e ASPNETCORE_ENVIRONMENT="Development" \
  zoplanner-service
```

## Next Steps

After the container is running:
1. Open Swagger UI at http://localhost:5027/swagger/index.html
2. Test the Session, Assignment, User, Customer, and Class endpoints
3. Verify communication with Spring Boot backend


## .env file 
1. Create .env.local file next to .env.example
2. Copy and paste the content from the example file
3. Change the tokenkey

The TOKENKEY should be 64 characters long.
Use a random and secure string.



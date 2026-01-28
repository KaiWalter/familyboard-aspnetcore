#!/bin/bash

# docker buildx build --platform linux/arm/v7 -t familyboard-aspnetcore:latest .

docker build --network host --build-arg VARIANT=bookworm-slim --build-arg DOTNETVERSION=8.0 -t familyboard-aspnetcore:latest .

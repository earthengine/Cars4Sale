# Introduction
Cars4Sales is a WEB API app written in ASP .NET core 

# Prerequsits

* Microsoft Windows 10 20H2 19042.867
* Docker version 20.10.2, build 2291f61
* Optional: Microsoft Visual Studio 16.9.2
* Optional: Microsoft .NET Framework 4.8.04084
* Optional: Microsoft .NET Core SDK 5.0.201

To build and run the app you need an operating system that can run Docker and build/run the image. To develop
or debug the code you will need Visual Studio and the SDKs. The versions listed above is only a reference since
they are the versions the author uses. Other versions/products might also work.

# Build the docker image

Make sure docker is installed. Open a terminal and enter the root folder of this solution.

```sh
docker build -t [Image name] .
```

This will buiild the solution into a docker image of the name you specified. 

# Run the image in a container

You can then run it with

```sh
docker run -it -rm -p 5000:80 --name [Container name] [Image Name]
```
The above is for a Linux container, if you are running Docker to run a Windows container, or need more information,
check

* https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-5.0

The author hasn't done this in the test so there is no guarantee that this will work the same way, but it should work
if properly configured.

# Browse the app

Assuming running the image in a Linux container, then you can open a browser and navigate to

* https://localhost:5000/api/Cars4Sale/

This will show the Swagger API document page.

# API keys

To demostrate that the app can handle multiple dealers, two API keys was hardcoded to represent two different dealers:

* DDF53D65-E63B-4677-A0F5-DE3D8AE576AE for dealer "CarsRus"
* 03C80182-D7D7-4732-92EC-3FCC898EB706 for dealer "ECars"

To use modifying API functions like POST/DELTE/PATCH, the API key above have to present as a HTTP header "ApiKey".

For create, the new entity (car) will be added under the dealer with that key.

For update/delete, if the modifying entity is not belong the that dealer, it will be rejected with error messages.

# The sample database

The system contains three car model data:

* Toyota Corola 2015, stock: 20 (belongs to CarsRus)
* Carl Banz "Benz Patent Motor" 1886, stock: 1 (this one is the earliest gas powered vehicle, the first one that can be called a "car", belongs to ECars)
* Audi A4 2020, stock: 15 (belongs to ECars)

# Data validation

There is no data validations implemented when creating a new car, except the year. The year can only be between 1886 and the current year,
since there was no car before 1886 and the cars after this year havn't been built yet.

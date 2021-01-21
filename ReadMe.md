# MyPlanner

You'll need to install the following pre-requisites in order to build SAFE applications

* The [.NET 5 SDK](https://www.microsoft.com/net/download)
* The [Yarn](https://yarnpkg.com/lang/en/docs/install/) package manager (you can also use `npm` but the usage of `yarn` is encouraged).
* [Node LTS](https://nodejs.org/en/download/) installed for the front end components.
* If you're running on OSX or Linux, you'll also need to install [Mono](https://www.mono-project.com/docs/getting-started/install/).

## Work with the application

Before you run the project **for the first time only** you should install its local tools with this command:

```bash
dotnet tool restore
dotnet paket restore
dotnet build
```

To run the application locally:

```bash
dotnet fake build -t run
```

To build a release version
```bash
dotnet fake build --target BuildRelease
```

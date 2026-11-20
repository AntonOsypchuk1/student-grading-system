# NuGet Central Package Management

All .NET projects in SGS use the root `Directory.Packages.props` file to
manage package versions. The API, semantic analysis service, libraries,
experiment runner and tests remain independent projects and keep their own
`PackageReference` items. Central management does not install all listed
packages in every project; NuGet restores the dependencies that each project
actually references.

## Add or update a package

1. Add or change its `PackageVersion` in `Directory.Packages.props`.
2. Add a versionless `PackageReference` only to the project that needs it.
3. Keep project-specific assets (such as `PrivateAssets` for EF tools or
   gRPC tooling) in the project file.
4. Restore and build the solution; test the affected projects.

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Grpc.Net.Client" Version="2.67.0" />
```

```xml
<!-- A project that uses the package -->
<PackageReference Include="Grpc.Net.Client" />
```

Version overrides are intentionally disabled. If a future project targets
a different framework and needs a different package version, use a central
conditional `PackageVersion` based on `$(TargetFramework)` and document the
reason; all current .NET projects target net9.0.

## Alignment decisions

- EF Core uses 9.0.3, including a transitive pin for `Relational` to prevent
  Npgsql's lower minimum version resolving to a different assembly in
  downstream projects. Npgsql provider stays at 9.0.4.
- gRPC packages use 2.67.0, including the server, client and code generator.
- The preview configuration binder is replaced with stable 9.0.3.
- Microsoft.Extensions DI, HTTP, logging and options use 9.0.3 instead of
  older transitive versions; logging/DI abstractions stay at 10.0.3, required
  by the existing TesseractOCR 5.5.2 dependency. This does not change the
  target framework.
- Other directly referenced packages retain their existing versions.

Transitive pinning applies only to listed central packages that are present
in a project's dependency graph. It does not create a universal dependency
lockfile or guarantee all unlisted transitive packages have the same version.
Review new dependency constraints on restore instead of suppressing downgrade
warnings. Security upgrades are separate from this migration.

## Build and deployment

```sh
dotnet restore SGS.sln
dotnet build SGS.sln --no-restore
dotnet list SGS.sln package --include-transitive
```

The individual projects can still be restored, built and published from their
own `.csproj` paths. MSBuild finds the central file in the parent directory.
The Docker build context must include that file and project references before
the first restore; the API Dockerfile copies them to preserve restore caching.
The runtime container only needs the publish output, not the central file.

See [NuGet Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management).

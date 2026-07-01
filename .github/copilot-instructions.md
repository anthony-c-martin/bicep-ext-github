# Agent Instructions

## Model Authoring
* Bicep model classes should follow standard C# naming conventions, with properties defined using PascalCase. This ensures the types shown in Bicep are using camelCase, even if the underlying API follows a different convention (e.g. snake_case).

## Iterating
To iterate on changes to the extensions (e.g. changing or adding models or handlers), use the following flow:
* Make changes
* Build the solution using `dotnet build .`. This'll typically run a lot faster for catching C# errors than runnig a full publish.
* Run `./scripts/publish.ps1 ./bicep-ext-github` to publish the self-contained extension to `./bicep-ext-github`.
* Modify `./samples/bicepconfig.json` to use the local extension instead of the one from the registry. For example:
    ```json
    {
      "experimentalFeaturesEnabled": {
        "localDeploy": true
      },
      "extensions": {
        "github": "../bicep-ext-github"
      },
      "implicitExtensions": []
    }
    ```
* Check the samples for warnings or errors by running `bicep lint --pattern './samples/**/*.bicepparam'`. Note that the error code BCP427 is expected, as expected env variables are not set - ignore this.

To verify that the extension actually works end-to-end, ask the user to select a sample to run, and be very clear that this will actually interact with the external environment, and can potentially be destructive.

After making changes, if relevant, add or update samples.
using System.Net;
using System.Text;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;
using Sodium;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class DependabotSecretTests
{
    [TestMethod]
    public async Task Encrypts_value_with_repository_public_key()
    {
        var keyPair = PublicKeyBox.GenerateKeyPair();
        var publicKeyBase64 = Convert.ToBase64String(keyPair.PublicKey);

        var publicKey =
            $$"""
            {
                "key_id": "key-1",
                "key": "{{publicKeyBase64}}"
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.EndsWith("/public-key", StringComparison.Ordinal)
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, publicKey)
                : MockHttpMessageHandler.Empty(HttpStatusCode.Created));

        var handler = new DependabotSecretHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "DependabotSecret", new
        {
            owner = "acme",
            repo = "widgets",
            name = "NUGET_TOKEN",
            value = "super-secret",
        });

        Assert.IsNull(response.ErrorData);

        var put = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/repos/acme/widgets/dependabot/secrets/NUGET_TOKEN", put.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(put.Body);
        Assert.AreEqual("key-1", body.GetProperty("key_id").GetString());

        var encrypted = Convert.FromBase64String(body.GetProperty("encrypted_value").GetString()!);
        var decrypted = SealedPublicKeyBox.Open(encrypted, keyPair.PrivateKey, keyPair.PublicKey);
        Assert.AreEqual("super-secret", Encoding.UTF8.GetString(decrypted));
    }

    [TestMethod]
    public async Task Does_not_return_secret_value_to_bicep()
    {
        var keyPair = PublicKeyBox.GenerateKeyPair();
        var publicKeyBase64 = Convert.ToBase64String(keyPair.PublicKey);

        var publicKey =
            $$"""
            {
                "key_id": "key-1",
                "key": "{{publicKeyBase64}}"
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.EndsWith("/public-key", StringComparison.Ordinal)
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, publicKey)
                : MockHttpMessageHandler.Empty(HttpStatusCode.Created));

        var handler = new DependabotSecretHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "DependabotSecret", new
        {
            owner = "acme",
            repo = "widgets",
            name = "NUGET_TOKEN",
            value = "super-secret",
        });

        var properties = response.ResourceProperties();
        Assert.IsFalse(
            properties.TryGetProperty("value", out var value) && value.ValueKind != JsonValueKind.Null,
            "the secret value must not be echoed back to Bicep");
    }
}

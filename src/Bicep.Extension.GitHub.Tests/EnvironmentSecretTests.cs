using System.Net;
using System.Text;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;
using Sodium;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class EnvironmentSecretTests
{
    [TestMethod]
    public async Task Encrypts_value_with_environment_public_key()
    {
        var keyPair = PublicKeyBox.GenerateKeyPair();
        var publicKeyBase64 = Convert.ToBase64String(keyPair.PublicKey);

        var publicKey =
            $$"""
            {
                "key_id": "key-9",
                "key": "{{publicKeyBase64}}"
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.EndsWith("/public-key", StringComparison.Ordinal)
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, publicKey)
                : MockHttpMessageHandler.Empty(HttpStatusCode.Created));

        var handler = new EnvironmentSecretHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "EnvironmentSecret", new
        {
            owner = "acme",
            repo = "widgets",
            environment = "production",
            name = "API_KEY",
            value = "hunter2",
        });

        Assert.IsNull(response.ErrorData);

        var put = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/repos/acme/widgets/environments/production/secrets/API_KEY", put.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(put.Body);
        Assert.AreEqual("key-9", body.GetProperty("key_id").GetString());

        var encrypted = Convert.FromBase64String(body.GetProperty("encrypted_value").GetString()!);
        var decrypted = SealedPublicKeyBox.Open(encrypted, keyPair.PrivateKey, keyPair.PublicKey);
        Assert.AreEqual("hunter2", Encoding.UTF8.GetString(decrypted));
    }
}

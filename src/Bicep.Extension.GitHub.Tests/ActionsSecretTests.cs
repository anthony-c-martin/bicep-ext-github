using System.Net;
using System.Text;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;
using Sodium;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class ActionsSecretTests
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
                : MockHttpMessageHandler.Empty(HttpStatusCode.NoContent));

        var handler = new ActionsSecretHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "ActionsSecret", new
        {
            owner = "acme",
            repo = "widgets",
            name = "MY_SECRET",
            value = "top-secret",
        });

        Assert.IsNull(response.ErrorData);

        var put = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/repos/acme/widgets/actions/secrets/MY_SECRET", put.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(put.Body);
        Assert.AreEqual("key-1", body.GetProperty("key_id").GetString());

        var encrypted = Convert.FromBase64String(body.GetProperty("encrypted_value").GetString()!);
        var decrypted = SealedPublicKeyBox.Open(encrypted, keyPair.PrivateKey, keyPair.PublicKey);
        Assert.AreEqual("top-secret", Encoding.UTF8.GetString(decrypted));
    }
}

using System.Net;
using System.Text;
using System.Text.Json;
using Bicep.Extension.Github.Handlers;
using Sodium;

namespace Bicep.Extension.Github.Tests;

[TestClass]
public sealed class OrganizationActionsSecretTests
{
    [TestMethod]
    public async Task Encrypts_value_with_organization_public_key()
    {
        var keyPair = PublicKeyBox.GenerateKeyPair();
        var publicKeyBase64 = Convert.ToBase64String(keyPair.PublicKey);

        var publicKey =
            $$"""
            {
                "key_id": "org-key",
                "key": "{{publicKeyBase64}}"
            }
            """;

        var mock = new MockHttpMessageHandler((request, _) =>
            request.RequestUri!.AbsolutePath.EndsWith("/public-key", StringComparison.Ordinal)
                ? MockHttpMessageHandler.Json(HttpStatusCode.OK, publicKey)
                : MockHttpMessageHandler.Empty(HttpStatusCode.NoContent));

        var handler = new OrganizationActionsSecretHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "OrganizationActionsSecret", new
        {
            org = "acme",
            name = "ORG_SECRET",
            value = "org-secret-value",
            visibility = "all",
        });

        Assert.IsNull(response.ErrorData);

        var put = mock.Requests.Single(r => r.Method == HttpMethod.Put);
        Assert.AreEqual("/orgs/acme/actions/secrets/ORG_SECRET", put.Uri.AbsolutePath);

        var body = JsonSerializer.Deserialize<JsonElement>(put.Body);
        Assert.AreEqual("org-key", body.GetProperty("key_id").GetString());
        Assert.AreEqual("all", body.GetProperty("visibility").GetString());

        var encrypted = Convert.FromBase64String(body.GetProperty("encrypted_value").GetString()!);
        var decrypted = SealedPublicKeyBox.Open(encrypted, keyPair.PrivateKey, keyPair.PublicKey);
        Assert.AreEqual("org-secret-value", Encoding.UTF8.GetString(decrypted));
    }
}

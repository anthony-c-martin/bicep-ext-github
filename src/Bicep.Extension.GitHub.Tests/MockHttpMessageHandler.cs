using System.Net;
using System.Text;

namespace Bicep.Extension.Github.Tests;

/// <summary>
/// A test double for <see cref="HttpMessageHandler"/> that records every outgoing request and
/// returns responses produced by a caller-supplied responder function. Used to mock both the raw
/// <see cref="HttpClient"/> calls and the Octokit client's underlying transport.
/// </summary>
public sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, string, HttpResponseMessage> _responder;

    public MockHttpMessageHandler(Func<HttpRequestMessage, string, HttpResponseMessage> responder)
        => _responder = responder;

    public List<RecordedRequest> Requests { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Add(new RecordedRequest(request.Method, request.RequestUri!, body));

        return _responder(request, body);
    }

    public static HttpResponseMessage Json(HttpStatusCode statusCode, string json)
        => new(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    public static HttpResponseMessage Empty(HttpStatusCode statusCode)
        => new(statusCode)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json"),
        };
}

public sealed record RecordedRequest(HttpMethod Method, Uri Uri, string Body);

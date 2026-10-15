using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tywynh.Infrastructure.Persistence;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Enums;
using Tywynh.API.Services;

// Refuse remote or ordinary databases. The runner is for disposable localhost PostgreSQL only.
var connection = Environment.GetEnvironmentVariable("ARCHIVE_TEST_DB");
if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Set ARCHIVE_TEST_DB to a disposable localhost database ending in _test.");
var parsed = new NpgsqlConnectionStringBuilder(connection);
if (!new[] { "127.0.0.1", "localhost" }.Contains(parsed.Host) || parsed.Database?.EndsWith("_test") != true)
    throw new InvalidOperationException("Integration tests refuse remote or non-test databases.");
var schema = "archive_checks_" + Guid.NewGuid().ToString("N");
await using (var adminConnection = new NpgsqlConnection(connection))
{
    await adminConnection.OpenAsync();
    await using var create = new NpgsqlCommand("CREATE SCHEMA " + schema, adminConnection);
    await create.ExecuteNonQueryAsync();
}
parsed.SearchPath = schema;
connection = parsed.ConnectionString;
await using var schemaLease = new SchemaLease(connection, schema);
var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection, o => o.MigrationsHistoryTable("__EFMigrationsHistory", schema)).Options;
await using (var db = new AppDbContext(options)) await db.Database.MigrateAsync();
var fixtures = new Dictionary<string, Guid>();
await using (var db = new AppDbContext(options))
{
    foreach (var status in new[] { "pending", "approved", "rejected", "deleted" })
    {
        var c = Confession.Create("Synthetic integration fixture: " + status, ConfessionCategory.Self, 3, null, "test alias");
        if (status == "approved") c.Approve(); if (status == "rejected") c.Reject("test"); if (status == "deleted") c.SoftDelete();
        db.Confessions.Add(c); fixtures[status] = c.Id;
    }
    await db.SaveChangesAsync();
}
var apiDll = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Tywynh.API.dll"));
var processInfo = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
processInfo.ArgumentList.Add(apiDll);
processInfo.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:55440";
processInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
processInfo.Environment["ConnectionStrings__DefaultConnection"] = connection;
const string secret = "isolated-test-secret-12345678901234567890";
processInfo.Environment["Moderation__AdminKey"] = secret;
processInfo.Environment["Gateway__SharedKey"] = secret;
processInfo.Environment["Receipts__SigningKey"] = secret;
processInfo.Environment["Logging__LogLevel__Default"] = "Warning";
using var server = Process.Start(processInfo) ?? throw new Exception("Could not start test API.");
// Drain logs to avoid blocked pipes; never print submission bodies or receipts.
server.BeginOutputReadLine(); server.BeginErrorReadLine();
using var http = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:55440"), Timeout = TimeSpan.FromSeconds(15) };
var passed = 0;
var requestNumber = 0;
async Task<HttpResponseMessage> Send(string method, string path, object? body = null, bool admin = false, string? key = null, string? visitor = null, string? partition = null)
{
    var req = new HttpRequestMessage(new HttpMethod(method), path);
    req.Headers.Add("X-Gateway-Key", secret);
    req.Headers.Add("X-Archive-Client", partition ?? ArchiveWorkflowService.Hash("test-request-" + Interlocked.Increment(ref requestNumber)));
    if (admin) req.Headers.Add("X-Admin-Key", secret);
    if (key != null) req.Headers.Add("Idempotency-Key", key);
    if (visitor != null) req.Headers.Add("Cookie", "visitor_token=" + visitor);
    if (body != null) req.Content = JsonContent.Create(body);
    return await http.SendAsync(req);
}
void Check(bool condition, string label) { if (!condition) throw new Exception("FAILED: " + label); passed++; Console.WriteLine("PASS: " + label); }
async Task<JsonElement> Json(HttpResponseMessage response) => JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
object Submission(bool tiktok = false, string text = "Synthetic writing for integration tests.", bool consent = true) => new { text, category = "self", intensity = 3, archive_consent = consent, tik_tok_consent = tiktok };
async Task<JsonElement> Submit(bool tiktok = false)
{
    var r = await Send("POST", "/api/confessions", Submission(tiktok), key: Guid.NewGuid().ToString("N"));
    Check(r.StatusCode == HttpStatusCode.Created, "submission accepted pending review"); return await Json(r);
}
try
{
    for (var i = 0; i < 50; i++) { try { if ((await http.GetAsync("/api/health/live")).IsSuccessStatusCode) break; } catch (HttpRequestException) { } await Task.Delay(100); }
    Check((await http.GetAsync("/api/health")).IsSuccessStatusCode, "database readiness succeeds");
    Check((await Send("GET", "/api/moderation/confessions")).StatusCode == HttpStatusCode.Unauthorized, "owner endpoints reject missing admin key");
    Check((await Send("GET", "/api/confessions?page=2147483647&pageSize=100")).StatusCode == HttpStatusCode.BadRequest, "out-of-range page numbers are rejected without server errors");
    var farPage = await Json(await Send("GET", "/api/confessions?page=100000&pageSize=100"));
    Check(!farPage.GetProperty("items").EnumerateArray().Any(), "valid pages beyond the archive return an empty page");
    var list = await Json(await Send("GET", "/api/confessions"));
    Check(list.GetProperty("items").EnumerateArray().All(item => item.GetProperty("id").GetGuid() == fixtures["approved"]), "public list excludes pending, rejected, and deleted writing");
    foreach (var status in new[] { "pending", "rejected", "deleted" })
    {
        Check((await Send("GET", "/api/confessions/" + fixtures[status])).StatusCode == HttpStatusCode.NotFound, "detail protects " + status);
        Check((await Send("POST", "/api/confessions/" + fixtures[status] + "/resonate", visitor: Guid.NewGuid().ToString("N"))).StatusCode == HttpStatusCode.NotFound, "resonance protects " + status);
    }
    var publicStory = await Json(await Send("GET", "/api/confessions/" + fixtures["approved"]));
    Check(publicStory.GetProperty("category").GetString() == "self" && !publicStory.TryGetProperty("author_id", out _) && !publicStory.TryGetProperty("burned", out _), "public contract uses strings and omits internal fields");
    Check((await http.GetAsync("/api/confessions")).StatusCode == HttpStatusCode.Forbidden, "production API rejects calls bypassing the gateway");
    var invalidText = await Send("POST", "/api/confessions", Submission(text: " "), key: Guid.NewGuid().ToString("N"));
    Check(invalidText.StatusCode == HttpStatusCode.BadRequest && (await Json(invalidText)).GetProperty("errors").TryGetProperty("text", out _), "invalid writing produces field-level validation errors");
    Check((await Send("GET", "/api/confessions?page=0")).StatusCode == HttpStatusCode.BadRequest, "invalid pagination returns 400");
    Check((await Send("POST", "/api/confessions", Submission(text: new string('x', 501)), key: Guid.NewGuid().ToString("N"))).StatusCode == HttpStatusCode.BadRequest, "oversized writing returns 400 before persistence");
    Check((await Send("POST", "/api/confessions", Submission(consent: false), key: Guid.NewGuid().ToString("N"))).StatusCode == HttpStatusCode.BadRequest, "publication requires explicit consent");
    var key = Guid.NewGuid().ToString("N");
    var repeats = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => Send("POST", "/api/confessions", Submission(), key: key)));
    var results = await Task.WhenAll(repeats.Select(Json));
    Check(results.Select(r => r.GetProperty("id").GetGuid()).Distinct().Count() == 1, "concurrent retries create one writing record");
    Check(results.Select(r => r.GetProperty("receipt_token").GetString()).Distinct().Count() == 1, "concurrent retries recover the same receipt");
    Check((await Send("POST", "/api/confessions", Submission(text: "Different request"), key: key)).StatusCode == HttpStatusCode.BadRequest, "retry key cannot be reused for different writing");
    var owned = results[0]; var id = owned.GetProperty("id").GetGuid(); var token = owned.GetProperty("receipt_token").GetString();
    Check((await Send("POST", $"/api/contributor/{id}/view", new { receipt_token = new string('0', 64) })).StatusCode == HttpStatusCode.NotFound, "wrong receipt cannot access writing");
    Check((await Send("POST", $"/api/contributor/{id}/view", new { receipt_token = token })).IsSuccessStatusCode, "correct receipt can access own pending writing");
    Check((await Send("POST", $"/api/moderation/confessions/{id}/approve", admin: true)).IsSuccessStatusCode, "consented writing can be approved");
    Check((await Send("POST", $"/api/moderation/publishing/{id}/review", new { excerpt = "Synthetic excerpt" }, admin: true)).StatusCode == HttpStatusCode.NotFound, "declining TikTok blocks external review and export");
    var shared = await Submit(true); var sharedId = shared.GetProperty("id").GetGuid(); var sharedToken = shared.GetProperty("receipt_token").GetString();
    await Send("POST", $"/api/moderation/confessions/{sharedId}/approve", admin: true);
    var reviewedResponse = await Send("POST", $"/api/moderation/publishing/{sharedId}/review", new { excerpt = "A synthetic, reviewed excerpt." }, admin: true);
    Check(reviewedResponse.IsSuccessStatusCode, "approved consented excerpt can be reviewed");
    var record = (await Json(reviewedResponse)).GetProperty("id").GetGuid();
    Check((await Send("GET", $"/api/moderation/publishing/{record}/export", admin: true)).IsSuccessStatusCode, "reviewed excerpt can be exported with active permission");
    Check((await Send("POST", $"/api/moderation/publishing/{record}/published", new { post_url = "https://www.tiktok.com/@synthetic/video/123" }, admin: true)).IsSuccessStatusCode, "manual publication can be tracked");
    Check((await Send("POST", $"/api/contributor/{sharedId}/replace", new { receipt_token = sharedToken, text = "Replacement synthetic writing", category = "self", tik_tok_consent = false })).IsSuccessStatusCode, "replacement returns writing to review");
    Check((await Send("GET", "/api/confessions/" + sharedId)).StatusCode == HttpStatusCode.NotFound, "replacement is hidden until reviewed");
    Check((await Send("GET", $"/api/moderation/publishing/{record}/export", admin: true)).StatusCode == HttpStatusCode.NotFound, "replacement and revoked sharing permission invalidate exports");
    var ledger = await Json(await Send("GET", "/api/moderation/publishing/records", admin: true));
    Check(ledger.EnumerateArray().Single(p => p.GetProperty("id").GetGuid() == record).GetProperty("removal_requested_at").ValueKind != JsonValueKind.Null, "external copies are flagged for removal after replacement");
    Check((await Send("POST", $"/api/contributor/{id}/withdraw", new { receipt_token = token })).IsSuccessStatusCode, "receipt holder can withdraw writing");
    Check((await Send("GET", "/api/confessions/" + id)).StatusCode == HttpStatusCode.NotFound, "withdrawal removes public visibility");
    Check((await Send("POST", $"/api/moderation/confessions/{id}/approve", admin: true)).StatusCode == HttpStatusCode.BadRequest, "withdrawn writing cannot be approved");
    var visitor = Guid.NewGuid().ToString("N");
    var reactions = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Send("POST", "/api/confessions/" + fixtures["approved"] + "/resonate", visitor: visitor)));
    var reactionBodies = await Task.WhenAll(reactions.Select(Json));
    Check(reactionBodies.Count(r => r.GetProperty("is_new").GetBoolean()) == 1 && reactionBodies.All(r => r.GetProperty("resonance_count").GetInt32() == 1), "concurrent reactions are idempotent with accurate counts");
    var others = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Send("POST", "/api/confessions/" + fixtures["approved"] + "/resonate", visitor: Guid.NewGuid().ToString("N"))));
    Check(others.All(r => r.IsSuccessStatusCode) && (await Json(await Send("GET", "/api/confessions/" + fixtures["approved"]))).GetProperty("resonance_count").GetInt32() == 6, "independent concurrent reactions do not lose increments");
    var echoRequests = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Send("GET", "/api/daily-echo")));
    Check(echoRequests.All(r => r.IsSuccessStatusCode), "concurrent first daily reads share one persisted selection");
    var limitClient = ArchiveWorkflowService.Hash("rate-limit-fixture");
    foreach (var n in Enumerable.Range(0, 3)) await Send("POST", "/api/confessions", Submission(), key: Guid.NewGuid().ToString("N"), partition: limitClient);
    var limited = await Send("POST", "/api/confessions", Submission(), key: Guid.NewGuid().ToString("N"), partition: limitClient);
    Check(limited.StatusCode == HttpStatusCode.TooManyRequests && limited.Headers.Contains("Retry-After"), "submission limiter is activated and provides retry guidance");
    Check((await Send("POST", "/api/confessions", Submission(), key: Guid.NewGuid().ToString("N"), partition: ArchiveWorkflowService.Hash("other-client"))).StatusCode == HttpStatusCode.Created, "one client cannot exhaust another client's quota");
    var loginClient = ArchiveWorkflowService.Hash("owner-login-limit-fixture");
    foreach (var n in Enumerable.Range(0, 10)) await Send("POST", "/api/owner/access-attempt", partition: loginClient);
    var loginLimited = await Send("POST", "/api/owner/access-attempt", partition: loginClient);
    Check(loginLimited.StatusCode == HttpStatusCode.TooManyRequests && loginLimited.Headers.Contains("Retry-After"), "owner sign-in attempts are limited independently of moderation work");
    Check((await Send("POST", "/api/owner/access-attempt", partition: ArchiveWorkflowService.Hash("other-owner-client"))).StatusCode == HttpStatusCode.NoContent, "independent owner client can still sign in");
    Check((await Send("POST", "/api/reports", new { confession_id = fixtures["approved"], reason = "identifying details", details = "Synthetic report" })).StatusCode == HttpStatusCode.Accepted, "public readers can report an eligible story");
    var reports = await Json(await Send("GET", "/api/moderation/publishing/reports", admin: true));
    Check(reports.EnumerateArray().Any(r => r.GetProperty("text").GetString() == "Synthetic integration fixture: approved"), "protected reports include source writing for owner review");
    Console.WriteLine($"Completed {passed} backend checks against disposable PostgreSQL.");
}
finally
{
    if (!server.HasExited) { server.Kill(entireProcessTree: true); await server.WaitForExitAsync(); }

}

sealed class SchemaLease(string connection, string schema) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await using var db = new NpgsqlConnection(connection);
        await db.OpenAsync();
        // Internally generated lowercase prefix and GUID; never derived from user input.
        await using var drop = new NpgsqlCommand("DROP SCHEMA IF EXISTS " + schema + " CASCADE", db);
        await drop.ExecuteNonQueryAsync();
    }
}

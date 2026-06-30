using Server.Infrastructure.Configuration;

namespace Server.Infrastructure.Tests;

/// <summary>
/// Тесты загрузчика .env.
/// </summary>
public sealed class DotEnvLoaderTests
{
    [Test]
    public void ReadVariables_ParsesRedmineKeys()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"pcmonitor-env-{Guid.NewGuid():N}.env");
        try
        {
            File.WriteAllText(
                filePath,
                """
                # comment
                REDMINE_URL=https://redmine.example/
                API_KEY=secret-key
                PROJECT_ID=robot_development
                CHECK_INTERVAL=45
                TELEGRAM_BOT_TOKEN=ignored-for-redmine
                """);

            var variables = DotEnvLoader.ReadVariables(filePath);

            Assert.That(variables["REDMINE_URL"], Is.EqualTo("https://redmine.example/"));
            Assert.That(variables["API_KEY"], Is.EqualTo("secret-key"));
            Assert.That(variables["PROJECT_ID"], Is.EqualTo("robot_development"));
            Assert.That(variables["CHECK_INTERVAL"], Is.EqualTo("45"));
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Test]
    public void TryApplyRedmineFromFile_SetsAspNetCoreEnvironmentVariables()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"pcmonitor-env-{Guid.NewGuid():N}.env");
        var targetKeys = new[]
        {
            "Redmine__BaseUrl",
            "Redmine__ApiKey",
            "Redmine__ProjectId",
            "Redmine__PollIntervalSeconds"
        };

        var previous = targetKeys.ToDictionary(
            key => key,
            key => Environment.GetEnvironmentVariable(key));

        try
        {
            foreach (var key in targetKeys)
            {
                Environment.SetEnvironmentVariable(key, null);
            }

            File.WriteAllText(
                filePath,
                """
                REDMINE_URL=https://inside.example/
                API_KEY=test-api-key
                PROJECT_ID=demo
                CHECK_INTERVAL=30
                """);

            var result = DotEnvLoader.TryApplyRedmineFromFile(explicitFilePath: filePath);

            Assert.That(result.Loaded, Is.True);
            Assert.That(result.AppliedRedmineKeys, Is.EqualTo(4));
            Assert.That(Environment.GetEnvironmentVariable("Redmine__BaseUrl"), Is.EqualTo("https://inside.example"));
            Assert.That(Environment.GetEnvironmentVariable("Redmine__ApiKey"), Is.EqualTo("test-api-key"));
            Assert.That(Environment.GetEnvironmentVariable("Redmine__ProjectId"), Is.EqualTo("demo"));
            Assert.That(Environment.GetEnvironmentVariable("Redmine__PollIntervalSeconds"), Is.EqualTo("30"));
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            foreach (var (key, value) in previous)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}

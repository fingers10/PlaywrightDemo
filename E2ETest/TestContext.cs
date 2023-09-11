﻿using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace E2ETest;

[SetUpFixture]
public class TestContext
{
    public static TestContext Instance { get; private set; } = null!;

    private readonly IReadOnlyDictionary<HostingModel, SampleSite> SampleSites = new Dictionary<HostingModel, SampleSite> {
            { HostingModel.Wasm70, new SampleSite(5013, /*"Client",*/ "net7.0") }
        };

    private IPlaywright? _Playwrite;

    private IBrowser? _Browser;

    private IPage? _Page;

    private class TestOptions
    {
        public string Browser { get; set; } = "";

        public bool Headless { get; set; } = true;

        public bool SkipInstallBrowser { get; set; } = false;
    }

    private readonly TestOptions _Options = new();

    public ValueTask<SampleSite> StartHostAsync(HostingModel hostingModel)
    {
        return this.SampleSites[hostingModel].StartAsync();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "DOTNET_")
            .AddTestParameters()
            .Build();
        configuration.Bind(this._Options);

        Instance = this;

        if (!this._Options.SkipInstallBrowser)
        {
            Microsoft.Playwright.Program.Main(new[] { "install" });
        }
    }

    public async ValueTask<IPage> GetPageAsync()
    {
        this._Playwrite ??= await Playwright.CreateAsync();
        this._Browser ??= await this.LaunchBrowserAsync(this._Playwrite);
        this._Page ??= await this._Browser.NewPageAsync();
        return this._Page;
    }

    private Task<IBrowser> LaunchBrowserAsync(IPlaywright playwright)
    {
        var browserType = this._Options.Browser.ToLower() switch
        {
            "firefox" => playwright.Firefox,
            "webkit" => playwright.Webkit,
            _ => playwright.Chromium
        };

        var channel = this._Options.Browser.ToLower() switch
        {
            "firefox" or "webkit" => "",
            _ => this._Options.Browser.ToLower()
        };

        return browserType.LaunchAsync(new()
        {
            Channel = channel,
            Headless = this._Options.Headless,
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        if (this._Browser != null) await this._Browser.DisposeAsync();
        this._Playwrite?.Dispose();
        Parallel.ForEach(this.SampleSites.Values, sampleSite => sampleSite.Stop());
    }
}
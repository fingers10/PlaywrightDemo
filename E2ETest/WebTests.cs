namespace E2ETest;

public class WebTests
{
    [Test]
    public async Task Navigation_Test()
    {
        var context = TestContext.Instance;
        var host = await context.StartHostAsync(HostingModel.Wasm70);

        // Navigate to Home
        var page = await context.GetPageAsync();
        await page.GotoAndWaitForReadyAsync(host.GetUrl());
        await page.AssertUrlIsAsync(host.GetUrl("/"));
    }
}
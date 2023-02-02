using Applitools.Selenium;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Drawing;

namespace Applitools.Example.Tests;

/// <summary>
/// Tests for the ACME Bank demo app.
/// </summary>
[TestClass]
public class AcmeBankTest
{
    #pragma warning disable CS8618

    // Context for the current test (set by MSTest during runtime)
    public TestContext TestContext { get; set; }

    // Test control inputs to read once and share for all tests
    private static string? ApplitoolsApiKey;
    private static bool Headless;

    // Applitools objects to share for all tests
    private static BatchInfo Batch;
    private static Configuration Config;
    private static ClassicRunner Runner;

    // Test-specific objects
    private WebDriver Driver;
    private Eyes Eyes;
    
    #pragma warning restore CS8618

    /// <summary>
    /// Sets up the configuration for running visual tests locally using the classic runner.
    /// The configuration is shared by all tests in a test suite, so it belongs in a one-time method.
    /// If you have more than one test class, then you should abstract this configuration to avoid duplication.
    /// <summary>
    [AssemblyInitialize]
    public static void SetUpConfigAndRunner(TestContext context)
    {
        // Read the Applitools API key from an environment variable.
        ApplitoolsApiKey = Environment.GetEnvironmentVariable("APPLITOOLS_API_KEY");

        // Read the headless mode setting from an environment variable.
        // Use headless mode for Continuous Integration (CI) execution.
        // Use headed mode for local development.
        Headless = Environment.GetEnvironmentVariable("HEADLESS")?.ToLower() == "true";

        // Create the runner for the classic runner.
        Runner = new ClassicRunner();

        // Create a new batch for tests.
        // A batch is the collection of visual checkpoints for a test suite.
        // Batches are displayed in the Eyes Test Manager, so use meaningful names.
        Batch = new BatchInfo("Example: Selenium C# MSTest with the Classic runner");

        // Create a configuration for Applitools Eyes.
        Config = new Configuration();

        // Set the Applitools API key so test results are uploaded to your account.
        // If you don't explicitly set the API key with this call,
        // then the SDK will automatically read the `APPLITOOLS_API_KEY` environment variable to fetch it.
        Config.SetApiKey(ApplitoolsApiKey);

        // Set the batch for the config.
        Config.SetBatch(Batch);
    }

    /// <summary>
    /// Sets up each test with its own ChromeDriver and Applitools Eyes objects.
    /// <summary>
    [TestInitialize]
    public void OpenBrowserAndEyes()
    {
        // Open the browser with the ChromeDriver instance.
        ChromeOptions options = new ChromeOptions();
        if (Headless) options.AddArgument("headless");
        Driver = new ChromeDriver(options);

        // Set an implicit wait of 10 seconds.
        // For larger projects, use explicit waits for better control.
        // https://www.selenium.dev/documentation/webdriver/waits/
        // The following call works for Selenium 4:
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

        // Create the Applitools Eyes object connected to the VisualGridRunner and set its configuration.
        Eyes = new Eyes(Runner);
        Eyes.SetConfiguration(Config);
        Eyes.SaveNewTests = true;

        // Open Eyes to start visual testing.
        // It is a recommended practice to set all four inputs:
        Eyes.Open(
            
            // WebDriver object to "watch".
            Driver,
            
            // The name of the application under test.
            // All tests for the same app should share the same app name.
            // Set this name wisely: Applitools features rely on a shared app name across tests.
            "ACME Bank Web App",
            
            // The name of the test case for the given application.
            // Additional unique characteristics of the test may also be specified as part of the test name,
            // such as localization information ("Home Page - EN") or different user permissions ("Login by admin").
            TestContext.TestName,
            
            // The viewport size for the local browser.
            // Eyes will resize the web browser to match the requested viewport size.
            // This parameter is optional but encouraged in order to produce consistent results.
            new Size(1024, 768));
    }

    /// <summary>
    /// This test covers login for the Applitools demo site, which is a dummy banking app.
    /// The interactions use typical Selenium WebDriver calls,
    /// but the verifications use one-line snapshot calls with Applitools Eyes.
    /// If the page ever changes, then Applitools will detect the changes and highlight them in the Eyes Test Manager.
    /// Traditional assertions that scrape the page for text values are not needed here.
    /// <summary>
    [TestMethod]
    public void LogIntoBankAccount()
    {
        // Load the login page.
        Driver.Navigate().GoToUrl("https://demo.applitools.com");

        // Verify the full login page loaded correctly.
        Eyes.Check(Target.Window().Fully().WithName("Login page"));

        // Perform login.
        Driver.FindElement(By.Id("username")).SendKeys("applibot");
        Driver.FindElement(By.Id("password")).SendKeys("I<3VisualTests");
        Driver.FindElement(By.Id("log-in")).Click();

        // Verify the full main page loaded correctly.
        // This snapshot uses LAYOUT match level to avoid differences in closing time text.
        Eyes.Check(Target.Window().Fully().WithName("Main page").Layout());
    }

    /// <summary>
    /// Concludes the test by quitting the browser and closing Eyes.
    /// <summary>
    [TestCleanup]
    public void CleanUpTest()
    {
        // Close Eyes to tell the server it should display the results.
        Eyes.CloseAsync();

        // Quit the WebDriver instance.
        Driver.Quit();

        // Warning: `Eyes.CloseAsync()` will NOT wait for visual checkpoints to complete.
        // You will need to check the Eyes Test Manager for visual results per checkpoint.
        // Note that "unresolved" and "failed" visual checkpoints will not cause the test to fail.

        // If you want the test to wait synchronously for all checkpoints to complete, then use `Eyes.Close()`.
        // If any checkpoints are unresolved or failed, then `Eyes.Close()` will make the test fail.
    }

    /// <summary>
    /// Prints the final summary report for the test suite.
    /// <summary>
    [AssemblyCleanup]
    public static void PrintResults()
    {
        // Close the batch and report visual differences to the console.
        // Note that it forces MSTest to wait synchronously for all visual checkpoints to complete.
        TestResultsSummary allTestResults = Runner.GetAllTestResults();
        Console.WriteLine(allTestResults);
    }
}

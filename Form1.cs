using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Reflection;

namespace AssistenciasDesktop;

public partial class Form1 : Form
{
    private const string AppHost = "assistencias.local";
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };

    public Form1()
    {
        InitializeComponent();
        Controls.Add(_webView);
        Shown += InitializePanel;
    }

    private async void InitializePanel(object? sender, EventArgs e)
    {
        Shown -= InitializePanel;
        try
        {
            var appFolder = AppContext.BaseDirectory;
            var panelFolder = Path.Combine(appFolder, "Painel");
            ExtractPanelResources(panelFolder);

            var dataFolder = Path.Combine(appFolder, "Dados", "PerfilWebView2");
            Directory.CreateDirectory(dataFolder);
            var environment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: dataFolder);
            await _webView.EnsureCoreWebView2Async(environment);

            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                AppHost, panelFolder, CoreWebView2HostResourceAccessKind.DenyCors);
            _webView.CoreWebView2.NavigationStarting += RestrictNavigation;
            _webView.CoreWebView2.DocumentTitleChanged += (_, _) =>
            {
                var title = _webView.CoreWebView2.DocumentTitle;
                Text = string.IsNullOrWhiteSpace(title) ? "Controle de Assistências" : title;
            };
            _webView.Source = new Uri($"https://{AppHost}/report.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível abrir o painel desktop.\n\n" +
                "Verifique se o Microsoft Edge WebView2 Runtime está disponível no Windows.\n\n" + ex.Message,
                "Falha ao iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private static void ExtractPanelResources(string panelFolder)
    {
        var resources = new Dictionary<string, string>
        {
            ["AssistenciasDesktop.Panel.report.html"] = "report.html",
            ["AssistenciasDesktop.Panel.vendor.chart.umd.js"] = Path.Combine("vendor", "chart.umd.js"),
            ["AssistenciasDesktop.Panel.vendor.xlsx.full.min.js"] = Path.Combine("vendor", "xlsx.full.min.js"),
            ["AssistenciasDesktop.Panel.vendor.pptxgen.bundle.js"] = Path.Combine("vendor", "pptxgen.bundle.js"),
            ["AssistenciasDesktop.Panel.licenses.Chart.js-LICENSE.txt"] = Path.Combine("licenses", "Chart.js-LICENSE.txt"),
            ["AssistenciasDesktop.Panel.licenses.SheetJS-LICENSE.txt"] = Path.Combine("licenses", "SheetJS-LICENSE.txt"),
            ["AssistenciasDesktop.Panel.licenses.PptxGenJS-LICENSE.txt"] = Path.Combine("licenses", "PptxGenJS-LICENSE.txt")
        };

        foreach (var (resourceName, relativePath) in resources)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                ?? throw new InvalidDataException($"Recurso ausente no aplicativo: {resourceName}");
            var destination = Path.Combine(panelFolder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            using var output = File.Create(destination);
            stream.CopyTo(output);
        }
    }

    private static void RestrictNavigation(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !uri.Host.Equals(AppHost, StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
        }
    }
}

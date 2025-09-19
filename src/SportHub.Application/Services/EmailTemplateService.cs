using System.Globalization;
using System.Net;
using System.Reflection;
using SportHub.Application.Common.Interfaces.Email;

namespace SportHub.Application.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly Dictionary<string, string> _templateCache = new();
    private static readonly object _lock = new();

    public async Task<string> GetTemplateAsync(string templateName, CancellationToken cancellationToken = default)
    {
        var cacheKey = templateName.ToLowerInvariant();
        
        if (_templateCache.ContainsKey(cacheKey))
        {
            return _templateCache[cacheKey];
        }

        var template = await LoadTemplateFromFile(templateName, cancellationToken);
        
        lock (_lock)
        {
            if (!_templateCache.ContainsKey(cacheKey))
            {
                _templateCache[cacheKey] = template;
            }
            return _templateCache[cacheKey];
        }
    }

    public async Task<string> RenderTemplateAsync(string templateName, object model, CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(templateName, cancellationToken);
        return RenderTemplate(template, model);
    }

    private async Task<string> LoadTemplateFromFile(string templateName, CancellationToken cancellationToken)
    {
        var template = await TryLoadEmbeddedResource(templateName);
        if (!string.IsNullOrEmpty(template))
        {
            return template;
        }

        return await LoadFromFileSystem(templateName, cancellationToken);
    }

    private async Task<string> TryLoadEmbeddedResource(string templateName)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SportHub.Application.Emails.{templateName}";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return string.Empty;
            
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
        catch
        {
            return string.Empty;
        }
    }

    private async Task<string> LoadFromFileSystem(string templateName, CancellationToken cancellationToken)
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "Emails", templateName),
            
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emails", templateName),
            
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "SportHub.Application", "Emails", templateName),
            
            Path.Combine(Directory.GetCurrentDirectory(), "..", "SportHub.Application", "Emails", templateName)
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return await File.ReadAllTextAsync(fullPath, cancellationToken);
                }
            }
            catch
            {
            }
        }

        throw new FileNotFoundException($"Template '{templateName}' não foi encontrado em nenhum dos caminhos possíveis.");
    }

    private static string RenderTemplate(string template, object model)
    {
        var properties = model.GetType().GetProperties();
        var result = template;

        foreach (var prop in properties)
        {
            var value = prop.GetValue(model);
            var placeholder = $"{{{{{prop.Name}}}}}";
            
            if (value != null)
            {
                var stringValue = value switch
                {
                    string s => WebUtility.HtmlEncode(s),
                    int i => i.ToString(CultureInfo.InvariantCulture),
                    DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                    _ => WebUtility.HtmlEncode(value.ToString() ?? "")
                };
                
                result = result.Replace(placeholder, stringValue);

                if (prop.Name == "VerificationCode" && value is string code)
                {
                    var paddedCode = code.PadLeft(6, '0');
                    for (int i = 0; i < Math.Min(6, paddedCode.Length); i++)
                    {
                        result = result.Replace($"{{{{VerificationCode:{i}}}}}", paddedCode[i].ToString());
                    }
                }
            }
            else
            {
                result = result.Replace(placeholder, "");
            }
        }

        return result;
    }
}
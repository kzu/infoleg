using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Clarius.OpenLaw.Saij;
using Devlooped;

namespace Clarius.OpenLaw;

public class SaijClient(IHttpClientFactory httpFactory, IProgress<string> progress)
{
    const string UrlFormat = "https://www.saij.gob.ar/busqueda?o={0}&p={1}&f=Total|Tipo+de+Documento/Legislación/{2}|Fecha|Organismo|Publicación|Tema|Estado+de+Vigencia/Vigente,+de+alcance+general|Autor|Jurisdicción/Nacional&s=fecha-rango|DESC&v=colapsada";

    static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public async IAsyncEnumerable<JsonObject> EnumerateAsync(string tipo = "Ley", int skip = 0, int take = 25, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        using var http = httpFactory.CreateClient();
        var url = string.Format(CultureInfo.InvariantCulture, UrlFormat, skip, take, tipo);
        var json = await http.GetStringAsync(url, cancellation);

        var jq = await JQ.ExecuteAsync(json, ThisAssembly.Resources.SaijSearch.Text);
        var result = JsonSerializer.Deserialize<SearchResult[]>(jq, options);
        if (result == null)
            yield break;

        foreach (var item in result)
        {
            var id = await JQ.ExecuteAsync(item.Abstract, ".document.metadata.uuid");
            var doc = await http.GetStringAsync("https://www.saij.gob.ar/view-document?guid=" + id);
            var data = JsonNode.Parse(doc)?["data"]?.GetValue<string>();
            if (data is null ||
                JsonNode.Parse(data) is not JsonObject obj)
                // TODO: log warninlg?
                continue;

            yield return obj;
        }
    }
}
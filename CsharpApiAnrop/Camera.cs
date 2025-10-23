   using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CsharpApiAnrop;

internal sealed record Camera(
    string? Id,
    string? Name,
    string? Type,
    bool Active,
    int[] Counties,
    string? PhotoUrl,
    bool HasFullSizePhoto,
    bool HasSketchImage,
    DateTime? ModifiedTime,
    string? Wgs84Wkt)
{
    private static readonly Regex PointRegex = new(
        @"POINT\s*\(\s*(?<lon>-?\d+(\.\d+)?)\s+(?<lat>-?\d+(\.\d+)?)\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public (double lon, double lat)? Coordinates
    {
        get
        {
            if (Wgs84Wkt is null) return null;
            var m = PointRegex.Match(Wgs84Wkt);
            if (!m.Success) return null;
            if (!double.TryParse(m.Groups["lon"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) ||
                !double.TryParse(m.Groups["lat"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat))
                return null;
            return (lon, lat);
        }
    }

    public static List<Camera> ParseFromXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        var list = new List<Camera>();
        foreach (var e in doc.Descendants("Camera"))
        {
            list.Add(new Camera(
                Id: GetString(e, "Id"),
                Name: GetString(e, "Name"),
                Type: GetString(e, "Type"),
                Active: GetBool(e, "Active"),
                Counties: GetIntArray(e, "CountyNo").ToArray(),
                PhotoUrl: GetString(e, "PhotoUrl"),
                HasFullSizePhoto: GetBool(e, "HasFullSizePhoto"),
                HasSketchImage: GetBool(e, "HasSketchImage"),
                ModifiedTime: GetDateTime(e, "ModifiedTime"),
                Wgs84Wkt: e.Element("Geometry")?.Element("WGS84")?.Value?.Trim()
            ));
        }
        return list;
    }

    private static string? GetString(XElement parent, string name) => parent.Element(name)?.Value?.Trim();
    private static bool GetBool(XElement parent, string name) => string.Equals(parent.Element(name)?.Value?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
    private static DateTime? GetDateTime(XElement parent, string name) => DateTime.TryParse(parent.Element(name)?.Value, out var dt) ? dt : null;

    private static IEnumerable<int> GetIntArray(XElement parent, string name)
    {
        var elems = parent.Elements(name).ToList();
        if (elems.Count > 1)
        {
            foreach (var el in elems)
                if (int.TryParse(el.Value, out var v)) yield return v;
        }
        else
        {
            var single = parent.Element(name)?.Value;
            if (!string.IsNullOrWhiteSpace(single))
            {
                foreach (var part in single.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
                    if (int.TryParse(part, out var v)) yield return v;
            }
        }
    }
}
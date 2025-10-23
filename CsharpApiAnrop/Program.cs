using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using CsharpApiAnrop;

namespace CsharpApiAnrop;

internal class Program
{
    private const string ApiKey = "demokey"; // Din API-nyckel (byt till riktig om du har)
    private static readonly Uri Endpoint = new("https://api.trafikinfo.trafikverket.se/v2/data.xml"); // Trafikverkets endpoint
    private static readonly HttpClient Http = new()
    {
        // User-Agent bara för att visa något eget i loggar
        DefaultRequestHeaders = { { "User-Agent", "SimpleCameraClient/1.0" } }
    };

    private static async Task Main(string[] args)
    {
        // 1) Anropa API för kameror
        Console.WriteLine("--- Trafikverket Cameras ---");
        int camerasToDisplay = 5; // Hur många kameror vi skriver ut

        // Bygger XML-förfrågan med vilka fält vi vill ha (INCLUDE) till vilken objektstyp och schemaversion och antalet träffar (utelämna limitToSearch eller sätt till -1 för att inte begränsa)
        var xmlRequest = BuildRequestXml(
            includes:
            [
                "Id","Name","Type","Active","CountyNo"
            ],
            objType: "Camera",
            schemaVersion: "1",
            limitToSearch: camerasToDisplay // Begränsar antal kameror i svaret om man vill visa alla kan man utelämna denna parameter
        );

        //string xml;
        try
        {
            // Skickar POST med XML till API:t
            using var content = new StringContent(xmlRequest, Encoding.UTF8, "text/xml");
            using var resp = await Http.PostAsync(Endpoint, content);
            if (!resp.IsSuccessStatusCode)
            {
                // Om fel, skriv status och ev. felmeddelande från servern
                Console.WriteLine($"HTTP {(int)resp.StatusCode} {resp.StatusCode}");
                Console.WriteLine(await resp.Content.ReadAsStringAsync());
                return;
            }
            // Läser svar som text (XML)
            string xml = await resp.Content.ReadAsStringAsync();
            var xdoc = XDocument.Parse(xml);
            Console.WriteLine(xdoc);
        }
        catch (Exception ex)
        {
            // Om nätverksfel eller liknande
            Console.WriteLine("Request failed: " + ex.Message);
            return;
        }
        Console.WriteLine("\n\n\n\n\n\n");




        // 2) Anropa Vägnummer med specificerade INCLUDE-fält (begränsad data)
        Console.WriteLine("--- Vägnummer (vägdata) med filter ---");
        try
        {
            int vagnummerToShow = 3; // Begränsar antal rader i svaret
            // Hårdkodad XML-body: vi inkluderar bara vissa fält
            var vagnummerXmlReq = $"<REQUEST>" +
                                          $"<LOGIN authenticationkey='{ApiKey}'/>" +
                                              $"<QUERY objecttype='Vägnummer' namespace='vägdata.nvdb_dk_o' schemaversion='1.2' limit='{vagnummerToShow}'>" +
                                                $"<INCLUDE>Huvudnummer</INCLUDE>" +
                                                $"<INCLUDE>Europaväg</INCLUDE>" +
                                                $"<INCLUDE>Role</INCLUDE>" +
                                                $"<INCLUDE>Länstillhörighet</INCLUDE>" +
                                              $"</QUERY>" +
                                          $"</REQUEST>";
            using var content = new StringContent(vagnummerXmlReq, Encoding.UTF8, "text/xml");
            using var resp = await Http.PostAsync(Endpoint, content);
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP {(int)resp.StatusCode} {resp.StatusCode} (Vägnummer)");
                Console.WriteLine(await resp.Content.ReadAsStringAsync());
            }
            else
            {
                // Skriver ut hela XML-svaret (begränsad data pga INCLUDE)
                var vagnummerXml = await resp.Content.ReadAsStringAsync();
                var xdoc = XDocument.Parse(vagnummerXml);
                Console.WriteLine(xdoc); // XDocument ToString formatterar med indentering
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Vägnummer-anrop misslyckades: " + ex.Message);
        }

        Console.WriteLine("\n---Done---");
        Console.WriteLine("\n\n\n\n\n\n");




        // 3) Samma Vägnummer men utan INCLUDE => mer komplett data
        Console.WriteLine("\n--- Vägnummer (vägdata) Med all data ---");
        try
        {
            int vagnummerToShow2 = 3; // Limit anger hur många objekt API:t ska returnera
            var vagnummerXmlReq2 = $"<REQUEST>" +
                                            $"<LOGIN authenticationkey='{ApiKey}'/>" +
                                                $"<QUERY objecttype='Vägnummer' namespace='vägdata.nvdb_dk_o' schemaversion='1.2' limit='{vagnummerToShow2}'>" +
                                                    $"<FILTER></FILTER>" + // Tom FILTER => inga begränsningar här
                                                $"</QUERY>" +
                                        $"</REQUEST>";
            using var content2 = new StringContent(vagnummerXmlReq2, Encoding.UTF8, "text/xml");
            using var resp2 = await Http.PostAsync(Endpoint, content2);
            if (!resp2.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP {(int)resp2.StatusCode} {resp2.StatusCode} (Vägnummer)");
                Console.WriteLine(await resp2.Content.ReadAsStringAsync());
            }
            else
            {
                // Visar mer komplett XML med alla fält
                var vagnummerXml2 = await resp2.Content.ReadAsStringAsync();
                var xdoc2 = XDocument.Parse(vagnummerXml2);
                Console.WriteLine(xdoc2);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Vägnummer-anrop misslyckades: " + ex.Message);
        }

        Console.WriteLine("\n---Done---");
    }




    // Hjälpmetod som bygger XML för kamera-anropet baserat på vilka fält (INCLUDE) vi vill ha
    private static string BuildRequestXml(IEnumerable<string> includes, string objType, string schemaVersion, int limitToSearch = -1 )
    {
        string limitString = (limitToSearch == -1) ? "" : $" limit='{limitToSearch}'";
        var sb = new StringBuilder();
        sb.Append("<REQUEST>");
        sb.Append($"<LOGIN authenticationkey='{ApiKey}'/>");
        sb.Append($"<QUERY objecttype='{objType}' schemaversion='{schemaVersion}'{limitString}>");
        foreach (var inc in includes)
            sb.Append($"<INCLUDE>{inc}</INCLUDE>");
        sb.Append("</QUERY></REQUEST>");
        return sb.ToString();
    }
}
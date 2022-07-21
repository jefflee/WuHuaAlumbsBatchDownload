using HtmlAgilityPack;

Console.WriteLine("Hello, World!");
string outputRootFolder = "C:\\Users\\JeffL\\Desktop\\20220719\\temp";

for (int pageNum = 1; pageNum <= 6; pageNum++)
{
    string alubmListPage = $"https://class.tn.edu.tw/modules/tad_web/action.php?WebID=6466&op=&CateID=&g2p={pageNum}";


    await HandleAulbumPage(alubmListPage, pageNum);
}

Console.WriteLine("Success");

async Task HandleAulbumPage(string albumListUrl, int pageNum)
{
    HtmlWeb web = new HtmlWeb();
    HtmlDocument document = web.Load(albumListUrl);
    var node = document.DocumentNode.Descendants("div")
        .Select(y => y.Descendants()
            .Where(x => x.Id == "web_center_block"))
        .First().FirstOrDefault();

    var aNodes = node.SelectNodes(".//a").Where(
        x => x.Attributes.Count == 1
             && x.Attributes["href"].Value.StartsWith("action.php")
             && x.InnerHtml.Contains('<') == false).ToList();

    int k = 0;
    foreach (var link in aNodes)
    {
        k++;
        Console.WriteLine($"Handle Album {k}");
        string targetAlbumUrl = "https://class.tn.edu.tw/modules/tad_web/" + link.Attributes["href"].Value;
        var aLinks = await DownloadAlbum(targetAlbumUrl, $"{pageNum}-{k}_{link.InnerText}");
    }
}

async Task<IEnumerable<string>> DownloadAlbum(string albumUrl1, string albumName)
{
    Console.WriteLine("Downloading Album...");
    HtmlWeb web = new HtmlWeb();
    HtmlDocument document = web.Load(albumUrl1);
    var node = document.DocumentNode.Descendants("div")
        .Select(y => y.Descendants()
            .Where(x => x.Id == "web_center_block"))
        .First().FirstOrDefault();

    var aLinksNodes = node?.Descendants("a").ToList();
    var enumerable = aLinksNodes?
        .Where(k => k.Attributes["href"].Value.StartsWith("htt"))
        .Select(k => k.Attributes["href"].Value);

    // Create album
    string outputFolder1 = Path.Combine(outputRootFolder, albumName);
    if (Directory.Exists(outputFolder1) == false)
    {
        Directory.CreateDirectory(outputFolder1);
    }

    await DownloadPictures(enumerable, outputFolder1);
    return enumerable;
}

async Task DownloadPictures(IEnumerable<string> links, string outputFolder)
{
    HttpClient httpClient = new HttpClient();
    int k = 1;
    foreach (var url in links)
    {
        byte[] fileBytes = await httpClient.GetByteArrayAsync(url);
        string[] tokens = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string filename = tokens.Last();
        File.WriteAllBytes(Path.Combine(outputFolder, filename), fileBytes);
        k++;
    }
}

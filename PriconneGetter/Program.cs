using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using TABBotApp.Libs.Utils;


var baseUrl = "https://priconne_redive.rwiki.jp/%E3%82%AD%E3%83%A3%E3%83%A9/%E4%B8%80%E8%A6%A7";

var htmlText = await HttpHelper.GetRequestAsync(baseUrl);

File.WriteAllText("dump.txt", htmlText);

var regex = new Regex(@"<a href.*?>");
var matches = regex.Matches(htmlText);

var downloadedItems = new HashSet<string>();
Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "out"));
foreach (Match match in matches)
{
    var ahref = match.Value;

    if (!ahref.Contains("title") || !ahref.Contains("キャラ") || !ahref.Contains("★"))
    {
        continue;
    }

    var regex2 = new Regex("^.*title=\"キャラ/.*?/(.*?)\".*$");
    if (!regex2.IsMatch(ahref))
    {
        continue;
    }

    var regex3 = new Regex("^.*href=\"(.*?)\".*$");
    if (!regex3.IsMatch(ahref)) 
    {
        continue;
    }

    var title = regex2.Replace(ahref,"$1" );
    var url = regex3.Replace(ahref,"$1");
    var charText = await HttpHelper.GetRequestAsync(url);

    // File.WriteAllText($"{title}.txt", charText);

    Console.WriteLine(title);


    var regex4 = new Regex(@"https://priconne_redive\.rwiki\.jp/attach/priconne_redive/.*?\.png");
    var images= regex4.Matches(charText).Select(e =>e.Value ).ToHashSet().OrderBy(e => e);
    foreach(var(imgUrl, index)in images.Select((e, i) => (e, i)))
    {
        try
        {
            if (downloadedItems.Contains(imgUrl)) 
            {
                continue;
            }

            downloadedItems.Add(imgUrl);
            var data = await HttpHelper.GetRequestByteArrayAsync(imgUrl);
            File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory,"out", $"{title}_{index}.png"), data);

        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}


Console.WriteLine("press any key to exit . . .");
Console.ReadKey(true);  
Environment.Exit(0);

# UTorrentClientApi
UTorrentClient Api is an extensible set of classes that use WebUI to manipulate µTorrent remotely.

## Main features
* CRUD operations
* Add torrent in subpath
* Cache
* Extensibility
* BEncoding Parser

[![Nuget page](http://download-codeplex.sec.s-msft.com/Download?ProjectName=utorrentclientapi&DownloadId=692904)](https://nuget.org/packages/UTorrentClientApi/)

## Get started

### Get all torrents
```c#
UTorrentClient client = new UTorrentClient("admin", "password");
var response = client.GetList();
List<Torrent> torrents = response.Result.Torrents;
```
### Add new torrent from file
```c#
using(var file = new System.IO.File("mytool.torrent"))
{
    UTorrentClient client = new UTorrentClient("admin", "password");
    var response = client.PostTorrent(file, "tools");
    var torrent = response.AddedTorrent;
}
```
### Send command
```c#
UTorrentClient client = new UTorrentClient("admin", "password");
string torrentId = "2D5B29C752CA5286D3B347591E7D08EAA13109CE";
client.StartTorrent(torrentId);
client.StopTorrent(torrentId);
client.PauseTorrent(torrentId);
client.RecheckTorrent(torrentId);
client.DeleteTorrent(torrentId);

List<string> torrentIds = new List<string>() { /* ... */ };
client.StopTorrent(torrentIds);
```

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using UTorrent.Api.Data;

namespace UTorrent.Api.UnitTest
{
    using System.IO;
    using System.Linq;

    [TestClass]
    public class UTorrentClientUnitTest
    {
#if !PORTABLE
        [TestMethod]
        public void TestClientConfiguration()
        {
            var exception = Assert.ThrowsException<InvalidOperationException>(() => new UTorrentClient());
            Assert.IsNotNull(exception);
            Assert.AreEqual("UTORRENT.LOGIN and UTORRENT.PASSWORD configuration key not found.", exception.Message);
        }
#endif

        [TestMethod]
        public void TestGetToken()
        {
            try
            {
                UTorrentClient client = new UTorrentClient("admin", "password");
                var token = client.TestGetToken();
                Assert.IsNotNull(token, "Invalid null token");
                Assert.AreNotEqual(0, token.Length, "Invalid empty token");
            }
            catch (ServerUnavailableException)
            {
                Assert.Inconclusive("Serveur unavailable");
            }
            catch (InvalidCredentialException)
            {
                Assert.Inconclusive("Invalid credential");
            }
        }

        [TestMethod]
        public void TestGetAllTorrent()
        {
            try
            {
                UTorrentClient client = new UTorrentClient("admin", "password");
                var response = client.GetList();
                Assert.IsNull(response.Error);
                Assert.IsNotNull(response.Result);
                Assert.IsNotNull(response.Result.Torrents);
            }
            catch (ServerUnavailableException)
            {
                Assert.Inconclusive("Serveur unavailable");
            }
            catch (InvalidCredentialException)
            {
                Assert.Inconclusive("Invalid credential");
            }
        }

        [TestMethod]
        public void TestGetAllTorrentAsync()
        {
            UTorrentClient client = new UTorrentClient("admin", "password");
            Task<Response> task = client.GetListAsync();
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(ServerUnavailableException))
                    Assert.Inconclusive("Serveur unavailable");
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(InvalidCredentialException))
                    Assert.Inconclusive("Invalid credential");
            }

            var response = task.Result;
            Assert.IsNull(response.Error);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Torrents);
        }

        [TestMethod]
        public void TestGetTorrentWithFiles()
        {
            try
            {
                UTorrentClient client = new UTorrentClient("admin", "password");
                var response = client.GetList();
                Assert.IsNull(response.Error);
                Assert.IsNotNull(response.Result);
                Assert.IsNotNull(response.Result.Torrents);
                Assert.AreNotEqual(response.Result.Torrents.Count, 0);

                var torrent = response.Result.Torrents[0];

                torrent = UTorrentClient.ConsolidateTorrent(response, torrent.Hash);
                Assert.IsNotNull(torrent);
                Assert.IsNotNull(torrent.Files);
                Assert.AreNotEqual(torrent.Files.Count, 0);

            }
            catch (ServerUnavailableException)
            {
                Assert.Inconclusive("Serveur unavailable");
            }
            catch (InvalidCredentialException)
            {
                Assert.Inconclusive("Invalid credential");
            }
        }

        [TestMethod]
        public void TestGetTorrentWithFilesAsync()
        {
            UTorrentClient client = new UTorrentClient("admin", "password");
            Task<Response> task = client.GetListAsync();
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(ServerUnavailableException))
                    Assert.Inconclusive("Serveur unavailable");
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(InvalidCredentialException))
                    Assert.Inconclusive("Invalid credential");
            }
            var response = task.Result;
            Assert.IsNull(response.Error);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Torrents);
            Assert.AreNotEqual(response.Result.Torrents.Count, 0);

            var torrent = response.Result.Torrents[0];

            Task<Torrent> task2 = client.GetTorrentAsync(torrent.Hash.ToLower());
            try
            {
                task2.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(ServerUnavailableException))
                    Assert.Inconclusive("Serveur unavailable");
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(InvalidCredentialException))
                    Assert.Inconclusive("Invalid credential");
            }
            Assert.IsNull(response.Error);
            Assert.IsNotNull(response.Result);

            torrent = UTorrentClient.ConsolidateTorrent(response, torrent.Hash);
            Assert.IsNotNull(torrent);
            Assert.IsNotNull(torrent.Files);
            Assert.AreNotEqual(torrent.Files.Count, 0);

            var result2 = task2.Result;
            Assert.IsNotNull(result2);
            Assert.IsNotNull(result2.Hash);
        }

        [TestMethod]
        public void TestGetFilesWithNullHash()
        {
            UTorrentClient client = new UTorrentClient("admin", "password");
            Assert.ThrowsException<ArgumentNullException>(() => client.GetFiles(null));
        }

        [TestMethod]
        public void TestGetFilesWithNullHashAsync()
        {
            UTorrentClient client = new UTorrentClient("admin", "password");
            Assert.ThrowsException<ArgumentNullException>(() => client.GetFilesAsync(null));
        }

        [TestMethod]
        public void TestGetFiles()
        {
            try
            {
                UTorrentClient client = new UTorrentClient("admin", "password");
                var response = client.GetList();
                Assert.IsNull(response.Error);
                Assert.IsNotNull(response.Result);
                Assert.IsNotNull(response.Result.Torrents);
                Assert.AreNotEqual(response.Result.Torrents.Count, 0);

                var torrent = response.Result.Torrents[0];
                response = client.GetFiles(torrent.Hash.ToLower());
                Assert.IsNull(response.Error);
                Assert.IsNotNull(response.Result);
                Assert.IsNotNull(response.Result.Files);
                Assert.AreNotEqual(response.Result.Files.Count, 0);
            }
            catch (ServerUnavailableException)
            {
                Assert.Inconclusive("Serveur unavailable");
            }
            catch (InvalidCredentialException)
            {
                Assert.Inconclusive("Invalid credential");
            }
        }

        [TestMethod]
        public void TestGetFilesAsync()
        {
            UTorrentClient client = new UTorrentClient("admin", "password");

            Task<Response> task = client.GetListAsync();
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(ServerUnavailableException))
                    Assert.Inconclusive("Serveur unavailable");
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(InvalidCredentialException))
                    Assert.Inconclusive("Invalid credential");
            }

            var response = task.Result;
            Assert.IsNull(response.Error);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Torrents);
            Assert.AreNotEqual(response.Result.Torrents.Count, 0);

            var torrent = response.Result.Torrents[0];

            task = client.GetFilesAsync(torrent.Hash.ToLower());
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(ServerUnavailableException))
                    Assert.Inconclusive("Serveur unavailable");
                if (ex.InnerExceptions.Count == 1 &&
                    ex.InnerExceptions[0].GetType() == typeof(InvalidCredentialException))
                    Assert.Inconclusive("Invalid credential");
            }
            response = task.Result;
            Assert.IsNull(response.Error);
            Assert.IsNotNull(response.Result);
            Assert.IsNotNull(response.Result.Files);
            Assert.AreNotEqual(response.Result.Files.Count, 0);
        }

        [TestMethod]
        public void TestConsolidateTorrentWithNullResponseParameter()
        {
            Assert.ThrowsException<ArgumentNullException>(() => UTorrentClient.ConsolidateTorrent(null, string.Empty));
        }

        [TestMethod]
        public void TestConsolidateTorrentWithNullHashParameter()
        {
            BaseResponse response = new Response();
            response.Result = new Result(null);
            Assert.ThrowsException<ArgumentNullException>(() => UTorrentClient.ConsolidateTorrent(response, null));
        }

        [TestMethod]
        public void TestConsolidateTorrentWithNullResponseResultParameter()
        {
            BaseResponse response = new Response();
            response.Result = null;
            Assert.ThrowsException<ArgumentNullException>(() => UTorrentClient.ConsolidateTorrent(response, string.Empty));
        }

        [TestMethod]
        public void TestConsolidateTorrentWithNoTorrentInResponseResult()
        {
            BaseResponse response = new Response();
            response.Result = new Result(null);
            Assert.IsNull(UTorrentClient.ConsolidateTorrent(response, string.Empty));
        }

        [TestMethod]
        public void TestProcessRequestWithNullParameter()
        {
            UTorrentClient client = new UTorrentClient("admin", "password");
            Assert.ThrowsException<ArgumentNullException>(() => client.ProcessRequest<Response>(null));
        }

        [TestMethod]
        public void TestAddAndRemoveTorrent()
        {
            try
            {
                var dummyFile =
                    "ZDEwOmNyZWF0ZWQgYnkx" +
                    "Mzp1VG9ycmVudC8zMzAw" +
                    "MTM6Y3JlYXRpb24gZGF0" +
                    "ZWkxMzc1NTU5MjcyZTg6" +
                    "ZW5jb2Rpbmc1OlVURi04" +
                    "NDppbmZvZDY6bGVuZ3Ro" +
                    "aTVlNDpuYW1lOTpkdW1t" +
                    "eS50eHQxMjpwaWVjZSBs" +
                    "ZW5ndGhpMTYzODRlNjpw" +
                    "aWVjZXMyMDqCnDgEQBsH" +
                    "J/cPc9RBXhYkAMvlezc6" +
                    "cHJpdmF0ZWkxZWVl";

                var bytes = Convert.FromBase64String(dummyFile);
                UTorrentClient client = new UTorrentClient("admin", "password");
                using (var file = new MemoryStream(bytes))
                {
                    var addResponse = client.PostTorrent(file);
                    Assert.IsNotNull(addResponse);
                    Assert.IsNotNull(addResponse.Result);
                    Assert.IsNull(addResponse.Result.Error);
                    Assert.IsNotNull(addResponse.AddedTorrent);
                    Torrent torrent = addResponse.AddedTorrent;

                    var deleteResponse = client.DeleteTorrent(torrent.Hash);
                    Assert.IsNotNull(deleteResponse);
                    Assert.IsNotNull(deleteResponse.Result);
                    Assert.IsNull(deleteResponse.Result.Error);
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ServerUnavailableException || ex.InnerException is InvalidCredentialException)
                    Assert.Inconclusive("Serveur unavailable");
                throw;
            }
            catch (ServerUnavailableException)
            {
                Assert.Inconclusive("Serveur unavailable");
            }
            catch (InvalidCredentialException)
            {
                Assert.Inconclusive("Invalid credential");
            }
        }

        [TestMethod]
        public void TestGetSettings()
        {
            try
            {
                UTorrentClient client = new UTorrentClient("admin", "password");
                var response = client.GetSettings();

                Assert.IsNull(response.Error);
                Assert.IsNotNull(response.Result);
                Assert.IsNotNull(response.Result.Settings);
                Assert.AreNotEqual(response.Result.Settings.Count, 0);

                var webuiEnableSetting = response.Result.Settings.FirstOrDefault(s => s.Key == "webui.enable");
                Assert.IsNotNull(webuiEnableSetting);
                Assert.AreEqual(SettingType.Integer, webuiEnableSetting.Type);
                Assert.AreEqual(1, webuiEnableSetting.Value);
                Assert.AreEqual("Y", webuiEnableSetting.Access);
            }
            catch (ServerUnavailableException)
            {
                Assert.Inconclusive("Serveur unavailable");
            }
        }

        [TestMethod]
        public void TestSetSettings()
        {
            try
            {
                UTorrentClient client = new UTorrentClient("admin", "password");
                var response = client.GetSettings();

                Assert.IsNull(response.Error);
                Assert.IsNotNull(response.Result);
                Assert.IsNotNull(response.Result.Settings);
                Assert.AreNotEqual(response.Result.Settings.Count, 0);

                var searchListSetting = response.Result.Settings.FirstOrDefault(s => s.Key == "search_list");
                Assert.IsNotNull(searchListSetting);
                Assert.AreEqual(SettingType.String, searchListSetting.Type);
                Assert.AreEqual("Y", searchListSetting.Access);

                string value = searchListSetting.Value.ToString();

                response = client.SetSetting("search_list", value + "\r\nUniut test|http://localhost?q=");
                Assert.IsNull(response.Error);

                response = client.GetSettings();
                Assert.IsNull(response.Error);
                Assert.IsNotNull(response.Result);
                Assert.IsNotNull(response.Result.Settings);
                Assert.AreNotEqual(response.Result.Settings.Count, 0);

                searchListSetting = response.Result.Settings.FirstOrDefault(s => s.Key == "search_list");
                Assert.IsNotNull(searchListSetting);
                Assert.AreEqual(SettingType.String, searchListSetting.Type);
                Assert.AreEqual("Y", searchListSetting.Access);

                string newValue = searchListSetting.Value.ToString();

                Assert.AreEqual(value + "\r\nUniut test|http://localhost?q=", newValue);

                var resp = client.SetSetting("search_list", value);

            }
            catch (ServerUnavailableException)
            {
                Assert.Inconclusive("Serveur unavailable");
            }
        }
    }
}

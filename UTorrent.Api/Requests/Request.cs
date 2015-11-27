using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UTorrent.Api
{
    public class Request : BaseRequest<Response>
    {
        #region Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly UrlActionCollection AuthorizedList = new UrlActionCollection(GenerateAuthorizedUrl());

        #endregion

        /// <summary>
        /// Fuck yeah!
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<UrlAction> GenerateAuthorizedUrl()
        {
            yield return UrlAction.Default;
            yield return UrlAction.Start;
            yield return UrlAction.Stop;
            yield return UrlAction.Pause;
            yield return UrlAction.ForceStart;
            yield return UrlAction.Unpause;
            yield return UrlAction.Recheck;
            yield return UrlAction.Remove;
            yield return UrlAction.RemoveData;
            yield return UrlAction.RemoveDataTorrent;
            yield return UrlAction.RemoveTorrent;
            yield return UrlAction.SetPriority;
            yield return UrlAction.AddUrl;
            yield return UrlAction.AddFile;
            yield return UrlAction.GetFiles;
            yield return UrlAction.GetSettings;
            yield return UrlAction.SetSetting;
        }

        protected override void ToUrl(StringBuilder sb)
        {
        }

        protected override void OnProcessingRequest(System.Net.HttpWebRequest wr)
        {
        }

        protected override void OnProcessedRequest(Response result)
        {
        }

        protected override bool CheckAction(UrlAction action)
        {
            return (AuthorizedList.Any(a => a.Equals(action)));
        }
    }
}

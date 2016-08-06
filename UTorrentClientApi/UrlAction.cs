using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace UTorrent.Api
{
    public sealed class UrlAction : IEquatable<UrlAction>
    {
        [ContractInvariantMethod]
        void Invariants()
        {
            Contract.Invariant(this._actionValue != null);
        }

        #region Default Instance

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction Default           = new UrlAction();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction Start             = new UrlAction("START");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction Stop              = new UrlAction("STOP");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction Pause             = new UrlAction("PAUSE");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction ForceStart        = new UrlAction("FORCESTART");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction Unpause           = new UrlAction("UNPAUSE");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction Recheck           = new UrlAction("RECHECK");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction Remove            = new UrlAction("REMOVE");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction RemoveData        = new UrlAction("REMOVEDATA");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction RemoveDataTorrent = new UrlAction("REMOVEDATATORRENT");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction RemoveTorrent = new UrlAction("REMOVETORRENT");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction SetPriority       = new UrlAction("SETPRIO");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction AddUrl            = new UrlAction("ADD-URL");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction AddFile           = new UrlAction("ADD-FILE");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction GetFiles          = new UrlAction("GETFILES");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction GetSettings       = new UrlAction("GETSETTINGS");
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "UrlAction is immutable")]
        public static readonly UrlAction SetSetting        = new UrlAction("SETSETTING");
        //public static readonly UrlAction SetProps        = new UrlAction( );


        #endregion

        #region Properties

        private readonly string _actionValue;
        public string ActionValue
        {
            get { return _actionValue; }
        }

        #endregion

        private UrlAction()
        {
            _actionValue = string.Empty;
        }

        private UrlAction(string actionValue)
        {
            Contract.Requires(actionValue != null);
            _actionValue = actionValue;
        }

        public static UrlAction Create(string actionValue)
        {
            if (actionValue == null)
                throw new ArgumentNullException("actionValue");

            actionValue = actionValue.Trim();
            if (actionValue.Length == 0)
                throw new ArgumentException("actionValue is empty", "actionValue");

            actionValue = actionValue.ToUpperInvariant();
            return new UrlAction(actionValue);
        }

        #region Interfaces

        public bool Equals(UrlAction other)
        {
            if (other == null) return false;
            return _actionValue.Equals(other._actionValue);
        }

        #endregion
    }

    public class UrlActionCollection : IEnumerable<UrlAction>
    {
        [ContractInvariantMethod]
        void Invariants()
        {
            Contract.Invariant(this.InnerList != null);
        }
        private List<UrlAction> InnerList { get; set; }

        public UrlActionCollection(IEnumerable<UrlAction> source)
        {
            InnerList = new List<UrlAction>(source);
        }

        public IEnumerator<UrlAction> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    //public enum UrlAction2
    //{
    //    Default,
    //    Start,
    //    Stop,
    //    Pause,
    //    ForceStart,
    //    Unpause,
    //    Recheck,
    //    Remove,
    //    RemoveData,
    //    RemoveDataTorrent,
    //    SetPriority,
    //    AddUrl,
    //    AddFile,
    //    GetFiles,
    //    GetSettings,
    //    SetSetting,
    //    SetProps,
    //}
}

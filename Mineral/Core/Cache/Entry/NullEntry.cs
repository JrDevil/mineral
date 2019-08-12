﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Cache.Entry
{
    public class NullEntry<TKey, TValue> : IReferenceEntry<TKey, TValue>
    {
        #region Field
        #endregion


        #region Property
        public TKey Key
        {
            get { return default(TKey); }
        }
        public int Hash
        {
            get { return 0; }
        }

        public IReferenceEntry<TKey, TValue> Next
        {
            get { return null; }
        }

        public long AccessTime
        {
            get { return 0; }
            set { }
        }

        public long WriteTime
        {
            get { return 0; }
            set { }
        }

        public IValueReference<TKey, TValue> ValueReference
        {
            get { return null; }
            set { }
        }

        public IReferenceEntry<TKey, TValue> PrevInAccessQueue
        {
            get { return null; }
            set { }
        }

        public IReferenceEntry<TKey, TValue> PrevInWriteQueue
        {
            get { return null; }
            set { }
        }

        public IReferenceEntry<TKey, TValue> NextInAccessQueue
        {
            get { return null; }
            set { }
        }

        public IReferenceEntry<TKey, TValue> NextInWriteQueue
        {
            get { return null; }
            set { }
        }

        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}

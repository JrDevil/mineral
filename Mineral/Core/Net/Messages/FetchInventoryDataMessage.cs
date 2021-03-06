﻿using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Protocol;
using static Protocol.Inventory.Types;

namespace Mineral.Core.Net.Messages
{
    public class FetchInventoryDataMessage : InventoryMessage
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        public FetchInventoryDataMessage(byte[] packed)
            : base(packed)
        {
            this.type = (byte)MessageTypes.MsgType.FETCH_INV_DATA;
        }

        public FetchInventoryDataMessage(Inventory inventory)
            : base(inventory)
        {
            this.type = (byte)MessageTypes.MsgType.FETCH_INV_DATA;
        }

        public FetchInventoryDataMessage(List<SHA256Hash> hashes, InventoryType type)
            : base(hashes, type)
        {
            this.type = (byte)MessageTypes.MsgType.FETCH_INV_DATA;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}

﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public class WallType : HostPartitionType
    {
        public WallType(WallType wallType)
            : base(wallType)
        {

        }

        public WallType(JObject jObject)
            : base(jObject)
        {

        }

        public WallType(string name)
            : base(name)
        {

        }

        public WallType(System.Guid guid, string name)
            : base(guid, name)
        {

        }

        public WallType(string name, IEnumerable<MaterialLayer> materialLayers)
            : base(name, materialLayers)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
                return jObject;

            return jObject;
        }

    }
}

﻿using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public abstract class HostPartition<T> : BuildingElement<T>, IHostPartition where T: HostPartitionType
    {
        private List<IOpening> openings;
        
        public HostPartition(HostPartition<T> hostPartition)
            : base(hostPartition)
        {

        }

        public HostPartition(JObject jObject)
            : base(jObject)
        {

        }

        public HostPartition(T hostPartitionType, Face3D face3D)
            : base(hostPartitionType, face3D)
        {

        }

        public HostPartition(Guid guid, T hostPartitionType, Face3D face3D)
            : base(guid, hostPartitionType, face3D)
        {

        }

        public List<IOpening> Openings
        {
            get
            {
                if (openings == null)
                    return null;

                return openings.ConvertAll(x => Core.Query.Clone(x));
            }
        }

        public bool AddOpening(IOpening opening)
        {
            if (opening == null)
                return false;

            if (!Query.IsValid(this, opening))
                return false;

            if (openings == null)
                openings = new List<IOpening>();

            openings.Add(opening);
            return true;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }


            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

    }
}

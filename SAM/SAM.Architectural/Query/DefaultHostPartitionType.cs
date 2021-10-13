﻿using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static HostPartitionType DefaultHostPartitionType(Vector3D normal, double tolerance = Core.Tolerance.Angle)
        {
            if (normal == null)
                return null;
            HostPartitionCategory hostPartitionCategory = HostPartitionCategory(normal, tolerance);
            switch (hostPartitionCategory)
            {
                case Architectural.HostPartitionCategory.Floor:
                    return new FloorType(new System.Guid("e04f38f4-c443-4cb5-bde8-254b2f27905b"), "Default Floor");

                case Architectural.HostPartitionCategory.Roof:
                    return new RoofType(new System.Guid("a7b6b657-6421-43a8-bf89-f254a450589d"), "Default Roof");

                case Architectural.HostPartitionCategory.Wall:
                    return new FloorType(new System.Guid("b58d4224-c0b0-4cc0-8622-ff5f1f838d03"), "Default Wall");

                default:
                    return null;
            }
        }

        public static HostPartitionType DefaultHostPartitionType(Face3D face3D, double tolerance = Core.Tolerance.Angle)
        {
            return DefaultHostPartitionType(face3D?.GetPlane()?.Normal, tolerance);
        }

        public static T DefaultHostPartitionType<T>(this PartitionAnalyticalType partitionAnalyticalType) where T: HostPartitionType
        {
            switch(partitionAnalyticalType)
            {
                case Architectural.PartitionAnalyticalType.Undefined:
                    return null;

                case Architectural.PartitionAnalyticalType.Air:
                    return null;

                case Architectural.PartitionAnalyticalType.Shade:
                    return null;

                case Architectural.PartitionAnalyticalType.Roof:
                    return new RoofType(new System.Guid("a7b6b657-6421-43a8-bf89-f254a450589d"), "Default Roof") as T;

                case Architectural.PartitionAnalyticalType.CurtainWall:
                    return new WallType(new System.Guid("e39aa6f3-14d8-42fa-849f-63881b6ab6c6"), "Default Curtain Wall") as T;

                case Architectural.PartitionAnalyticalType.InternalWall:
                    return new WallType(new System.Guid("0b579929-9390-42a3-820c-9e071b9d5ae0"), "Default Internal Wall") as T;

                case Architectural.PartitionAnalyticalType.ExternalWall:
                    return new WallType(new System.Guid("ad1457e6-f44c-425f-ad41-987cff9e3135"), "Default External Wall") as T;

                case Architectural.PartitionAnalyticalType.UndergroundWall:
                    return new WallType(new System.Guid("370112f4-8acf-401e-afb1-ba411a5233a0"), "Default Underground Wall") as T;

                case Architectural.PartitionAnalyticalType.InternalFloor:
                    return new FloorType(new System.Guid("cf3e3abd-0c42-4e78-a906-26bf2bc925ee"), "Default Internal Floor") as T;

                case Architectural.PartitionAnalyticalType.ExternalFloor:
                    return new FloorType(new System.Guid("98b356f0-afdd-426d-82d7-38695028a360"), "Default External Floor") as T;

                case Architectural.PartitionAnalyticalType.UndergroundFloor:
                    return new FloorType(new System.Guid("99d42f75-2616-4adc-b2ea-ac85e43e2b3c"), "Default Underground Floor") as T;

                case Architectural.PartitionAnalyticalType.UndergroundCeiling:
                    return new FloorType(new System.Guid("93394f3d-daf6-410e-99d1-8caa1d588a9b"), "Default Underground Ceiling") as T;

            }

            return null;
        }
    }
}
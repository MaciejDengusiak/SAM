﻿namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Vector3D AxisY(this Vector3D normal)
        {
            if (normal == null)
                return null;

            return AxisX(normal).CrossProduct(normal);

            if (normal.X == 0 && normal.Y == 0)
                return new Vector3D(0, 1, 0);

            return new Vector3D(-normal.Y, normal.X, 0).Unit;
        }
    }
}
﻿using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double LightTransmittance(this TransparentMaterial transparentMaterial)
        {
            if (transparentMaterial == null)
                return double.NaN;

            double result = double.NaN;
            if (!Core.Query.TryGetValue(transparentMaterial, ParameterName_LightTransmittance(), out result))
                return double.NaN;

            return result;
        }
    }
}
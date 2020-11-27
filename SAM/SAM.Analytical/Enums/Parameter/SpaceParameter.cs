﻿using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Space)), Description("Space Parameter")]
    public enum SpaceParameter
    {
        [ParameterProperties("Design Heating Load", "Design Heating Load"), DoubleParameterValue(0)] DesignHeatingLoad,
        [ParameterProperties("Specified Exhaust Airflow", "Extract Airflow"), DoubleParameterValue(0)] ExtractAirflow,
        [ParameterProperties("Specified Supply Airflow", "Supply Airflow"), DoubleParameterValue(0)] SupplyAirflow,
        [ParameterProperties("Volume", "Volume [m3]"), DoubleParameterValue(0)] Volume,
        [ParameterProperties("Area", "Area [m2]"), DoubleParameterValue(0)] Area,
        [ParameterProperties("Occupancy", "Occupancy [p]"), DoubleParameterValue(0)] Occupancy,
        [ParameterProperties("Facing External", "Facing External"), ParameterValue(Core.ParameterType.Boolean)] FacingExternal,
        [ParameterProperties("Facing External Glazing", "Facing External Glazing"), ParameterValue(Core.ParameterType.Boolean)] FacingExternalGlazing,
    }
}
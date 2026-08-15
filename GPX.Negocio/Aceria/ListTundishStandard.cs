using System;
using System.Collections.Generic;
using System.Text;

// [GPX-DOC-v1] ================================================================================
// Extiende ORM.TundishStandard para su uso en listados y vistas.
// ================================================================================================

namespace GPX.Negocio.Aceria
{
    /// <summary>
    /// Clase ListTundishStandard. Extiende ORM.TundishStandard para su uso en listados y vistas.
    /// </summary>
    public  class ListTundishStandard : ORM.TundishStandard
    {
        public string tsNombreCompleto
        {
            get
            {
                return tsId.ToString() + "    [" + tsCierre1.Trim() + " - " + tsCierre2.Trim() + " - " + tsCierre3.Trim() + " - " + tsCierre4.Trim() + " - " + tsCierre5.Trim() + " - " + tsCierre6.Trim() + "]";
            }
        }

    }
}

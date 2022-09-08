using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialTransactionListener.V1.Domain.Transaction
{
    public class SuspenseResolutionInfo
    {
        public DateTime? ResolutionDate { get; set; }

        public bool IsResolve
        {
            get
            {
                if ((IsConfirmed && IsApproved))
                    return true;
                return false;
            }
        }

        private bool IsConfirmed { get; set; }
        private bool IsApproved { get; set; }

        public string Note { get; set; }
    }
}

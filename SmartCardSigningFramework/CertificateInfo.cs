using System.Collections.Generic;

namespace SmartCardSigningFramework
{
    public class CertificateInfo
    {
        public CertificateViewModel SelectedCertificate { get; set; }
        public List<CertificateViewModel> AllValidCertificates { get; set; }
    }
}

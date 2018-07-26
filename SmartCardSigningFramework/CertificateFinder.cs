using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SmartCardSigningFramework
{
    public class CertificateFinder
    {
        public CertificateInfo GetCertInfo()
        {
            var validCertificates = GetValidCertificatesFromStore("May", "JONES");
            var selectedCertViewModel = validCertificates[0];

            return new CertificateInfo { AllValidCertificates = validCertificates, SelectedCertificate = selectedCertViewModel };
        }

        private List<CertificateViewModel> GetValidCertificatesFromStore(string givenname, string familyname)
        {
            var m_x509StoreHelper = new X509StoreHelper();
            var certificates = m_x509StoreHelper.Open();
            var validCertificates = CreateFromValidCertificates(certificates, givenname, familyname).OrderBy(c => c.Distance).ThenByDescending(v => v.ValidTo).ToList();
            return validCertificates;
        }
        private static List<CertificateViewModel> CreateFromValidCertificates(X509Certificate2Collection certificates, string givenName, string familyName)
        {
            var result = new List<CertificateViewModel>();
            foreach (var certificate in certificates)
            {
                foreach (var extension in certificate.Extensions)
                {
                    var keyUsage = extension as X509KeyUsageExtension;
                    if (keyUsage != null && (0 != (keyUsage.KeyUsages & X509KeyUsageFlags.NonRepudiation)))
                    {
                        if (certificate.HasPrivateKey && certificate.Archived == false)
                        {
                            result.Add(new CertificateViewModel(certificate, givenName, familyName));
                        }
                    }
                }
            }

            return result;
        }

        public RSACryptoServiceProvider GetKey(CertificateInfo certInfo, SecureString pinCode)
        {
            if (!certInfo.SelectedCertificate.Certificate.HasPrivateKey)
            {
                throw new Exception("Missing private key");
            }

            try
            {
                const int ProviderType = 1; // RSA algorithm

                var cspParams = (RSACryptoServiceProvider)certInfo.SelectedCertificate.Certificate.PrivateKey;
                var cspKci = cspParams.CspKeyContainerInfo;
                var parameters = new CspParameters(
                    ProviderType,
                    cspKci.ProviderName,
                    cspKci.KeyContainerName,
                    new CryptoKeySecurity(),
                    pinCode);
                parameters.Flags |= CspProviderFlags.NoPrompt; //CspProviderFlags.UseExistingKey;// | CspProviderFlags.NoPrompt;
                parameters.KeyNumber = (int)cspKci.KeyNumber;
                return new RSACryptoServiceProvider(parameters);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Signing exception");
                //KeyPassword = null;
                //CertificateSerialNumber = null;

                //throw new SigningException("Error getting certificate key", e);
            }


        }

    }
}
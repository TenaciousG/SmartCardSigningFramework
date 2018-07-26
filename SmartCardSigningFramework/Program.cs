using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SmartCardSigningFramework
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var stopWatch = Stopwatch.StartNew();

            var certificateFinder = new CertificateFinder();
            var certInfo = certificateFinder.GetCertInfo();
            Console.WriteLine($"GetCertInfo ticks: {stopWatch.ElapsedTicks}");

            var xmlDocs = CreateTwoXmlDocs();

            SecureString pinCode = new SecureString();
            pinCode.AppendChar('6');
            pinCode.AppendChar('8');
            pinCode.AppendChar('6');
            pinCode.AppendChar('1');

            using (var key = certificateFinder.GetKey(certInfo, pinCode))
            {
                Console.WriteLine($"GetKey ticks: {stopWatch.ElapsedTicks}");
                foreach (var xmlDoc in xmlDocs)
                {
                    var signedXml = new SignedXml(xmlDoc)
                    {
                        SigningKey = key
                    };

                    var keyInfo = new KeyInfo();
                    keyInfo.AddClause(new KeyInfoX509Data(certInfo.SelectedCertificate.Certificate,
                        X509IncludeOption.EndCertOnly));
                    signedXml.KeyInfo = keyInfo;

                    // Create a reference to be signed.
                    var reference = new Reference();
                    reference.Uri = string.Empty;

                    // Add an enveloped transformation to the reference.
                    var env = new XmlDsigEnvelopedSignatureTransform();
                    reference.AddTransform(env);

                    // Add the reference to the SignedXml object.
                    signedXml.AddReference(reference);

                    // Compute the signature.
                    signedXml.ComputeSignature();
                    Console.WriteLine($"ComputeSignature ticks: {stopWatch.ElapsedTicks}");

                    var signature = signedXml.GetXml();

                    var certificateSerialNumber = certInfo.SelectedCertificate.Certificate.SerialNumber;

                    xmlDoc.DocumentElement.AppendChild(signature);
                    //perSigningCallback?.Invoke();
                }
            }

            Console.WriteLine("\nDone! Press any key to exit");
            Console.ReadKey();
        }

        private static Collection<XmlDocument> CreateTwoXmlDocs()
        {
            return new Collection<XmlDocument>{CreateXmlDoc(), CreateXmlDoc()};
        }

        private static XmlDocument CreateXmlDoc()
        {
            XmlDocument doc = new XmlDocument();

            //(1) the xml declaration is recommended, but not mandatory
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            //(2) string.Empty makes cleaner code
            XmlElement element1 = doc.CreateElement(string.Empty, "body", string.Empty);
            doc.AppendChild(element1);

            XmlElement element2 = doc.CreateElement(string.Empty, "level1", string.Empty);
            element1.AppendChild(element2);

            XmlElement element3 = doc.CreateElement(string.Empty, "level2", string.Empty);
            XmlText text1 = doc.CreateTextNode("text");
            element3.AppendChild(text1);
            element2.AppendChild(element3);

            XmlElement element4 = doc.CreateElement(string.Empty, "level2", string.Empty);
            XmlText text2 = doc.CreateTextNode("other text");
            element4.AppendChild(text2);
            element2.AppendChild(element4);

            return doc;
        }
    }
}

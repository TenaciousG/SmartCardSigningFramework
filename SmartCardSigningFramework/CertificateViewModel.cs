using System;
using System.Security.Cryptography.X509Certificates;

namespace SmartCardSigningFramework
{
    public class CertificateViewModel
    {
        private X509Certificate2 m_x509Certificate2;
        private string m_givenName;
        private string m_familyName;

        public X509Certificate2 Certificate
        {
            get
            {
                return m_x509Certificate2;
            }
        }

        public CertificateViewModel(X509Certificate2 certificate, string givenName, string familyName)
        {
            m_x509Certificate2 = certificate;
            m_givenName = givenName;
            m_familyName = familyName;

            var fullName = string.Format("{0} {1}", givenName, familyName);
            Distance = this.LevenshteinDistance(fullName.ToUpper(), IssuedTo.ToUpper());
        }

        public int Distance { get; private set; }

        public bool IsNameMatch
        {
            get
            {
                return this.IsNameContainedInIssuedTo();
            }
        }

        public string IssuedTo
        {
            get
            {
                return m_x509Certificate2.GetNameInfo(X509NameType.SimpleName, false);
            }
        }

        public string ValidFrom
        {
            get { return m_x509Certificate2.NotBefore.ToShortDateString(); }
        }

        public string ValidTo
        {
            get { return m_x509Certificate2.NotAfter.ToShortDateString(); }
        }

        private bool IsNameContainedInIssuedTo()
        {
            return IssuedTo.ToUpper().Contains(m_givenName.ToUpper()) && IssuedTo.ToUpper().Contains(m_familyName.ToUpper());
        }

        private int LevenshteinDistance(string s, string t)
        {
            // degenerate cases
            if (s == t) return 0;
            if (s.Length == 0) return t.Length;
            if (t.Length == 0) return s.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[t.Length + 1];
            int[] v1 = new int[t.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < s.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < t.Length; j++)
                {
                    var cost = (s[i] == t[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(Math.Min(v1[j] + 1, v0[j + 1] + 1), v0[j] + cost);
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[t.Length];
        }
    }
}
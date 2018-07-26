using System;
using System.Security.Cryptography.X509Certificates;

namespace SmartCardSigningFramework
{
    public class X509StoreHelper : IDisposable
    {
        private X509Certificate2Collection m_certificate2Collection;
        private X509Store m_store;

        public X509StoreHelper() : this(null)
        { }

        public X509StoreHelper(X509Certificate2Collection testCollection)
        {
            m_certificate2Collection = testCollection;
        }

        public X509Certificate2Collection Open()
        {
            if (m_certificate2Collection != null)
            {
                return m_certificate2Collection;
            }

            m_store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            m_store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            return m_store.Certificates;
        }

        public void Close()
        {
            if (m_store != null)
            {
                m_store.Close();
                m_store = null;
            }
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
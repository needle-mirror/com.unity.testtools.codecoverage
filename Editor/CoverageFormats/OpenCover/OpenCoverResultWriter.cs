using System.Xml.Serialization;
using System.IO;
using UnityEngine;
using OpenCover.Framework.Model;

namespace UnityEditor.TestTools.CodeCoverage.OpenCover
{
    internal class OpenCoverResultWriter : CoverageResultWriterBase<CoverageSession>
    {
        public OpenCoverResultWriter(CoverageSettings coverageSettings) : base(coverageSettings)
        {
        }

        public override void WriteCoverageSession()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CoverageSession));
            string fileFullPath = GetNextFullFilePath();
            using (TextWriter writer = new StreamWriter(fileFullPath))
            {
                serializer.Serialize(writer, CoverageSession);
            }

            Debug.Log($"[{CoverageSettings.PackageName}] Code Coverage Results were saved in {fileFullPath}");

            base.WriteCoverageSession();
        }
    }
}

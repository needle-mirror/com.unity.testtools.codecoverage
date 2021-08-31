using System.Xml.Serialization;
using System.IO;
using OpenCover.Framework.Model;
using UnityEditor.TestTools.CodeCoverage.Utils;

namespace UnityEditor.TestTools.CodeCoverage.OpenCover
{
    internal class OpenCoverResultWriter : CoverageResultWriterBase<CoverageSession>
    {
        public OpenCoverResultWriter(CoverageSettings coverageSettings) : base(coverageSettings)
        {
        }

        public override void WriteCoverageSession(bool atRoot = false)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CoverageSession));
            string fileFullPath = atRoot ? GetRootFullEmptyPath() : GetNextFullFilePath();
            if (!System.IO.File.Exists(fileFullPath))
            {
                using (TextWriter writer = new StreamWriter(fileFullPath))
                {
                    serializer.Serialize(writer, CoverageSession);
                }

                ResultsLogger.Log(ResultID.Log_ResultsSaved, fileFullPath);
                CoverageEventData.instance.AddSessionResultPath(fileFullPath);

                base.WriteCoverageSession(atRoot);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIServices.Common.Types.Response;
using SoaCore.Wcf;

namespace BIServices.CgiisSvc
{
    [ParameterInspectorLogger, CGIErrorHandler]
    public class CgiisService : ICgiisService
    {
        public SubmitResponse SubmitJob(string jobName)
        {
            var processor = new CgiisProcessor();
            try
            {
                return processor.SubmitJob(jobName);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }

    [ParameterInspectorLogger, CGIErrorHandler]
    public class CgiisRestService : ICgiisRestService
    {
        public SubmitResponse SubmitJob(string jobName)
        {
            var processor = new CgiisProcessor();
            try
            {
                return processor.SubmitJob(jobName);
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}

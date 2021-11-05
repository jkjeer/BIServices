using System;
using BIServices.Common.Managers.Cgiis;
using BIServices.Common.Types.Response;

namespace BIServices.CgiisSvc
{
    public class CgiisProcessor
    {
        public SubmitResponse SubmitJob(string jobName)
        {
            try
            {
                var mgr = new CgiisManager();
                return mgr.SubmitJob(jobName);
            }
            catch (Exception err)
            {
                throw new ApplicationException(err.ToString());
            }
        }
    }
}

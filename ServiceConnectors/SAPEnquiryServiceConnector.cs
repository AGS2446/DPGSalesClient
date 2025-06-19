using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SAPEnquiryProxy;
using Microsoft.AspNetCore.Http;

namespace DPGSalesClient.ServiceConnectors
{    
    public class SAPEnquiryServiceConnector
    {
        private SAPEnquiryServiceClient _sapEnquiry = new SAPEnquiryServiceClient();
        public SAPEnquiryServiceConnector(string strRemoteIP, IHttpContextAccessor httpContextAccessor)
        {

            if (!String.IsNullOrEmpty(strRemoteIP))
            {              
                if (strRemoteIP.Contains("https"))
                {
                    _sapEnquiry = new SAPEnquiryServiceClient();                    
                }
                else
                {
                    _sapEnquiry = new SAPEnquiryServiceClient();
                }
            }

            _sapEnquiry.Endpoint.EndpointBehaviors.Add(new ServiceBehaviors.TokenEndpointBehavior(httpContextAccessor));
        }

        public async Task<AGS_SAPEnquiryResponse> Create(AGS_SAPEnquiry request)
        {
            return await _sapEnquiry.SAPEnquiryAsync(request);
        }
    }
}

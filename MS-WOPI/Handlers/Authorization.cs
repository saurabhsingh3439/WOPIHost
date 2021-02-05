using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Request;
using MS_WOPI.Interfaces;
using System.Net;

namespace MS_WOPI.Handlers
{
    public class Authorization : IAuthorization
    {
        /// <summary>
        /// Validate WOPI ProofKey to make sure request came from the expected Office Web Apps Server.
        /// </summary>
        /// <param name="request">Request information</param>
        /// <returns>true when WOPI ProofKey validation succeeded, false otherwise.</returns>
        public bool ValidateWopiProofKey(HttpListenerRequest request)
        {
            // TODO: WOPI proof key validation is not implemented in this sample.
            // For more details on proof keys, see the documentation
            // https://wopi.readthedocs.io/en/latest/scenarios/proofkeys.html

            // The proof keys are returned by WOPI Discovery. For more details, see
            // https://wopi.readthedocs.io/en/latest/discovery.html

            return true;
        }

        /// <summary>
        /// Validate that the provided access token is valid to get access to requested resource.
        /// </summary>
        /// <param name="requestData">Request information, including requested file Id</param>
        /// <param name="writeAccessRequired">Whether write permission is requested or not.</param>
        /// <returns>true when access token is correct and user has access to document, false otherwise.</returns>
        public bool ValidateAccess(WopiRequest requestData, bool writeAccessRequired)
        {
            // TODO: Access token validation is not implemented in this sample.
            // For more details on access tokens, see the documentation
            // https://wopi.readthedocs.io/projects/wopirest/en/latest/concepts.html#term-access-token
            // "INVALID" is used by the WOPIValidator.
            return !String.IsNullOrWhiteSpace(requestData.AccessToken) && (requestData.AccessToken != "INVALID");
        }
    }
}

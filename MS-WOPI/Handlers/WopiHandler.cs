using System;
using System.Collections.Generic;
using System.Web;
using MS_WOPI.Request;
using MS_WOPI.Interfaces;
using MS_WOPI.Common;
using MS_WOPI.Response;
using System.Net;
using MS_WOPI.ProcessWopi;

namespace MS_WOPI.Handlers
{
    public class WopiHandler : IWopiHandler
    {
        private const string WopiPath = @"/wopi/";
        private const string FilesRequestPath = @"files/";
        private const string FoldersRequestPath = @"folders/";
        private const string ContentsRequestPath = @"/contents";
        private const string ChildrenRequestPath = @"/children";
        public static string LocalStoragePath = @"c:\WopiStorage\";

        private IErrorHandler _errHandler;
        private IAuthorization _authorization;
        private IWopiProcessor _processor;
        //private WopiResponse _response;

        public WopiHandler(string storagePath)
        {
            //LocalStoragePath = storagePath;
            _errHandler = new ErrorHandler();
            _authorization = new Authorization();
        }

        private static readonly Dictionary<string, LockInfo> Locks = new Dictionary<string, LockInfo>();

        #region IHttpHandler Members

        /// <summary>
        /// Begins processing the incoming WOPI request.
        /// </summary>
        public void ProcessRequest(IAsyncResult result)
        {
            // WOPI ProofKey validation is an optional way that a WOPI host can ensure that the request
            // is coming from the Office server that they expect to be talking to.
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            if (!_authorization.ValidateWopiProofKey(context.Request))
            {
               _errHandler.ReturnServerError(context.Response);
            }
            _processor = new WopiProcessor(_authorization, _errHandler, context.Response);
            // Parse the incoming WOPI request
            WopiRequest requestData = ParseRequest(context.Request);

            // Call the appropriate handler for the WOPI request we received
            switch (requestData.Type)
            {
                case RequestType.CheckFileInfo:
                    _processor.HandleCheckFileInfoRequest(requestData);
                    break;

                case RequestType.Lock:
                    _processor.HandleLockRequest(requestData);
                    break;

                case RequestType.Unlock:
                    _processor.HandleUnlockRequest(requestData);
                    break;

                case RequestType.RefreshLock:
                    _processor.HandleRefreshLockRequest(requestData);
                    break;

                case RequestType.UnlockAndRelock:
                    _processor.HandleUnlockAndRelockRequest(requestData);
                    break;

                case RequestType.GetFile:
                    _processor.HandleGetFileRequest(requestData);
                    break;

                case RequestType.PutFile:
                    _processor.HandlePutFileRequest(requestData);
                    break;
                case RequestType.PutRelativeFile:
                case RequestType.EnumerateChildren:
                case RequestType.CheckFolderInfo:
                case RequestType.DeleteFile:
                case RequestType.ExecuteCobaltRequest:
                case RequestType.GetRestrictedLink:
                case RequestType.ReadSecureStore:
                case RequestType.RevokeRestrictedLink:
                    _errHandler.ReturnUnsupported(context.Response);
                    break;

                default:
                    _errHandler.ReturnServerError(context.Response);
                    break;
            }
        }

        private static WopiRequest ParseRequest(HttpListenerRequest request)
        {
            // Initilize wopi request data object with default values
            WopiRequest requestData = new WopiRequest()
            {
                Type = RequestType.None,
                AccessToken = request.QueryString["access_token"],
                Id = ""
            };

            string requestPath = request.Url.AbsolutePath;
            // remove /<...>/wopi/
            string wopiPath = requestPath.Substring(WopiPath.Length);

            if (wopiPath.StartsWith(FilesRequestPath))
            {
                string rawId = wopiPath.Substring(FilesRequestPath.Length);

                if (rawId.EndsWith(ContentsRequestPath))
                {
                    // The rawId ends with /contents so this is a request to read/write the file contents

                    // Remove /contents from the end of rawId to get the actual file id
                    requestData.Id = rawId.Substring(0, rawId.Length - ContentsRequestPath.Length);

                    if (request.HttpMethod == "GET")
                        requestData.Type = RequestType.GetFile;
                    if (request.HttpMethod == "POST")
                    {
                        requestData.Type = RequestType.PutFile;
                        request.InputStream.CopyTo(requestData.FileData);
                    }
                }
                else
                {
                    requestData.Id = rawId;

                    if (request.HttpMethod == "GET")
                    {
                        // a GET to the file is always a CheckFileInfo request
                        requestData.Type = RequestType.CheckFileInfo;
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        request.InputStream.CopyTo(requestData.FileData);
                        // For a POST to the file we need to use the X-WOPI-Override header to determine the request type
                        string wopiOverride = request.Headers[WopiHeaders.RequestType];

                        switch (wopiOverride)
                        {
                            case "PUT_RELATIVE":
                                requestData.Type = RequestType.PutRelativeFile;
                                break;
                            case "LOCK":
                                // A lock could be either a lock or an unlock and relock, determined based on whether
                                // the request sends an OldLock header.
                                if (request.Headers[WopiHeaders.OldLock] != null)
                                    requestData.Type = RequestType.UnlockAndRelock;
                                else
                                    requestData.Type = RequestType.Lock;
                                break;
                            case "UNLOCK":
                                requestData.Type = RequestType.Unlock;
                                break;
                            case "REFRESH_LOCK":
                                requestData.Type = RequestType.RefreshLock;
                                break;
                            case "COBALT":
                                requestData.Type = RequestType.ExecuteCobaltRequest;
                                break;
                            case "DELETE":
                                requestData.Type = RequestType.DeleteFile;
                                break;
                            case "READ_SECURE_STORE":
                                requestData.Type = RequestType.ReadSecureStore;
                                break;
                            case "GET_RESTRICTED_LINK":
                                requestData.Type = RequestType.GetRestrictedLink;
                                break;
                            case "REVOKE_RESTRICTED_LINK":
                                requestData.Type = RequestType.RevokeRestrictedLink;
                                break;
                        }
                    }
                }
            }
            else if (wopiPath.StartsWith(FoldersRequestPath))
            {
                // A folder-related request.

                // remove /folders/ from the beginning of wopiPath
                string rawId = wopiPath.Substring(FoldersRequestPath.Length);

                if (rawId.EndsWith(ChildrenRequestPath))
                {
                    // rawId ends with /children, so it's an EnumerateChildren request.

                    // remove /children from the end of rawId
                    requestData.Id = rawId.Substring(0, rawId.Length - ChildrenRequestPath.Length);
                    requestData.Type = RequestType.EnumerateChildren;
                }
                else
                {
                    // rawId doesn't end with /children, so it's a CheckFolderInfo.

                    requestData.Id = rawId;
                    requestData.Type = RequestType.CheckFolderInfo;
                }
            }
            else
            {
                // An unknown request.
                requestData.Type = RequestType.None;
            }
            return requestData;
        }

        #endregion
    }
}

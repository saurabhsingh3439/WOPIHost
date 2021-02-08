using System;
using System.Collections.Generic;
using System.Web;
using MS_WOPI.Request;
using MS_WOPI.Interfaces;
using MS_WOPI.Common;
using MS_WOPI.Response;
using System.Net;
using MS_WOPI.ProcessWopi;
using System.IO;

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

        

        
        public void ProcessRequest(IAsyncResult result)
        {
            
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            if (!_authorization.ValidateWopiProofKey(context.Request))
            {
               _errHandler.ReturnServerError(context.Response);
            }
            _processor = new WopiProcessor(_authorization, _errHandler, context.Response);
            
            WopiRequest requestData = ParseRequest(context.Request);

            
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
            WopiRequest requestData = new WopiRequest()
            {
                Type = RequestType.None,
                AccessToken = request.QueryString["access_token"],
                Id = "",
                LockId = request.Headers[WopiHeaders.Lock],
                OldLockId = request.Headers[WopiHeaders.OldLock]
            };

            string requestPath = request.Url.AbsolutePath;
            // remove /<...>/wopi/
            string wopiPath = requestPath.Substring(WopiPath.Length);

            if (wopiPath.StartsWith(FilesRequestPath))
            {
                string rawId = wopiPath.Substring(FilesRequestPath.Length);

                if (rawId.EndsWith(ContentsRequestPath))
                {

                    requestData.Id = rawId.Substring(0, rawId.Length - ContentsRequestPath.Length);

                    if (request.HttpMethod == "GET")
                        requestData.Type = RequestType.GetFile;
                    if (request.HttpMethod == "POST")
                    {
                        requestData.Type = RequestType.PutFile;
                    }
                }
                else
                {
                    requestData.Id = rawId;

                    if (request.HttpMethod == "GET")
                    {
                        requestData.Type = RequestType.CheckFileInfo;
                    }
                    else if (request.HttpMethod == "POST")
                    {
                        
                        string wopiOverride = request.Headers[WopiHeaders.RequestType];

                        switch (wopiOverride)
                        {
                            case "PUT_RELATIVE":
                                requestData.Type = RequestType.PutRelativeFile;
                                break;
                            case "LOCK":
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
                string rawId = wopiPath.Substring(FoldersRequestPath.Length);

                if (rawId.EndsWith(ChildrenRequestPath))
                {
                    requestData.Id = rawId.Substring(0, rawId.Length - ChildrenRequestPath.Length);
                    requestData.Type = RequestType.EnumerateChildren;
                }
                else
                {
                    requestData.Id = rawId;
                    requestData.Type = RequestType.CheckFolderInfo;
                }
            }
            else
            {
                requestData.Type = RequestType.None;
            }
            return requestData;
        }

    }
}

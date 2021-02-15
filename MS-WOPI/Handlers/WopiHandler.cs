/*
Copyright Mitratech Holdings Inc, 2021
This software is provided under the terms of a License Agreement and may
only be used and/or copied in accordance with the terms of such agreement.
Neither this software nor any copy thereof may be provided or otherwise
made available to any other person. No title or ownership of this software
is hereby transferred.
*/

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
using System.Threading;

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
        private WopiUserRequest _userRequest;

        public WopiHandler(WopiUserRequest wopiUser)
        {
            _userRequest = wopiUser;
            _errHandler = new ErrorHandler();
            _authorization = new Authorization();
        }

        private static readonly Dictionary<string, LockInfo> Locks = new Dictionary<string, LockInfo>();
        
        public void ProcessRequest(IAsyncResult result)
        {
            Thread process_thread = new Thread(() => ProcessRequestPrivate(result));
            process_thread.Start();
        }
        
        public void ProcessRequestPrivate(IAsyncResult result)
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
                    _processor.HandlePutRelativeFileRequest(requestData);
                    break;

                case RequestType.GetLock:
                    _processor.GetFileLockId(requestData);
                    break;

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
                        using (var memstream = new MemoryStream())
                        {
                            memstream.Flush();
                            memstream.Position = 0;
                            request.InputStream.CopyTo(memstream);
                            requestData.FileData = memstream.ToArray();
                        }
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
                                if (request.Headers[WopiHeaders.RelativeTarget] != null) 
                                    requestData.RelativeTarget = request.Headers[WopiHeaders.RelativeTarget];
                                if (request.Headers[WopiHeaders.SuggestedTarget] != null) 
                                    requestData.SuggestedTarget = request.Headers[WopiHeaders.SuggestedTarget];
                                if (request.Headers[WopiHeaders.OverwriteRelativeTarget] != null)
                                    requestData.OverwriteTarget = bool.Parse(request.Headers[WopiHeaders.OverwriteRelativeTarget]);

                                using (var memstream = new MemoryStream())
                                {
                                    memstream.Flush();
                                    memstream.Position = 0;
                                    request.InputStream.CopyTo(memstream);
                                    requestData.FileData = memstream.ToArray();
                                }

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

                            case "GET_LOCK":
                                requestData.Type = RequestType.GetLock;
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

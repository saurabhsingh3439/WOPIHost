using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Request;
using MS_WOPI.Response;
using MS_WOPI.Interfaces;
using System.IO;
using MS_WOPI.Common;
using System.Net;
using MS_WOPI.Response.ResponseGenerator;
using System.Runtime.Serialization.Json;

namespace MS_WOPI.ProcessWopi
{
    public class WopiProcessor : IWopiProcessor
    {
        private IAuthorization _authorization;
        private IErrorHandler _errorHandler;
        private HttpListenerResponse _response;
        public WopiProcessor(IAuthorization authorization, IErrorHandler errorHandler, HttpListenerResponse response)
        {
            _authorization = authorization;
            _errorHandler = errorHandler;
            _response = response;
        }
        public void HandleCheckFileInfoRequest(WopiRequest requestData)
        {
            lock (this)
            {
                // userId(user@polihub) will be passed from the policyHub application
                if (!_authorization.ValidateToken(requestData.AccessToken, "user@policyhub", requestData.Id))
                {
                    _errorHandler.ReturnInvalidToken(_response);
                    _response.Close();
                    return;
                }

                if (!File.Exists(requestData.FullPath))
                {
                    _errorHandler.ReturnFileUnknown(_response);
                    _response.Close();
                    return;
                }

                try
                {
                    FileInfo fileInfo = new FileInfo(requestData.FullPath);
                    ResponseGenerator generator = new ResponseGenerator(fileInfo);
                    if (!fileInfo.Exists)
                    {
                        _errorHandler.ReturnFileUnknown(_response);
                        return;
                    }

                    var memoryStream = new MemoryStream();
                    var json = new DataContractJsonSerializer(typeof(WopiCheckFileInfo));
                    json.WriteObject(memoryStream, generator.GetFileInfoResponse());
                    memoryStream.Flush();
                    memoryStream.Position = 0;
                    StreamReader streamReader = new StreamReader(memoryStream);
                    var jsonResponse = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());

                    _response.ContentType = @"application/json";
                    _response.ContentLength64 = jsonResponse.Length;
                    _response.OutputStream.Write(jsonResponse, 0, jsonResponse.Length);
                    _errorHandler.ReturnSuccess(_response);

                }
                catch (UnauthorizedAccessException)
                {
                    _errorHandler.ReturnFileUnknown(_response);

                }
                _response.Close();
            }
        }

        public void HandleGetFileRequest(WopiRequest requestData)
        {

            lock (this)
            {
                // userId(user@polihub) will be passed from the policyHub application
                if (!_authorization.ValidateToken(requestData.AccessToken, "user@policyhub", requestData.Id))
                {
                    _errorHandler.ReturnInvalidToken(_response);
                    _response.Close();
                    return;
                }

                if (!File.Exists(requestData.FullPath))
                {
                    _errorHandler.ReturnFileUnknown(_response);
                    _response.Close();
                    return;
                }
                try
                {
                    FileInfo fileInfo = new FileInfo(requestData.FullPath);
                    ResponseGenerator generator = new ResponseGenerator(fileInfo);
                    var content = generator.GetFileContent();
                    _response.ContentType = @"application/x-binary";
                    _response.ContentLength64 = content.Length;
                    _response.OutputStream.Write(content, 0, content.Length);
                    _errorHandler.ReturnSuccess(_response);

                }
                catch (UnauthorizedAccessException)
                {
                    _errorHandler.ReturnFileUnknown(_response);

                }
                catch (FileNotFoundException)
                {
                    _errorHandler.ReturnFileUnknown(_response);
                }
            }
        }

        public static string GetFileVersion(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            return fileInfo.LastWriteTimeUtc.ToString("O" /* ISO 8601 DateTime format string */); // Using the file write time is an arbitrary choice.
        }


        public void HandlePutFileRequest(WopiRequest requestData)
        {
            lock (this)
            {
                // userId(user@polihub) will be passed from the policyHub application
                if (!_authorization.ValidateToken(requestData.AccessToken, "user@policyhub", requestData.Id))
                {
                    _errorHandler.ReturnInvalidToken(_response);
                    _response.Close();
                    return;
                }

                if (!File.Exists(requestData.FullPath))
                {
                    _errorHandler.ReturnFileUnknown(_response);
                    _response.Close();
                    return;
                }
                string newLock = requestData.LockId;
                LockInfo existingLock;
                bool hasExistingLock;

                lock (LockInfo.Locks)
                {
                    hasExistingLock = LockInfo.TryGetLock(requestData.Id, out existingLock);
                }

                if (hasExistingLock && existingLock.Lock != newLock)
                {
                    // lock mismatch/locked by another interface
                    _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                    _response.AddHeader(WopiHeaders.Lock, existingLock.Lock);
                    _response.AddHeader(WopiHeaders.LockFailureReason, "Lock mismatch/Locked by another interface");
                    _response.StatusCode = (int)HttpStatusCode.Conflict;
                    _response.Close();
                    return;
                }

                FileInfo putTargetFileInfo = new FileInfo(requestData.FullPath);

                if (!hasExistingLock && putTargetFileInfo.Length != 0)
                {
                    _response.AddHeader(WopiHeaders.Lock, newLock);
                    _response.AddHeader(WopiHeaders.LockFailureReason, "PutFile on unlocked file with current size != 0");
                    _response.StatusCode = (int)HttpStatusCode.Conflict;
                    _errorHandler.ReturnLockMismatch(_response, reason: "PutFile on unlocked file with current size != 0");
                }


                try
                {
                    ResponseGenerator generator = new ResponseGenerator(putTargetFileInfo);

                    generator.Save(requestData.FileData);
                    _response.ContentLength64 = 0;
                    _response.ContentType = @"text/html";
                    _response.AddHeader(WopiHeaders.Lock, newLock);
                    _response.StatusCode = (int)HttpStatusCode.OK;

                }
                catch (UnauthorizedAccessException)
                {
                    _errorHandler.ReturnFileUnknown(_response);

                }
                catch (IOException)
                {
                    _errorHandler.ReturnServerError(_response);

                }
                _response.Close();
            }
        }


        public void HandleLockRequest(WopiRequest requestData)
        {
            lock (this)
            {
                // userId(user@polihub) will be passed from the policyHub application
                if (!_authorization.ValidateToken(requestData.AccessToken, "user@policyhub", requestData.Id))
                {
                    _errorHandler.ReturnInvalidToken(_response);
                    _response.Close();
                    return;
                }

                if (!File.Exists(requestData.FullPath))
                {
                    _errorHandler.ReturnFileUnknown(_response);
                    _response.Close();
                    return;
                }

                string newLock = requestData.LockId;

                lock (LockInfo.Locks)
                {
                    LockInfo existingLock;
                    bool fLocked = LockInfo.TryGetLock(requestData.Id, out existingLock);
                    if (fLocked && existingLock.Lock != newLock)
                    {

                        _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                        _response.AddHeader(WopiHeaders.Lock, existingLock.Lock);
                        _response.AddHeader(WopiHeaders.LockFailureReason, "Lock mismatch/Locked by another interface");
                        _response.StatusCode = (int)HttpStatusCode.Conflict;

                    }
                    else
                    {

                        if (fLocked)
                            LockInfo.Locks.Remove(requestData.Id);

                        LockInfo.Locks[requestData.Id] = new LockInfo() { DateCreated = DateTime.UtcNow, Lock = newLock };
                        _errorHandler.ReturnSuccess(_response);
                        _response.AddHeader(WopiHeaders.Lock, newLock);
                        _response.StatusCode = (int)HttpStatusCode.OK;
                    }
                }
                _response.Close();
            }
        }


        public void HandleRefreshLockRequest(WopiRequest requestData)
        {
            lock (this)
            {
                // userId(user@polihub) will be passed from the policyHub application
                if (!_authorization.ValidateToken(requestData.AccessToken, "user@policyhub", requestData.Id))
                {
                    _errorHandler.ReturnInvalidToken(_response);
                    _response.Close();
                    return;
                }
                if (!File.Exists(requestData.FullPath))
                {
                    _errorHandler.ReturnFileUnknown(_response);
                    _response.Close();
                    return;
                }

                string newLock = requestData.LockId;

                lock (LockInfo.Locks)
                {
                    LockInfo existingLock;
                    if (LockInfo.TryGetLock(requestData.Id, out existingLock))
                    {
                        if (existingLock.Lock == newLock)
                        {

                            existingLock.DateCreated = DateTime.UtcNow;
                            _response.AddHeader(WopiHeaders.Lock, existingLock.Lock);
                            _response.StatusCode = (int)HttpStatusCode.OK;
                            _errorHandler.ReturnSuccess(_response);

                        }
                        else
                        {
                            _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                            _response.AddHeader(WopiHeaders.Lock, existingLock.Lock);
                            _response.AddHeader(WopiHeaders.LockFailureReason, "Lock mismatch/Locked by another interface");
                            _response.StatusCode = (int)HttpStatusCode.Conflict;
                        }
                    }
                    else
                    {
                        _errorHandler.ReturnLockMismatch(_response, reason: "File not locked");
                        _response.AddHeader(WopiHeaders.Lock, newLock);
                        _response.AddHeader(WopiHeaders.LockFailureReason, "File not locked");
                        _response.StatusCode = (int)HttpStatusCode.Conflict;

                    }
                }
                _response.Close();
            }
        }

        public void HandleUnlockRequest(WopiRequest requestData)
        {
            lock (this)
            {
                // userId(user@polihub) will be passed from the policyHub application
                if (!_authorization.ValidateToken(requestData.AccessToken, "user@policyhub", requestData.Id))
                {
                    _errorHandler.ReturnInvalidToken(_response);
                    _response.Close();
                    return;
                }
                if (!File.Exists(requestData.FullPath))
                {
                    _errorHandler.ReturnFileUnknown(_response);
                    _response.Close();
                    return;
                }

                string newLock = requestData.LockId;

                lock (LockInfo.Locks)
                {
                    LockInfo existingLock;
                    if (LockInfo.TryGetLock(requestData.Id, out existingLock))
                    {
                        if (existingLock.Lock == newLock)
                        {
                            LockInfo.Locks.Remove(requestData.Id);
                            _errorHandler.ReturnSuccess(_response);
                            _response.AddHeader(WopiHeaders.Lock, existingLock.Lock);
                            _response.StatusCode = (int)HttpStatusCode.OK;

                        }
                        else
                        {
                            _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                            _response.AddHeader(WopiHeaders.Lock, existingLock.Lock);
                            _response.AddHeader(WopiHeaders.LockFailureReason, "Lock mismatch/Locked by another interface");
                            _response.StatusCode = (int)HttpStatusCode.Conflict;
                        }
                    }
                    else
                    {
                        _errorHandler.ReturnLockMismatch(_response, reason: "File not locked");
                        _response.AddHeader(WopiHeaders.Lock, newLock);
                        _response.AddHeader(WopiHeaders.LockFailureReason, "File not locked");
                        _response.StatusCode = (int)HttpStatusCode.Conflict;

                    }
                }
                _response.Close();
            }
        }


        public void HandleUnlockAndRelockRequest(WopiRequest requestData)
        {
            lock (this)
            {
                // userId(user@polihub) will be passed from the policyHub application
                if (!_authorization.ValidateToken(requestData.AccessToken, "user@policyhub", requestData.Id))
                {
                    _errorHandler.ReturnInvalidToken(_response);
                    _response.Close();
                    return;
                }

                if (!File.Exists(requestData.FullPath))
                {
                    _errorHandler.ReturnFileUnknown(_response);
                    _response.Close();
                    return;
                }

                string newLock = requestData.LockId;
                string oldLock = requestData.OldLockId;

                lock (LockInfo.Locks)
                {
                    LockInfo existingLock;
                    if (LockInfo.TryGetLock(requestData.Id, out existingLock))
                    {
                        if (existingLock.Lock == oldLock)
                        {

                            LockInfo.Locks[requestData.Id] = new LockInfo() { DateCreated = DateTime.UtcNow, Lock = newLock };
                            _response.AddHeader(WopiHeaders.Lock, newLock);
                            _response.StatusCode = (int)HttpStatusCode.OK;
                            _errorHandler.ReturnSuccess(_response);
                        }
                        else
                        {
                            _response.AddHeader(WopiHeaders.Lock, existingLock.Lock);
                            _response.AddHeader(WopiHeaders.LockFailureReason, "Lock mismatch/Locked by another interface");
                            _response.StatusCode = (int)HttpStatusCode.Conflict;
                            _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                        }
                    }
                    else
                    {
                        _response.AddHeader(WopiHeaders.Lock, newLock);
                        _response.AddHeader(WopiHeaders.LockFailureReason, "File not locked");
                        _response.StatusCode = (int)HttpStatusCode.Conflict;
                        _errorHandler.ReturnLockMismatch(_response, reason: "File not locked");
                    }
                }
                _response.Close();
            }
        }
    }
}

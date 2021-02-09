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
           // if (_authorization.ValidateAccess(requestData, writeAccessRequired: false))
           // {
             //   _errorHandler.ReturnInvalidToken(_response);
             //   return;
            //}

            if (!File.Exists(requestData.FullPath))
            {
                _errorHandler.ReturnFileUnknown(_response);
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
                StreamReader streamReader = new StreamReader(memoryStream);
                var jsonResponse = Encoding.UTF8.GetBytes(streamReader.ReadToEnd());

                _response.ContentType = @"application/json";
                _response.ContentLength64 = jsonResponse.Length;
                _response.OutputStream.Write(jsonResponse, 0, jsonResponse.Length);
                StreamReader reader = new StreamReader(_response.OutputStream);
                string text = reader.ReadToEnd();
                Console.WriteLine(text);
                _response.Close();
                
                _errorHandler.ReturnSuccess(_response);
            }
            catch (UnauthorizedAccessException)
            {
                _errorHandler.ReturnFileUnknown(_response);
            }
        }

        /// <summary>
        /// Processes a GetFile request
        /// </summary>
        /// <remarks>
        /// For full documentation on GetFile, see
        /// https://wopi.readthedocs.io/projects/wopirest/en/latest/files/GetFile.html
        /// </remarks>
        public void HandleGetFileRequest(WopiRequest requestData)
        {
            if (!_authorization.ValidateAccess(requestData, writeAccessRequired: false))
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
                _response.ContentType = @"application/vnd.ms-excel";
                _response.ContentLength64 = content.Length;
                _response.OutputStream.Write(content, 0, content.Length);
                _response.OutputStream.Flush();
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
            _response.Close();
        }

        public static string GetFileVersion(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            return fileInfo.LastWriteTimeUtc.ToString("O" /* ISO 8601 DateTime format string */); // Using the file write time is an arbitrary choice.
        }

        /// <summary>
        /// Processes a PutFile request
        /// </summary>
        /// <remarks>
        /// For full documentation on PutFile, see
        /// https://wopi.readthedocs.io/projects/wopirest/en/latest/files/PutFile.html
        /// </remarks>
        public void HandlePutFileRequest(WopiRequest requestData)
        {

            if (!_authorization.ValidateAccess(requestData, writeAccessRequired: true))
            {
                _errorHandler.ReturnInvalidToken(_response);
                return;
            }

            if (!File.Exists(requestData.FullPath))
            {
                _errorHandler.ReturnFileUnknown(_response);
                return;
            }
            string newLock = requestData.lockID;
            LockInfo existingLock;
            bool hasExistingLock;

            lock (LockInfo.Locks)
            {
                hasExistingLock = LockInfo.TryGetLock(requestData.Id, out existingLock);
            }

            if (hasExistingLock && existingLock.Lock != newLock)
            {
                // lock mismatch/locked by another interface
                _errorHandler.ReturnLockMismatch(_response, existingLock.Lock, "Lock Mismatch");
                return;
            }

            FileInfo putTargetFileInfo = new FileInfo(requestData.FullPath);

            // The WOPI spec allows for a PutFile to succeed on a non-locked file if the file is currently zero bytes in length.
            // This allows for a more efficient Create New File flow that saves the Lock roundtrips.
            if (!hasExistingLock && putTargetFileInfo.Length != 0)
            {
                // With no lock and a non-zero file, a PutFile could potentially result in data loss by clobbering
                // existing content.  Therefore, return a lock mismatch error.
                _errorHandler.ReturnLockMismatch(_response, reason: "PutFile on unlocked file with current size != 0");
                return;
            }

            // Either the file has a valid lock that matches the lock in the request, or the file is unlocked
            // and is zero bytes.  Either way, proceed with the PutFile.
            try
            {
                ResponseGenerator generator = new ResponseGenerator(putTargetFileInfo);
                generator.SaveusingBytes(requestData);
                _response.ContentLength64 = 0;
                _response.ContentType = @"text/html";
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
        }

        /// <summary>
        /// Processes a Lock request
        /// </summary>
        /// <remarks>
        /// For full documentation on Lock, see
        /// https://wopi.readthedocs.io/projects/wopirest/en/latest/files/Lock.html
        /// </remarks>
        public void HandleLockRequest(WopiRequest requestData)
        {
            if (!_authorization.ValidateAccess(requestData, writeAccessRequired: true))
            {
                _errorHandler.ReturnInvalidToken(_response);
                return;
            }

            if (!File.Exists(requestData.FullPath))
            {
                _errorHandler.ReturnFileUnknown(_response);
                return;
            }

            string newLock = requestData.lockID;

            lock (LockInfo.Locks)
            {
                LockInfo existingLock;
                bool fLocked = LockInfo.TryGetLock(requestData.Id, out existingLock);
                if (fLocked && existingLock.Lock != newLock)
                {
                    // There is a valid existing lock on the file and it doesn't match the requested lockstring.

                    // This is a fairly common case and shouldn't be tracked as an error.  Office can store
                    // information about a current session in the lock value and expects to conflict when there's
                    // an existing session to join.
                    _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                    return;
                }
                else
                {
                    // The file is not currently locked or the lock has already expired

                    if (fLocked)
                        LockInfo.Locks.Remove(requestData.Id);

                    // Create and store new lock information
                    // TODO: In a real implementation the lock should be stored in a persisted and shared system.
                    LockInfo.Locks[requestData.Id] = new LockInfo() { DateCreated = DateTime.UtcNow, Lock = newLock };

                    //context.Response.AddHeader(WopiHeaders.ItemVersion, GetFileVersion(requestData.FullPath));

                    // Return success
                    _errorHandler.ReturnSuccess(_response);
                    return;
                }
            }
        }

        /// <summary>
        /// Processes a RefreshLock request
        /// </summary>
        /// <remarks>
        /// For full documentation on RefreshLock, see
        /// ttps://wopi.readthedocs.io/projects/wopirest/en/latest/files/RefreshLock.html
        /// </remarks>
        public void HandleRefreshLockRequest(WopiRequest requestData)
        {
            if (!_authorization.ValidateAccess(requestData, writeAccessRequired: true))
            {
                _errorHandler.ReturnInvalidToken(_response);
                return;
            }

            if (!File.Exists(requestData.FullPath))
            {
                _errorHandler.ReturnFileUnknown(_response);
                return;
            }

            string newLock = requestData.AccessToken;

            lock (LockInfo.Locks)
            {
                LockInfo existingLock;
                if (LockInfo.TryGetLock(requestData.Id, out existingLock))
                {
                    if (existingLock.Lock == newLock)
                    {
                        // There is a valid lock on the file and the existing lock matches the provided one

                        // Extend the lock timeout
                        existingLock.DateCreated = DateTime.UtcNow;
                        _errorHandler.ReturnSuccess(_response);
                    }
                    else
                    {
                        // The existing lock doesn't match the requested one.  Return a lock mismatch error
                        // along with the current lock
                        _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                    }
                }
                else
                {
                    // The requested lock does not exist.  That's also a lock mismatch error.
                    _errorHandler.ReturnLockMismatch(_response, reason: "File not locked");
                }
            }
        }

        /// <summary>
        /// Processes a Unlock request
        /// </summary>
        /// <remarks>
        /// For full documentation on Unlock, see
        /// https://wopi.readthedocs.io/projects/wopirest/en/latest/files/Unlock.html
        /// </remarks>
        public void HandleUnlockRequest(WopiRequest requestData)
        {
            if (!_authorization.ValidateAccess(requestData, writeAccessRequired: true))
            {
                _errorHandler.ReturnInvalidToken(_response);
                return;
            }

            if (!File.Exists(requestData.FullPath))
            {
                _errorHandler.ReturnFileUnknown(_response);
                return;
            }

            string newLock = requestData.AccessToken;

            lock (LockInfo.Locks)
            {
                LockInfo existingLock;
                if (LockInfo.TryGetLock(requestData.Id, out existingLock))
                {
                    if (existingLock.Lock == newLock)
                    {
                        // There is a valid lock on the file and the existing lock matches the provided one

                        // Remove the current lock
                        LockInfo.Locks.Remove(requestData.Id);
                        //context.Response.AddHeader(WopiHeaders.ItemVersion, GetFileVersion(requestData.FullPath));
                        _errorHandler.ReturnSuccess(_response);
                    }
                    else
                    {
                        // The existing lock doesn't match the requested one.  Return a lock mismatch error
                        // along with the current lock
                        _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                    }
                }
                else
                {
                    // The requested lock does not exist.  That's also a lock mismatch error.
                    _errorHandler.ReturnLockMismatch(_response, reason: "File not locked");
                }
            }
        }

        /// <summary>
        /// Processes a UnlockAndRelock request
        /// </summary>
        /// <remarks>
        /// For full documentation on UnlockAndRelock, see
        /// https://wopi.readthedocs.io/projects/wopirest/en/latest/files/UnlockAndRelock.html
        /// </remarks>
        public void HandleUnlockAndRelockRequest(WopiRequest requestData)
        {
            if (!_authorization.ValidateAccess(requestData, writeAccessRequired: true))
            {
                _errorHandler.ReturnInvalidToken(_response);
                return;
            }

            if (!File.Exists(requestData.FullPath))
            {
                _errorHandler.ReturnFileUnknown(_response);
                return;
            }

            string newLock = requestData.AccessToken;
            string oldLock = requestData.AccessToken;

            lock (LockInfo.Locks)
            {
                LockInfo existingLock;
                if (LockInfo.TryGetLock(requestData.Id, out existingLock))
                {
                    if (existingLock.Lock == oldLock)
                    {
                        // There is a valid lock on the file and the existing lock matches the provided one

                        // Replace the existing lock with the new one
                        LockInfo.Locks[requestData.Id] = new LockInfo() { DateCreated = DateTime.UtcNow, Lock = newLock };
                        //context.Response.Headers[WopiHeaders.OldLock] = newLock;
                        _errorHandler.ReturnSuccess(_response);
                    }
                    else
                    {
                        // The existing lock doesn't match the requested one.  Return a lock mismatch error
                        // along with the current lock
                        _errorHandler.ReturnLockMismatch(_response, existingLock.Lock);
                    }
                }
                else
                {
                    // The requested lock does not exist.  That's also a lock mismatch error.
                    _errorHandler.ReturnLockMismatch(_response, reason: "File not locked");
                }
            }
        }
    }
}

/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    16 Oct 2017
 * 
 * File:    Future.Download.cs
 * Purpose: Shortcut methods for downloading to a Future.
*/

// UnityWebRequest doesn't support loading from JAR files before 2017.1
// Since this is most likely to happen when reading from StreamingAssets
// on Android, we'll disable UnityWebRequest support on Android for prior versions
#if UNITY_5_4_OR_NEWER && (UNITY_EDITOR || !UNITY_ANDROID || UNITY_2017_1_OR_NEWER)
    #define WEB_REQUEST_SUPPORTED
    
    #if UNITY_2017_2_OR_NEWER
        #define USE_SEND_WEB_REQUEST
    #endif // UNITY_2017_2_OR_NEWER

    #if UNITY_2017_1_OR_NEWER
        #define USE_SEPARATE_ERROR_CHECKS
    #endif // UNITY_2017_1_OR_NEWER

    // Uncomment if we ever get a UnityWebRequest equivalent
    // to WWW.GetAudioClipCompressed()
    // #define WEB_REQUEST_AUDIOCLIP_COMPRESSED

#endif // UNITY_5_4_OR_NEWER && (UNITY_EDITOR || !UNITY_ANDROID || UNITY_2017_1_OR_NEWER)

using System;
using System.Collections;
using UnityEngine;
#if WEB_REQUEST_SUPPORTED
using UnityEngine.Networking;
#endif // WEB_REQUEST_SUPPORTED

namespace BeauRoutine
{
    static public partial class Future
    {
        /// <summary>
        /// Download methods. These will create Routines to download data from a URL,
        /// and return a Future for the result of that download.
        /// </summary>
        static public class Download
        {
            #region WWW

            /// <summary>
            /// Downloads from the given WWW and returns a Future for that WWW.
            /// </summary>
            static public Future<WWW> WWW(WWW inWWW, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<WWW>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadWWW(future, inWWW))
                    );
                return future;
            }

            /// <summary>
            /// Downloads from the given URL and returns a Future for the resulting WWW.
            /// </summary>
            static public Future<WWW> WWW(string inURL, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<WWW>();
                try
                {
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadWWW(future, www))
                        );
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadWWW(Future<WWW> inFuture, WWW inWWW)
            {
                using (inWWW)
                {
                    while(!inWWW.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWWW.progress);
                    }

                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        inFuture.Complete(inWWW);
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }

                    // wait two frames to ensure the Future's callbacks have been invoked
                    yield return null;
                    yield return null;
                }
            }

            #endregion

#if WEB_REQUEST_SUPPORTED

            #region UnityWebRequest

            /// <summary>
            /// Downloads from the given UnityWebRequest and returns a Future for that UnityWebRequest.
            /// </summary>
            static public Future<UnityWebRequest> UnityWebRequest(UnityWebRequest inUnityWebRequest, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<UnityWebRequest>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadUnityWebRequest(future, inUnityWebRequest))
                    );
                return future;
            }

            /// <summary>
            /// Downloads from the given URL and returns a Future for the resulting UnityWebRequest.
            /// </summary>
            static public Future<UnityWebRequest> UnityWebRequest(string inURL, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<UnityWebRequest>();
                try
                {
                    UnityWebRequest webRequest = new UnityWebRequest(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadUnityWebRequest(future, webRequest))
                        );
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadUnityWebRequest(Future<UnityWebRequest> inFuture, UnityWebRequest inWebRequest)
            {
                using (inWebRequest)
                {
                    AsyncOperation op = UnityWebRequest_SendWebRequest(inWebRequest);
                    while(!op.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWebRequest.downloadProgress);
                    }
                    if (!UnityWebRequest_IsError(inWebRequest))
                    {
                        inFuture.Complete(inWebRequest);
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWebRequest.error);
                    }

                    // wait two frames to ensure the Future's callbacks have been invoked
                    yield return null;
                    yield return null;
                }
            }

            #endregion

#endif // WEB_REQUEST_SUPPORTED

            #region Text

            /// <summary>
            /// Downloads text from the given WWW and returns a Future for the text.
            /// </summary>
            static public Future<string> Text(WWW inWWW, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<string>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadStringRoutine(future, inWWW))
                    );
                return future;
            }

#if WEB_REQUEST_SUPPORTED
            /// <summary>
            /// Downloads text from the given UnityWebRequest and returns a Future for the text.
            /// </summary>
            static public Future<string> Text(UnityWebRequest inWebRequest, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<string>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadStringRoutine(future, inWebRequest))
                    );
                return future;
            }
#endif // WEB_REQUEST_SUPPORTED

            /// <summary>
            /// Downloads text from the given URL and returns a Future for the text.
            /// </summary>
            static public Future<string> Text(string inURL, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<string>();
                try
                {
#if WEB_REQUEST_SUPPORTED
                    UnityWebRequest webRequest = new UnityWebRequest(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadStringRoutine(future, webRequest))
                    );
#else
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadStringRoutine(future, www))
                        );
#endif // WEB_REQUEST_SUPPORTED
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadStringRoutine(Future<string> inFuture, WWW inWWW)
            {
                using (inWWW)
                {
                    while(!inWWW.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWWW.progress);
                    }
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWWW, "text/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            inFuture.Complete(inWWW.text);
                        }
                        catch (Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }
                }
            }

#if WEB_REQUEST_SUPPORTED
            static private IEnumerator DownloadStringRoutine(Future<string> inFuture, UnityWebRequest inWebRequest)
            {
                using (inWebRequest)
                {
                    var downloadHandler = new DownloadHandlerBuffer();
                    inWebRequest.downloadHandler = downloadHandler;
                    AsyncOperation op = UnityWebRequest_SendWebRequest(inWebRequest);
                    while(!op.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWebRequest.downloadProgress);
                    }
                    if (!UnityWebRequest_IsError(inWebRequest))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWebRequest, "text/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            inFuture.Complete(downloadHandler.text);
                        }
                        catch (Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWebRequest.error);
                    }
                }
            }
#endif // WEB_REQUEST_SUPPORTED

            #endregion

            #region Bytes

            /// <summary>
            /// Downloads bytes from the given WWW and returns a Future for the bytes.
            /// </summary>
            static public Future<byte[]> Bytes(WWW inWWW, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<byte[]>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadBytesRoutine(future, inWWW))
                    );
                return future;
            }

#if WEB_REQUEST_SUPPORTED
            /// <summary>
            /// Downloads bytes from the given UnityWebRequest and returns a Future for the bytes.
            /// </summary>
            static public Future<byte[]> Bytes(UnityWebRequest inWebRequest, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<byte[]>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadBytesRoutine(future, inWebRequest))
                    );
                return future;
            }
#endif // WEB_REQUEST_SUPPORTED

            /// <summary>
            /// Downloads bytes from the given URL and returns a Future for the bytes.
            /// </summary>
            static public Future<byte[]> Bytes(string inURL, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<byte[]>();
                try
                {
#if WEB_REQUEST_SUPPORTED
                    UnityWebRequest webRequest = new UnityWebRequest(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadBytesRoutine(future, webRequest))
                    );
#else
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadBytesRoutine(future, www))
                    );
#endif // WEB_REQUEST_SUPPORTED
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadBytesRoutine(Future<byte[]> inFuture, WWW inWWW)
            {
                using (inWWW)
                {
                    while(!inWWW.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWWW.progress);
                    }
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        try
                        {
                            inFuture.Complete(inWWW.bytes);
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }
                    yield return null;
                }
            }

#if WEB_REQUEST_SUPPORTED
            static private IEnumerator DownloadBytesRoutine(Future<byte[]> inFuture, UnityWebRequest inWebRequest)
            {
                using (inWebRequest)
                {
                    var downloadHandler = new DownloadHandlerBuffer();
                    inWebRequest.downloadHandler = downloadHandler;
                    AsyncOperation op = UnityWebRequest_SendWebRequest(inWebRequest);
                    while(!op.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWebRequest.downloadProgress);
                    }
                    if (!UnityWebRequest_IsError(inWebRequest))
                    {
                        try
                        {
                            inFuture.Complete(downloadHandler.data);
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWebRequest.error);
                    }
                    yield return null;
                }
            }
#endif // WEB_REQUEST_SUPPORTED

            #endregion

            #region Texture

            /// <summary>
            /// Downloads a texture from the given WWW and returns a Future for the texture.
            /// </summary>
            static public Future<Texture2D> Texture(WWW inWWW, bool inbDownloadAsNonReadable = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<Texture2D>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadTextureRoutine(future, inWWW, inbDownloadAsNonReadable))
                    );
                return future;
            }

#if WEB_REQUEST_SUPPORTED
            /// <summary>
            /// Downloads a texture from the given UnityWebRequest and returns a Future for the texture.
            /// </summary>
            static public Future<Texture2D> Texture(UnityWebRequest inWebRequest, bool inbDownloadAsNonReadable = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<Texture2D>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadTextureRoutine(future, inWebRequest, inbDownloadAsNonReadable))
                    );
                return future;
            }
#endif // WEB_REQUEST_SUPPORTED

            /// <summary>
            /// Downloads a texture from the given URL and returns a Future for the texture.
            /// </summary>
            static public Future<Texture2D> Texture(string inURL, bool inbDownloadAsNonReadable = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<Texture2D>();
                try
                {
#if WEB_REQUEST_SUPPORTED
                    UnityWebRequest webRequest = new UnityWebRequest(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadTextureRoutine(future, webRequest, inbDownloadAsNonReadable))
                        );
#else
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadTextureRoutine(future, www, inbDownloadAsNonReadable))
                        );
#endif // WEB_REQUEST_SUPPORTED
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadTextureRoutine(Future<Texture2D> inFuture, WWW inWWW, bool inbNonReadable)
            {
                using (inWWW)
                {
                    while(!inWWW.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWWW.progress);
                    }
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWWW, "image/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            Texture2D texture = inbNonReadable ? inWWW.textureNonReadable : inWWW.texture;
                            if (texture == null)
                                inFuture.Fail(Future.FailureType.Unknown, "Texture is null");
                            else if (!TextureIsValid(texture))
                                inFuture.Fail(Future.FailureType.Unknown, "Not a valid texture");
                            else
                            {
                                texture.name = UnityEngine.WWW.UnEscapeURL(inWWW.url);
                                inFuture.Complete(texture);
                            }
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }
                    yield return null;
                }
            }

#if WEB_REQUEST_SUPPORTED
            static private IEnumerator DownloadTextureRoutine(Future<Texture2D> inFuture, UnityWebRequest inWebRequest, bool inbNonReadable)
            {
                using (inWebRequest)
                {
                    var downloadHandler = new DownloadHandlerTexture(!inbNonReadable);
                    inWebRequest.downloadHandler = downloadHandler;
                    AsyncOperation op = UnityWebRequest_SendWebRequest(inWebRequest);
                    while(!op.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWebRequest.downloadProgress);
                    }
                    if (!UnityWebRequest_IsError(inWebRequest))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWebRequest, "image/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            Texture2D texture = downloadHandler.texture;
                            if (texture == null)
                                inFuture.Fail(Future.FailureType.Unknown, "Texture is null");
                            else if (!TextureIsValid(texture))
                                inFuture.Fail(Future.FailureType.Unknown, "Not a valid texture");
                            else
                            {
                                texture.name = UnityEngine.WWW.UnEscapeURL(inWebRequest.url);
                                inFuture.Complete(texture);
                            }
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWebRequest.error);
                    }
                    yield return null;
                }
            }
#endif // WEB_REQUEST_SUPPORTED

            static private bool TextureIsValid(Texture2D inTexture)
            {
                // TODO(Alex): Replace this with something better?
                // The default texture returned by Unity is 8x8,
                // but if we try downloading an 8x8 texture (no matter how unlikely),
                // this will fail
                return inTexture.width != 8 || inTexture.height != 8;
            }

            #endregion

            #region AudioClip

            /// <summary>
            /// Downloads an AudioClip from the given WWW and returns a Future for the AudioClip.
            /// </summary>
            static public Future<AudioClip> AudioClip(WWW inWWW, bool inbCompressed = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<AudioClip>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadAudioClipRoutine(future, inWWW, inbCompressed))
                    );
                return future;
            }

#if WEB_REQUEST_SUPPORTED
            /// <summary>
            /// Downloads an AudioClip from the given UnityWebRequest and returns a Future for the AudioClip.
            /// </summary>
            static public Future<AudioClip> AudioClip(UnityWebRequest inWebRequest, bool inbCompressed = false, MonoBehaviour inRoutineHost = null)
            {
#if !WEB_REQUEST_AUDIOCLIP_COMPRESSED
                if (inbCompressed)
                {
                    //Debug.LogError("[BeauRoutine.Future] Cannot load a compressed AudioClip through a UnityWebRequest in this version of Unity.");
                    return Future.Failed<AudioClip>(Future.FailureType.Unknown, "Unable to load compressed AudioClips with UnityWebRequest");
                }
#endif // WEB_REQUEST_AUDIOCLIP_COMPRESSED
                var future = Future.Create<AudioClip>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadAudioClipRoutine(future, inWebRequest, inbCompressed))
                    );
                return future;
            }
#endif // WEB_REQUEST_SUPPORTED

            /// <summary>
            /// Downloads an AudioClip from the given URL and returns a Future for the AudioClip.
            /// </summary>
            static public Future<AudioClip> AudioClip(string inURL, bool inbCompressed = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<AudioClip>();
                try
                {
#if WEB_REQUEST_SUPPORTED
    #if !WEB_REQUEST_AUDIOCLIP_COMPRESSED
                    if (!inbCompressed)
    #endif // !WEB_REQUEST_AUDIOCLIP_COMPRESSED
                    {
                        UnityWebRequest webRequest = new UnityWebRequest(inURL);
                        future.LinkTo(
                            Routine.Start(inRoutineHost, DownloadAudioClipRoutine(future, webRequest, inbCompressed))
                        );
                    }
    #if !WEB_REQUEST_AUDIOCLIP_COMPRESSED
                    else
    #endif // !WEB_REQUEST_AUDIOCLIP_COMPRESSED
#endif

#if !WEB_REQUEST_SUPPORTED || !WEB_REQUEST_AUDIOCLIP_COMPRESSED
                    {
                        WWW www = new WWW(inURL);
                        future.LinkTo(
                            Routine.Start(inRoutineHost, DownloadAudioClipRoutine(future, www, inbCompressed))
                            );
                    }
#endif // !WEB_REQUEST_AUDIOCLIP_COMPRESSED
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadAudioClipRoutine(Future<AudioClip> inFuture, WWW inWWW, bool inbCompressed)
            {
                using (inWWW)
                {
                    while(!inWWW.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWWW.progress);
                    }
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWWW, "audio/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            AudioClip audioClip = inbCompressed ? inWWW.GetAudioClipCompressed(false) : inWWW.GetAudioClip(false);
                            if (audioClip == null)
                                inFuture.Fail(Future.FailureType.Unknown, "Clip is null");
                            else
                            {
                                audioClip.name = UnityEngine.WWW.UnEscapeURL(inWWW.url);
                                inFuture.Complete(audioClip);
                            }
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }
                    yield return null;
                }
            }

#if WEB_REQUEST_SUPPORTED
            static private IEnumerator DownloadAudioClipRoutine(Future<AudioClip> inFuture, UnityWebRequest inWebRequest, bool inbCompressed)
            {
                using (inWebRequest)
                {
                    var downloadHandler = new DownloadHandlerAudioClip(inWebRequest.url, GetAudioTypeForURL(inWebRequest.url));
                    inWebRequest.downloadHandler = downloadHandler;
                    AsyncOperation op = UnityWebRequest_SendWebRequest(inWebRequest);
                    while(!op.isDone)
                    {
                        yield return null;
                        inFuture.SetProgress(inWebRequest.downloadProgress);
                    }
                    if (!UnityWebRequest_IsError(inWebRequest))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWebRequest, "audio/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            AudioClip audioClip = downloadHandler.audioClip;
                            if (audioClip == null)
                                inFuture.Fail(Future.FailureType.Unknown, "Clip is null");
                            else
                            {
                                audioClip.name = UnityEngine.WWW.UnEscapeURL(inWebRequest.url);
                                inFuture.Complete(audioClip);
                            }
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWebRequest.error);
                    }
                    yield return null;
                }
            }
#endif // WEB_REQUEST_SUPPORTED

            static private AudioType GetAudioTypeForURL(string inURL)
            {
                string extension = System.IO.Path.GetExtension(inURL);

                // Most common types here
                if (extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                    return AudioType.MPEG;
                if (extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase))
                    return AudioType.OGGVORBIS;
                if (extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
                    return AudioType.WAV;

                if (extension.Equals(".acc", StringComparison.OrdinalIgnoreCase))
                    return AudioType.ACC;
                if (extension.Equals(".aiff", StringComparison.OrdinalIgnoreCase))
                    return AudioType.AIFF;
                if (extension.Equals(".it", StringComparison.OrdinalIgnoreCase))
                    return AudioType.IT;
                if (extension.Equals(".mod", StringComparison.OrdinalIgnoreCase))
                    return AudioType.MOD;
                if (extension.Equals(".mp2", StringComparison.OrdinalIgnoreCase))
                    return AudioType.MPEG;
                if (extension.Equals(".s3m", StringComparison.OrdinalIgnoreCase))
                    return AudioType.S3M;
                if (extension.Equals(".xm", StringComparison.OrdinalIgnoreCase))
                    return AudioType.XM;
                if (extension.Equals(".xma", StringComparison.OrdinalIgnoreCase))
                    return AudioType.XMA;
                if (extension.Equals(".vag", StringComparison.OrdinalIgnoreCase))
                    return AudioType.VAG;

#if UNITY_IOS && !UNITY_EDITOR
                return AudioType.AUDIOQUEUE;
#else
                return AudioType.UNKNOWN;
#endif // UNITY_IOS && !UNITY_EDITOR
            }

#endregion

            // Returns whether it could validate the MIME type of the returned content.
            static private bool TryValidateMIMEType(WWW inWWW, string inMIMEType, out bool outbValidated)
            {
                outbValidated = true;
                
                try
                {
                    string contentType = string.Empty;
                    bool bFoundContentType = inWWW.responseHeaders != null && inWWW.responseHeaders.TryGetValue("Content-Type", out contentType);
                    if (bFoundContentType)
                    {
                        if (!contentType.StartsWith(inMIMEType))
                        {
                            outbValidated = false;
                        }

                        return true;
                    }
                }
                catch (Exception) { }

                return false;
            }

#if WEB_REQUEST_SUPPORTED

            // Returns whether it could validate the MIME type of the returned content.
            static private bool TryValidateMIMEType(UnityWebRequest inWebRequest, string inMIMEType, out bool outbValidated)
            {
                outbValidated = true;
                
                try
                {
                    string contentType = inWebRequest.GetResponseHeader("Content-Type");;
                    if (contentType != null)
                    {
                        if (!contentType.StartsWith(inMIMEType))
                        {
                            outbValidated = false;
                        }

                        return true;
                    }
                }
                catch (Exception) { }

                return false;
            }

            static private AsyncOperation UnityWebRequest_SendWebRequest(UnityWebRequest inWebRequest)
            {
    #if USE_SEND_WEB_REQUEST
                return inWebRequest.SendWebRequest();
    #else
                return inWebRequest.Send();
    #endif // USE_SEND_WEB_REQUEST
            }

            static private bool UnityWebRequest_IsError(UnityWebRequest inWebRequest)
            {
    #if USE_SEPARATE_ERROR_CHECKS
                return inWebRequest.isNetworkError || inWebRequest.isHttpError;
    #else
                return inWebRequest.isError;
    #endif // USE_SEPARATE_ERROR_CHECKS
            }

#endif // WEB_REQUEST_SUPPORTED
            }
        }
}
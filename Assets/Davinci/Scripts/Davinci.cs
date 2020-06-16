﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Davinci - A powerful, esay-to-use image downloading and caching library for Unity
/// v 1.1 beta
/// Developed by ShamsDEV.com
/// copyright (c) ShamsDEV.com All Rights Reserved.
/// Licensed under the MIT License.
/// https://github.com/shamsdev/davinci
/// </summary>
public class Davinci : MonoBehaviour
{
    private bool enableLog = false;
    private float fadeTime = 1;
    private bool cached = true;

    private enum RendererType
    {
        none,
        uiImage,
        renderer
    }

    private RendererType rendererType = RendererType.none;
    private GameObject targetObj;
    private string url = null;

    private Texture2D loadingPlaceholder, errorPlaceholder;

    private UnityAction onStartAction,
        onDownloadedAction,
        OnLoadedAction,
        onEndAction;

    private UnityAction<int> onDownloadProgressChange;
    private UnityAction<string> onErrorAction;

    private static Dictionary<string, Davinci> underProcessDavincies
        = new Dictionary<string, Davinci>();

    private string uniqueHash;
    private int progress;

    private bool success = false;

    static string filePath = Application.persistentDataPath + "/" +
             "davinci" + "/";


    /// <summary>
    /// Get instance of davinci class
    /// </summary>
    public static Davinci get()
    {
        return new GameObject("Davinci").AddComponent<Davinci>();
    }

    /// <summary>
    /// Set image url for download.
    /// </summary>
    /// <param name="url">Image Url</param>
    /// <returns></returns>
    public Davinci load(string url)
    {
        if (enableLog)
            Debug.Log("[Davinci] Url set : " + url);

        this.url = url;
        return this;
    }

    /// <summary>
    /// Set fading animation time.
    /// </summary>
    /// <param name="fadeTime">Fade animation time. Set 0 for disable fading.</param>
    /// <returns></returns>
    public Davinci setFadeTime(float fadeTime)
    {
        if (enableLog)
            Debug.Log("[Davinci] Fading time set : " + fadeTime);

        this.fadeTime = fadeTime;
        return this;
    }

    /// <summary>
    /// Set target Image component.
    /// </summary>
    /// <param name="image">target Unity UI image component</param>
    /// <returns></returns>
    public Davinci into(Image image)
    {
        if (enableLog)
            Debug.Log("[Davinci] Target as UIImage set : " + image);

        rendererType = RendererType.uiImage;
        this.targetObj = image.gameObject;
        return this;
    }

    /// <summary>
    /// Set target Renderer component.
    /// </summary>
    /// <param name="renderer">target renderer component</param>
    /// <returns></returns>
    public Davinci into(Renderer renderer)
    {
        if (enableLog)
            Debug.Log("[Davinci] Target as Renderer set : " + renderer);

        rendererType = RendererType.renderer;
        this.targetObj = renderer.gameObject;
        return this;
    }

    #region Actions
    public Davinci withStartAction(UnityAction action)
    {
        this.onStartAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On start action set : " + action);

        return this;
    }

    public Davinci withDownloadedAction(UnityAction action)
    {
        this.onDownloadedAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On downloaded action set : " + action);

        return this;
    }

    public Davinci withDownloadProgressChangedAction(UnityAction<int> action)
    {
        this.onDownloadProgressChange = action;

        if (enableLog)
            Debug.Log("[Davinci] On download progress changed action set : " + action);

        return this;
    }

    public Davinci withLoadedAction(UnityAction action)
    {
        this.OnLoadedAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On loaded action set : " + action);

        return this;
    }

    public Davinci withErrorAction(UnityAction<string> action)
    {
        this.onErrorAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On error action set : " + action);

        return this;
    }

    public Davinci withEndAction(UnityAction action)
    {
        this.onEndAction = action;

        if (enableLog)
            Debug.Log("[Davinci] On end action set : " + action);

        return this;
    }
    #endregion

    /// <summary>
    /// Show or hide logs in console.
    /// </summary>
    /// <param name="enable">'true' for show logs in console.</param>
    /// <returns></returns>
    public Davinci setEnableLog(bool enableLog)
    {
        this.enableLog = enableLog;

        if (enableLog)
            Debug.Log("[Davinci] Logging enabled : " + enableLog);

        return this;
    }

    /// <summary>
    /// Set the sprite of image when davinci is downloading and loading image
    /// </summary>
    /// <param name="loadingPlaceholder">loading texture</param>
    /// <returns></returns>
    public Davinci setLoadingPlaceholder(Texture2D loadingPlaceholder)
    {
        this.loadingPlaceholder = loadingPlaceholder;

        if (enableLog)
            Debug.Log("[Davinci] Loading placeholder has been set.");

        return this;
    }

    /// <summary>
    /// Set image sprite when some error occurred during downloading or loading image
    /// </summary>
    /// <param name="errorPlaceholder">error texture</param>
    /// <returns></returns>
    public Davinci setErrorPlaceholder(Texture2D errorPlaceholder)
    {
        this.errorPlaceholder = errorPlaceholder;

        if (enableLog)
            Debug.Log("[Davinci] Error placeholder has been set.");

        return this;
    }

    /// <summary>
    /// Enable cache
    /// </summary>
    /// <returns></returns>
    public Davinci setCached(bool cached)
    {
        this.cached = cached;

        if (enableLog)
            Debug.Log("[Davinci] Cache enabled : " + cached);

        return this;
    }

    /// <summary>
    /// Start davinci process.
    /// </summary>
    public void start()
    {
        if (url == null)
        {
            error("Url has not been set. Use 'load' funtion to set image url.");
            return;
        }

        try
        {
            Uri uri = new Uri(url);
            this.url = uri.AbsoluteUri;
        }
        catch (Exception ex)
        {
            error("Url is not correct.");
            return;
        }

        if (rendererType == RendererType.none || targetObj == null)
        {
            error("Target has not been set. Use 'into' function to set target component.");
            return;
        }

        if (enableLog)
            Debug.Log("[Davinci] Start Working.");

        if (loadingPlaceholder != null)
            SetLoadingImage();

        if (onStartAction != null)
            onStartAction.Invoke();

        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        uniqueHash = CreateMD5(url);

        if (underProcessDavincies.ContainsKey(uniqueHash))
        {
            Davinci sameProcess = underProcessDavincies[uniqueHash];
            sameProcess.onDownloadedAction += () =>
            {
                if (onDownloadedAction != null)
                    onDownloadedAction.Invoke();

                loadSpriteToImage();
            };
        }
        else
        {
            if (File.Exists(filePath + uniqueHash))
            {
                if (onDownloadedAction != null)
                    onDownloadedAction.Invoke();

                loadSpriteToImage();
            }
            else
            {
                underProcessDavincies.Add(uniqueHash, this);
                StopAllCoroutines();
                StartCoroutine("Downloader");
            }
        }
    }

    private IEnumerator Downloader()
    {
        if (enableLog)
            Debug.Log("[Davinci] Download started.");

        var www = new WWW(url);

        while (!www.isDone)
        {
            if (www.error != null)
            {
                error("Error while downloading the image : " + www.error);
                yield break;
            }

            progress = Mathf.FloorToInt(www.progress * 100);
            if (onDownloadProgressChange != null)
                onDownloadProgressChange.Invoke(progress);

            if (enableLog)
                Debug.Log("[Davinci] Downloading progress : " + progress + "%");

            yield return null;
        }

        if (www.error == null)
            File.WriteAllBytes(filePath + uniqueHash, www.bytes);

        www.Dispose();
        www = null;

        if (onDownloadedAction != null)
            onDownloadedAction.Invoke();

        loadSpriteToImage();

        underProcessDavincies.Remove(uniqueHash);
    }

    private void loadSpriteToImage()
    {
        progress = 100;
        if (onDownloadProgressChange != null)
            onDownloadProgressChange.Invoke(progress);

        if (enableLog)
            Debug.Log("[Davinci] Downloading progress : " + progress + "%");

        if (!File.Exists(filePath + uniqueHash))
        {
            error("Loading image file has been failed.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ImageLoader());
    }

    private void SetLoadingImage()
    {
        switch (rendererType)
        {
            case RendererType.renderer:
                Renderer renderer = targetObj.GetComponent<Renderer>();
                renderer.material.mainTexture = loadingPlaceholder;
                break;

            case RendererType.uiImage:
                Image image = targetObj.GetComponent<Image>();
                Sprite sprite = Sprite.Create(loadingPlaceholder,
                     new Rect(0, 0, loadingPlaceholder.width, loadingPlaceholder.height),
                     new Vector2(0.5f, 0.5f));
                image.sprite = sprite;

                break;
        }

    }

    private IEnumerator ImageLoader(Texture2D texture = null)
    {
        if (enableLog)
            Debug.Log("[Davinci] Start loading image.");

        if (texture == null)
        {
            byte[] fileData;
            fileData = File.ReadAllBytes(filePath + uniqueHash);
            texture = new Texture2D(2, 2);
            //ImageConversion.LoadImage(texture, fileData);
            texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        Color color;
        switch (rendererType)
        {
            case RendererType.renderer:
                Renderer renderer = targetObj.GetComponent<Renderer>();
                renderer.material.mainTexture = texture;
                color = renderer.material.color;
                float maxAlpha = color.a;

                if (fadeTime > 0)
                {
                    color.a = 0;
                    renderer.material.color = color;

                    float time = Time.time;
                    while (color.a < maxAlpha)
                    {
                        color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);
                        renderer.material.color = color;
                        yield return null;
                    }
                }

                break;

            case RendererType.uiImage:
                Image image = targetObj.GetComponent<Image>();
                Sprite sprite = Sprite.Create(texture,
                     new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
                color = image.color;
                maxAlpha = color.a;

                if (fadeTime > 0)
                {
                    color.a = 0;
                    image.color = color;

                    float time = Time.time;
                    while (color.a < maxAlpha)
                    {
                        color.a = Mathf.Lerp(0, maxAlpha, (Time.time - time) / fadeTime);
                        image.color = color;
                        yield return null;
                    }
                }
                break;
        }

        if (OnLoadedAction != null)
            OnLoadedAction.Invoke();

        if (enableLog)
            Debug.Log("[Davinci] Image has been loaded.");

        success = true;
        finish();
    }

    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }

    private void error(string message)
    {
        success = false;

        if (enableLog)
            Debug.LogError("[Davinci] Error : " + message);

        if (onErrorAction != null)
            onErrorAction.Invoke(message);

        if (errorPlaceholder != null)
            StartCoroutine(ImageLoader(errorPlaceholder));
        else finish();
    }

    private void finish()
    {
        if (enableLog)
            Debug.Log("[Davinci] Operation has been finished.");

        if (!cached)
            File.Delete(filePath + uniqueHash);

        if (onEndAction != null)
            onEndAction.Invoke();

        Invoke("destroyer", 0.5f);
    }

    private void destroyer()
    {
        Destroy(gameObject);
    }
}

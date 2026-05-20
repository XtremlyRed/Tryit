using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Tryit;

/// <summary>
/// Represents a folder with various predefined special folders. It allows combining paths and creating directories if
/// they don't exist.
/// </summary>
[DebuggerDisplay("{folder}")]
public readonly struct Folder : IEquatable<object>, IEquatable<Folder>
{
    /// <summary>
    /// <see cref="Environment.SpecialFolder.Desktop"/> folder
    /// </summary>
    public static readonly Folder Desktop = new(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Programs"/> folder
    /// </summary>
    public static readonly Folder Programs = new(Environment.GetFolderPath(Environment.SpecialFolder.Programs));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.MyDocuments"/> folder
    /// </summary>
    public static readonly Folder MyDocuments = new(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Favorites"/> folder
    /// </summary>
    public static readonly Folder Favorites = new(Environment.GetFolderPath(Environment.SpecialFolder.Favorites));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Startup"/> folder
    /// </summary>
    public static readonly Folder Startup = new(Environment.GetFolderPath(Environment.SpecialFolder.Startup));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Recent"/> folder
    /// </summary>
    public static readonly Folder Recent = new(Environment.GetFolderPath(Environment.SpecialFolder.Recent));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.SendTo"/> folder
    /// </summary>
    public static readonly Folder SendTo = new(Environment.GetFolderPath(Environment.SpecialFolder.SendTo));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.StartMenu"/> folder
    /// </summary>
    public static readonly Folder StartMenu = new(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.MyMusic"/> folder
    /// </summary>
    public static readonly Folder MyMusic = new(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.MyVideos"/> folder
    /// </summary>
    public static readonly Folder MyVideos = new(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.DesktopDirectory"/> folder
    /// </summary>
    public static readonly Folder DesktopDirectory = new(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.MyComputer"/> folder
    /// </summary>
    public static readonly Folder MyComputer = new(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.NetworkShortcuts"/> folder
    /// </summary>
    public static readonly Folder NetworkShortcuts = new(Environment.GetFolderPath(Environment.SpecialFolder.NetworkShortcuts));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Fonts"/> folder
    /// </summary>
    public static readonly Folder Fonts = new(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Templates"/> folder
    /// </summary>
    public static readonly Folder Templates = new(Environment.GetFolderPath(Environment.SpecialFolder.Templates));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonStartMenu"/> folder
    /// </summary>
    public static readonly Folder CommonStartMenu = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonPrograms"/> folder
    /// </summary>
    public static readonly Folder CommonPrograms = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonStartup"/> folder
    /// </summary>
    public static readonly Folder CommonStartup = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonDesktopDirectory"/> folder
    /// </summary>
    public static readonly Folder CommonDesktopDirectory = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.ApplicationData"/> folder
    /// </summary>
    public static readonly Folder ApplicationData = new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.PrinterShortcuts"/> folder
    /// </summary>
    public static readonly Folder PrinterShortcuts = new(Environment.GetFolderPath(Environment.SpecialFolder.PrinterShortcuts));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.LocalApplicationData"/> folder
    /// </summary>
    public static readonly Folder LocalApplicationData = new(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.InternetCache"/> folder
    /// </summary>
    public static readonly Folder InternetCache = new(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Cookies"/> folder
    /// </summary>
    public static readonly Folder Cookies = new(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.History"/> folder
    /// </summary>
    public static readonly Folder History = new(Environment.GetFolderPath(Environment.SpecialFolder.History));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonApplicationData"/> folder
    /// </summary>
    public static readonly Folder CommonApplicationData = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Windows"/> folder
    /// </summary>
    public static readonly Folder Windows = new(Environment.GetFolderPath(Environment.SpecialFolder.Windows));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.System"/> folder
    /// </summary>
    public static readonly Folder System = new(Environment.GetFolderPath(Environment.SpecialFolder.System));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.ProgramFiles"/> folder
    /// </summary>
    public static readonly Folder ProgramFiles = new(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.MyPictures"/> folder
    /// </summary>
    public static readonly Folder MyPictures = new(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.UserProfile"/> folder
    /// </summary>
    public static readonly Folder UserProfile = new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.SystemX86"/> folder
    /// </summary>
    public static readonly Folder SystemX86 = new(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.ProgramFilesX86"/> folder
    /// </summary>
    public static readonly Folder ProgramFilesX86 = new(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonProgramFiles"/> folder
    /// </summary>
    public static readonly Folder CommonProgramFiles = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonProgramFilesX86"/> folder
    /// </summary>
    public static readonly Folder CommonProgramFilesX86 = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonTemplates"/> folder
    /// </summary>
    public static readonly Folder CommonTemplates = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonTemplates));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonDocuments"/> folder
    /// </summary>
    public static readonly Folder CommonDocuments = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonAdminTools"/> folder
    /// </summary>
    public static readonly Folder CommonAdminTools = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonAdminTools));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.AdminTools"/> folder
    /// </summary>
    public static readonly Folder AdminTools = new(Environment.GetFolderPath(Environment.SpecialFolder.AdminTools));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonMusic"/> folder
    /// </summary>
    public static readonly Folder CommonMusic = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonPictures"/> folder
    /// </summary>
    public static readonly Folder CommonPictures = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonVideos"/> folder
    /// </summary>
    public static readonly Folder CommonVideos = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.Resources"/> folder
    /// </summary>
    public static readonly Folder Resources = new(Environment.GetFolderPath(Environment.SpecialFolder.Resources));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.LocalizedResources"/> folder
    /// </summary>
    public static readonly Folder LocalizedResources = new(Environment.GetFolderPath(Environment.SpecialFolder.LocalizedResources));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CommonOemLinks"/> folder
    /// </summary>
    public static readonly Folder CommonOemLinks = new(Environment.GetFolderPath(Environment.SpecialFolder.CommonOemLinks));

    /// <summary>
    /// <see cref="Environment.SpecialFolder.CDBurning"/> folder
    /// </summary>
    public static readonly Folder CDBurning = new(Environment.GetFolderPath(Environment.SpecialFolder.CDBurning));

    /// <summary>
    /// <see cref="Environment.CurrentDirectory"/> folder
    /// </summary>
    public static readonly Folder Current = new(Environment.CurrentDirectory);

    /// <summary>
    /// auto create folder when not exist
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool AutoCreateFolder = true;

    /// <summary>
    /// share a new <see cref="Folder"/> folder
    /// </summary>
    private Folder(string folder)
    {
        this.folder = folder;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string folder;

    /// <summary>
    /// combines four strings into a path.
    /// </summary>
    /// <param name="paths">an array of parts of the path.</param>
    /// <Exception cref="System.ArgumentException">one of the strings in the array contains one or more of the invalid characters defined in System.IO.Path.GetInvalidPathChars.</Exception>
    /// <Exception cref="System.ArgumentNullException">one of the strings in the array is null.</Exception>
    /// <returns>The combined paths.</returns>
    public Folder Combine(params string[] paths)
    {
        if (paths is null || paths.Length == 0)
        {
            throw new ArgumentNullException(nameof(paths));
        }

        string[] newPaths = [folder, .. paths];

        return new Folder(Path.Combine(newPaths));
    }

    /// <summary>
    /// Combines one or more path segments into a single folder path.
    /// </summary>
    /// <remarks>The method uses System.IO.Path.Combine to join the provided path segments. All segments are
    /// combined in the order they appear in the array.</remarks>
    /// <param name="paths">An array of path segments to combine. Each segment represents a part of the folder path.</param>
    /// <returns>A Folder instance representing the combined path created from the specified segments.</returns>
    /// <exception cref="ArgumentNullException">Thrown if paths is null or contains no elements.</exception>
    public static Folder Combines(params string[] paths)
    {
        if (paths is null || paths.Length == 0)
        {
            throw new ArgumentNullException(nameof(paths));
        }

        return new Folder(Path.Combine(paths));
    }

    /// <summary>
    /// get folder string from <paramref name="folder"/>
    /// </summary>
    /// <param name="folder"></param>
    public static implicit operator string(Folder folder)
    {
        if (AutoCreateFolder)
        {
            if (Directory.Exists(folder) == false)
            {
                Directory.CreateDirectory(folder);
            }
        }
        return folder.folder;
    }

    /// <summary>
    /// create <see cref="Folder"/> from string
    /// </summary>
    /// <param name="path"></param>
    public static implicit operator Folder(string path)
    {
        return new Folder(path);
    }

    /// <summary>
    /// get <see cref="Folder"/> hash code
    /// </summary>
    /// <returns>hash code</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
    {
        return folder.ToLower().GetHashCode();
    }

    /// <summary>
    /// compare two objects for equality
    /// </summary>
    /// <param name="obj">compare object</param>
    /// <returns>compare result</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        return obj is Folder easyFolder && string.Compare(easyFolder.folder, folder, true) == 0;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString()
    {
        return this;
    }

    /// <summary>
    /// compare two objects for equality
    /// </summary>
    /// <param name="obj">compare object</param>
    /// <returns>compare result</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool Equals(Folder obj)
    {
        return string.Compare(obj.folder, folder, true) == 0;
    }

    /// <summary>
    /// compare two objects for equality
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Folder left, Folder right)
    {
        return string.Compare(left.folder, right.folder, true) == 0;
    }

    /// <summary>
    /// compare two objects for not equality
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Folder left, Folder right)
    {
        return string.Compare(left.folder, right.folder, true) != 0;
    }
}

#region usings

using System;

#endregion

namespace Assets.PsdToUnity.Editor.FullSerializer.Aot
{
    /// <summary>
    ///     Interface that AOT generated converters extend. Used to check to see if
    ///     the AOT converter is up to date.
    /// </summary>
    public interface IFsIAotConverter
    {
        Type ModelType { get; }
        FsAotVersionInfo VersionInfo { get; }
    }
}
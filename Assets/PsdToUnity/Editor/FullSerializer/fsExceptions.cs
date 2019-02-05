// note: This file contains exceptions used by FullSerializer. Exceptions are
//       never used at runtime in FullSerializer; they are only used when
//       validating annotations and code-based models.

#region usings

using System;

#endregion

namespace Assets.PsdToUnity.Editor.FullSerializer
{
    public sealed class FsMissingVersionConstructorException : Exception
    {
        public FsMissingVersionConstructorException(Type versionedType, Type constructorType) :
            base(versionedType + " is missing a constructor for previous model type " + constructorType)
        {
        }
    }

    public sealed class FsDuplicateVersionNameException : Exception
    {
        public FsDuplicateVersionNameException(Type typeA, Type typeB, string version) :
            base(typeA + " and " + typeB + " have the same version string (" + version +
                 "); please change one of them.")
        {
        }
    }
}